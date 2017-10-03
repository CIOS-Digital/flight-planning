using System.IO;
using System.Text;

namespace CIOSDigital.FlightPlanner.Utility
{
    public class StringWriterUtf8 : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
