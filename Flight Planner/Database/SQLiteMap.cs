using System;
using System.Data.SQLite;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CIOSDigital.FlightPlanner.Database
{
    public class SQLiteMap : MapProvider
    {
        private const string API_KEY = "AIzaSyAqKdnHyxbEm1dFI6xX5Lx0TgOEbRuJ2CE";

        private SQLiteConnection Connection { get; }

        private SQLiteMap()
        {
            Connection = OpenAppDataDBConnection();
            InitializeDBTable();
        }

        public static SQLiteMap OpenDB()
        {
            return new SQLiteMap();
        }

        private static SQLiteConnection OpenAppDataDBConnection()
        {
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            const string filePath = "cios-digital/mapdb.sqlite3";
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string canonicalPath = Path.Combine(folderPath, filePath);
            builder.Uri = "file://" + canonicalPath;
            Directory.CreateDirectory(Path.GetDirectoryName(canonicalPath));
            return new SQLiteConnection(builder.ToString()).OpenAndReturn();
        }

        private void InitializeDBTable()
        {
            const string sql = ""
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
            lock (Connection)
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, Connection))
                {
                    new SQLiteCommand("BEGIN TRANSACTION;", Connection).ExecuteNonQuery();
                    command.ExecuteNonQuery();
                    new SQLiteCommand("END TRANSACTION;", Connection).ExecuteNonQuery();
                }
            }
        }

        public async Task<ImageSource> GetImageAsync(MapImageSpec spec)
        {
            if (Math.Abs(spec.Coordinate.Latitude) > 70 || Math.Abs(spec.Coordinate.Longitude) > 180)
            {
                return null;
            }

            byte[] image = await GetCachedImageAsync(spec);
            if (image == null)
            {
                Task<byte[]> downloadedImage = DownloadImageAsync(spec);
                CacheImageAsync(spec, await downloadedImage);
                image = await downloadedImage;
            }

            MemoryStream imageStream = new MemoryStream(image);
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = imageStream;
            bitmap.EndInit();
            return bitmap;
        }

        private async Task<byte[]> DownloadImageAsync(MapImageSpec spec)
        {
            StringBuilder uriBuilder = new StringBuilder("https://maps.googleapis.com/maps/api/staticmap");
            uriBuilder.AppendFormat("?key={0}", API_KEY);
            uriBuilder.AppendFormat("&center={0},{1}", spec.Coordinate.Latitude, spec.Coordinate.Longitude);
            uriBuilder.AppendFormat("&maptype={0}", spec.MapType.ToString().ToLower());
            uriBuilder.AppendFormat("&size={0}x{1}", spec.Size.Width, spec.Size.Height);
            uriBuilder.AppendFormat("&zoom={0}", spec.Zoom);
            return await new WebClient().DownloadDataTaskAsync(uriBuilder.ToString());
        }

        private async Task<byte[]> GetCachedImageAsync(MapImageSpec spec)
        {
            const string sql = ""
                + "SELECT PngData, PngDataLen FROM Images"
                + "    WHERE Latitude  = @latitude"
                + "      AND Longitude = @longitude"
                + "      AND Width     = @width"
                + "      AND Height    = @height"
                + "      AND Zoom      = @zoom"
                + "      AND MapType   = @mapType;";
            byte[] buffer = null;
            using (SQLiteCommand command = new SQLiteCommand(sql, Connection))
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

        private void CacheImageAsync(MapImageSpec spec, byte[] image)
        {
            const string sql = ""
            + "INSERT OR REPLACE INTO Images VALUES ("
            + "    @latitude, @longitude,"
            + "    @width, @height,"
            + "    @zoom, @mapType,"
            + "    @pngData, @pngDataLen"
            + ");";
            Task ignore = Task.Run(() => {
                using (SQLiteCommand command = new SQLiteCommand(sql, Connection))
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
    }
}
