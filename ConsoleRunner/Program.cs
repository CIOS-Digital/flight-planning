using CIOSDigital.FlightPlan;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIOSDigital.ConsoleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            string input_dir = Prompt("Input Directory");
            string output_dir = Prompt("Output Directory");

            Plan[] flightPlans = (from f in Directory.GetFiles(input_dir)
                                  where f.EndsWith(".fpl")
                                  select Plan.XmlLoad(f)).ToArray();
            uint index = 0;
            foreach (Plan fp in flightPlans)
            {
                string output_name = Path.Combine(output_dir, string.Format("{0:0000}.fpl", index));
                string xml_out = fp.ToXmlString(++index);
                Console.WriteLine(output_name);
                using (StreamWriter writer = new StreamWriter(output_name))
                {
                    writer.WriteLine(xml_out);
                }
            }
            Console.ReadKey();
        }

        static string Prompt(string prompt_text)
        {
            string buffer = null;
            do
            {
                Console.Write("{0}> ", prompt_text);
                buffer = Console.ReadLine();
            } while (buffer == null || buffer.Length == 0);
            return buffer;
        }
    }
}
