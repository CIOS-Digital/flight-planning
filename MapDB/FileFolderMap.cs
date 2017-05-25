using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CIOSDigital.MapDB
{
    public class FileFolderMap : MapProvider
    {
        public ImageSource GetImage(MapImageSpec spec)
        {
            const string path_to_image = "cios-digital/maps/";
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string filepath = Path.Combine(appdata, path_to_image,
                string.Format("{0}/{1}/{2},{3}.png", spec.MapType,
                spec.Zoom, spec.Coordinate.Latitude, spec.Coordinate.Longitude));
            try
            {
                return new BitmapImage(new Uri(filepath));
            } catch (Exception e)
            {
                return null;
            }
        }
    }
}
