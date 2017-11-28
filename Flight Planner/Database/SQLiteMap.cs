using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Concurrent;

namespace CIOSDigital.FlightPlanner.Database
{
    public class SQLiteMap : IMapProvider
    {
        private const string API_KEY = "AIzaSyAqKdnHyxbEm1dFI6xX5Lx0TgOEbRuJ2CE";
        private const uint MAX_DOWNLOADS = 8;

        private static SQLiteMap instance = null;
        public static SQLiteMap Instance {
            get {
                instance = instance ?? new SQLiteMap();
                return instance;
            }
        }

        private SQLiteConnection DbConnection { get; }
        
        private List<TileSpecifier> SleepingDownloads { get; }
        private HashSet<TileSpecifier> CurrentDownloads { get; }
        private Dictionary<TileSpecifier, Task<ImageSource>> Cache { get; }

        private SQLiteMap()
        {
            SleepingDownloads = new List<TileSpecifier>();
            CurrentDownloads = new HashSet<TileSpecifier>();
            Cache = new Dictionary<TileSpecifier, Task<ImageSource>>();

            DbConnection = OpenDbConnection();
            InitializeDBTable();
        }

        private bool IsTileNext(TileSpecifier specifier)
        {
            lock (SleepingDownloads)
            {
                return SleepingDownloads.Last() == specifier;
            }
        }

        private void EnqueueTile(TileSpecifier specifier)
        {
            lock (SleepingDownloads)
            {
                SleepingDownloads.Add(specifier);
            }
        }

        private void RequeueTile(TileSpecifier specifier)
        {
            lock (SleepingDownloads)
            {
                if (SleepingDownloads.Contains(specifier))
                {
                    SleepingDownloads.Remove(specifier);
                    SleepingDownloads.Add(specifier);
                }
            }
        }

        private void DequeueTile(TileSpecifier specifier)
        {
            lock (SleepingDownloads)
            {
                if (SleepingDownloads.Contains(specifier))
                {
                    SleepingDownloads.Remove(specifier);
                }
            }
        }

        private int GetCountDownloading()
        {
            lock (CurrentDownloads)
            {
                return CurrentDownloads.Count;
            }
        }

        private void SetTileDownloading(TileSpecifier specifier)
        {
            lock (CurrentDownloads)
            {
                CurrentDownloads.Add(specifier);
            }
        }

        private void SetTileFinishedDownloading(TileSpecifier specifier)
        {
            lock (CurrentDownloads)
            {
                CurrentDownloads.Remove(specifier);
            }
        }

        private static SQLiteConnection OpenDbConnection()
        {
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            const string filePath = "cios-digital/mapdb.sqlite3";
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string canonicalPath = Path.Combine(folderPath, filePath);
            builder.Uri = "file://" + canonicalPath;
            Directory.CreateDirectory(Path.GetDirectoryName(canonicalPath));
            return new SQLiteConnection(builder.ToString()).OpenAndReturn();
        }

        private void InitializeDBTable()
        {
            using (SQLiteCommand command = new SQLiteCommand(SqlStrings.CREATE_TABLE, DbConnection))
            {
                command.ExecuteNonQuery();
            }
        }

        public async Task<ImageSource> GetImageAsync(TileSpecifier specifier)
        {
            if (!specifier.IsValidCoordinate())
            {
                return null;
            }

            if (Cache.ContainsKey(specifier))
            {
                RequeueTile(specifier);
                return await Cache[specifier];
            }

            Task<ImageSource> task = GetImageAsyncImpl(specifier);
            Cache.Add(specifier, task);
            return await task;
        }
        
        private async Task<ImageSource> GetImageAsyncImpl(TileSpecifier specifier)
        {
            byte[] image = await GetCachedImageAsync(specifier);
            if (image == null)
            {
                EnqueueTile(specifier);
                while (GetCountDownloading() >= MAX_DOWNLOADS && IsTileNext(specifier))
                {
                    await Task.Delay(new TimeSpan(10000));
                }
                DequeueTile(specifier);
                SetTileDownloading(specifier);
                Task<byte[]> downloadedImage = DownloadImageAsync(specifier);
                CacheImageAsync(specifier, await downloadedImage);
                image = await downloadedImage;
                SetTileFinishedDownloading(specifier);
            }

            MemoryStream imageStream = new MemoryStream(image);
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = imageStream;
            bitmap.EndInit();
            return bitmap;
        }

