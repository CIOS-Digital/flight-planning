using System.IO;
using System.Text;

namespace CIOSDigital.FlightPlan
{
    public class StringWriterUtf8 : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
