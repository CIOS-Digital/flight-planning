using System.Threading.Tasks;
using System.Windows.Media;

namespace CIOSDigital.FlightPlanner.Database
{
    public interface IMapProvider
    {
        Task<ImageSource> GetImageAsync(TileSpecifier spec);
    }
}
