using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CIOSDigital.MapDB
{
    public class SQLiteMap : MapProvider
    {
        private const string API_KEY = "AIzaSyAqKdnHyxbEm1dFI6xX5Lx0TgOEbRuJ2CE";
        private WebClient WebClient { get; }

        private SQLiteConnection Connection { get; }

        private SQLiteMap()
        {
            { // Initialize the database connection
                SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
                const string filePath = "cios-digital/mapdb.sqlite3";
                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                builder.Uri = "file://" + Path.Combine(folderPath, filePath);
                this.Connection = new SQLiteConnection(builder.ToString()).OpenAndReturn();

                const string createTable = ""
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
                using (SQLiteCommand command = new SQLiteCommand(createTable, this.Connection))
                {
                    BeginTransaction();
                    command.ExecuteNonQuery();
                    EndTransaction();
                }
            }

            this.WebClient = new WebClient();
        }

        public static SQLiteMap OpenDB()
        {
            return new SQLiteMap();
        }

        private void BeginTransaction()
        {
            const string sql = "BEGIN TRANSACTION;";
            using (SQLiteCommand command = new SQLiteCommand(sql, this.Connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private void EndTransaction()
        {
            const string sql = "END TRANSACTION;";
            using (SQLiteCommand command = new SQLiteCommand(sql, this.Connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public ImageSource GetImage(MapImageSpec spec)
        {
            byte[] cachedImage = this.GetCachedImage(spec);
            if (cachedImage == null)
            {
                byte[] image = DownloadImage(spec);
                this.CacheImage(spec, image);
                cachedImage = image;
            }

            MemoryStream imageStream = new MemoryStream(cachedImage);
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = imageStream;
            bitmap.EndInit();
            return bitmap;
        }

        private byte[] DownloadImage(MapImageSpec spec)
        {
            const string queryStringFormat = "https://maps.googleapis.com/maps/api/staticmap?key={0}&center={1},{2}&maptype={3}&size={4}x{5}&zoom={6}";
            Uri queryString = new Uri(string.Format(queryStringFormat,
                API_KEY,
                spec.Coordinate.Latitude, spec.Coordinate.Longitude,
                spec.MapType.ToString().ToLower(),
                spec.Size.Width, spec.Size.Height,
                spec.Zoom));
            Console.WriteLine(queryString);
            return this.WebClient.DownloadData(queryString);
        }

        private byte[] GetCachedImage(MapImageSpec spec)
        {
            byte[] buffer = null;
            const string sql = ""
                + "SELECT PngData, PngDataLen FROM Images"
                + "    WHERE Latitude  = @latitude"
                + "      AND Longitude = @longitude"
                + "      AND Width     = @width"
                + "      AND Height    = @height"
                + "      AND Zoom      = @zoom"
                + "      AND MapType   = @mapType;";
            using (SQLiteCommand command = new SQLiteCommand(sql, this.Connection))
            {
                BeginTransaction();
                command.Parameters.AddWithValue("@latitude", spec.Coordinate.Latitude);
                command.Parameters.AddWithValue("@longitude", spec.Coordinate.Longitude);
                command.Parameters.AddWithValue("@width", spec.Size.Width);
                command.Parameters.AddWithValue("@height", spec.Size.Height);
                command.Parameters.AddWithValue("@zoom", spec.Zoom);
                command.Parameters.AddWithValue("@mapType", spec.MapType);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int length = reader.GetInt32(1);
                        buffer = new byte[length];
                        reader.GetBytes(0, 0, buffer, 0, length);
                    }
                }
                EndTransaction();
            }
            return buffer;
        }

        private void CacheImage(MapImageSpec spec, byte[] image)
        {
            const string sql = ""
                + "INSERT OR REPLACE INTO Images VALUES ("
                + "    @latitude, @longitude,"
                + "    @width, @height,"
                + "    @zoom, @mapType,"
                + "    @pngData, @pngDataLen"
                + ");";
            using (SQLiteCommand command = new SQLiteCommand(sql, this.Connection))
            {
                BeginTransaction();
                command.Parameters.AddWithValue("@latitude", spec.Coordinate.Latitude);
                command.Parameters.AddWithValue("@longitude", spec.Coordinate.Longitude);
                command.Parameters.AddWithValue("@width", spec.Size.Width);
                command.Parameters.AddWithValue("@height", spec.Size.Height);
                command.Parameters.AddWithValue("@zoom", spec.Zoom);
                command.Parameters.AddWithValue("@mapType", spec.MapType);
                command.Parameters.AddWithValue("@pngData", image);
                command.Parameters.AddWithValue("@pngDataLen", image.Length);
                command.ExecuteNonQuery();
                EndTransaction();
            }
        }
    }
}