        private async Task<byte[]> DownloadImageAsync(TileSpecifier spec)
        {
            StringBuilder uriBuilder = new StringBuilder("https://maps.googleapis.com/maps/api/staticmap");
            uriBuilder.AppendFormat("?key={0}", API_KEY);
            uriBuilder.AppendFormat("&center={0},{1}", spec.Coordinate.Latitude, spec.Coordinate.Longitude);
            uriBuilder.AppendFormat("&maptype={0}", spec.MapType.ToString().ToLower());
            uriBuilder.AppendFormat("&size={0}x{1}", spec.Size.Width, spec.Size.Height);
            uriBuilder.AppendFormat("&zoom={0}", spec.Zoom);
            var client = new WebClient();
            var data = client.DownloadDataTaskAsync(uriBuilder.ToString());
            return await data;
        }

        private async Task<byte[]> GetCachedImageAsync(TileSpecifier spec)
        {
            byte[] buffer = null;
            using (SQLiteCommand command = new SQLiteCommand(SqlStrings.GET_IMAGE, DbConnection))
            {
                command.Parameters.AddWithValue("@latitude", spec.Coordinate.Latitude);
                command.Parameters.AddWithValue("@longitude", spec.Coordinate.Longitude);
                command.Parameters.AddWithValue("@width", spec.Size.Width);
                command.Parameters.AddWithValue("@height", spec.Size.Height);
                command.Parameters.AddWithValue("@zoom", spec.Zoom);
                command.Parameters.AddWithValue("@mapType", spec.MapType);
                using (SQLiteDataReader reader = await Task.Run(() => command.ExecuteReader()))
                {
                    if (reader.Read())
                    {
                        int length = reader.GetInt32(1);
                        buffer = new byte[length];
                        reader.GetBytes(0, 0, buffer, 0, length);
                    }
                }
            }
            return buffer;
        }

        private void CacheImageAsync(TileSpecifier spec, byte[] image)
        {
            Task ignore = Task.Run(() =>
            {
                using (SQLiteCommand command = new SQLiteCommand(SqlStrings.WRITE_IMAGE, DbConnection))
                {
                    command.Parameters.AddWithValue("@latitude", spec.Coordinate.Latitude);
                    command.Parameters.AddWithValue("@longitude", spec.Coordinate.Longitude);
                    command.Parameters.AddWithValue("@width", spec.Size.Width);
                    command.Parameters.AddWithValue("@height", spec.Size.Height);
                    command.Parameters.AddWithValue("@zoom", spec.Zoom);
                    command.Parameters.AddWithValue("@mapType", spec.MapType);
                    command.Parameters.AddWithValue("@pngData", image);
                    command.Parameters.AddWithValue("@pngDataLen", image.Length);
                    command.ExecuteNonQuery();
                }
            });
        }

        private static class SqlStrings
        {
            public const string CREATE_TABLE = ""
                + "CREATE TABLE IF NOT EXISTS Images ("
                + "    Latitude  REAL NOT NULL,"
                + "    Longitude REAL NOT NULL,"
                + "    Width  INTEGER NOT NULL,"
                + "    Height INTEGER NOT NULL,"
                + "    Zoom REAL NOT NULL,"
                + "    MapType TEXT NOT NULL,"
                + "    PngData    BLOB NOT NULL,"
                + "    PngDataLen INTEGER NOT NULL,"
                + "    PRIMARY KEY(Latitude, Longitude, Width, Height, Zoom, MapType)"
                + ");";

            public const string WRITE_IMAGE = ""
                + "INSERT OR REPLACE INTO Images VALUES ("
                + "    @latitude, @longitude,"
                + "    @width, @height,"
                + "    @zoom, @mapType,"
                + "    @pngData, @pngDataLen"
                + ");";

            public const string GET_IMAGE = ""
                + "SELECT PngData, PngDataLen FROM Images"
                + "    WHERE Latitude  = @latitude"
                + "      AND Longitude = @longitude"
                + "      AND Width     = @width"
                + "      AND Height    = @height"
                + "      AND Zoom      = @zoom"
                + "      AND MapType   = @mapType;";
        }
    }
}
