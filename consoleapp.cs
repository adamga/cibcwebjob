using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {
        
        static void Main(string[] args)
        {
            TelemetryClient _client = null;
            //get your instrumentation key for app insights here
            //or use hard-coded one
            string key = "575c6048-f89f-4c66-8667-d23a4c7153c5";

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Argument is empty", nameof(key));
            }

            _client = new TelemetryClient(new TelemetryConfiguration() { InstrumentationKey = key });
            string filename = @"C:\Users\burli\source\repos\ConsoleApp1\ConsoleApp2\bin\Debug\test1.bat";
            IEnumerable<string> lines =  System.IO.File.ReadLines(filename) ;
            Dictionary<string,int> urls =  ParseInput(lines);
            foreach (var uristring in urls)
            {

                bool success = false;
                string message = "";
                success = tracknetresult(uristring.Key, uristring.Value);
                AvailabilityTelemetry at = new AvailabilityTelemetry(uristring.Key.Substring(0,20), DateTimeOffset.Now, TimeSpan.Zero, "CanadaCentral", success, message);
                at.Properties.Add("Service", "CoinsICF");
                at.Properties.Add("Instance", "0");
                _client.TrackAvailability(at);
                _client.Flush();

            }

        }

        private static Dictionary<string,int> ParseInput(IEnumerable<string> lines)
        {
            Dictionary<string,int> urlList = new Dictionary<string, int>();
            foreach (string line in lines)
            {
                var linesarray = line.Split(' ');
                urlList.Add(linesarray[6], 80);
            }
            return urlList;
        }


        static bool tracknetresult(string fqdnaddress, int port)

        {

            if (fqdnaddress.Length < 1)

                fqdnaddress = "www.msn.com";

            if (port == 0)

                port = 80;



            var success = false;

            var startTime = DateTime.UtcNow;

            var timer = System.Diagnostics.Stopwatch.StartNew();

            try

            {

                success = true;

                //Uses a remote endpoint to establish a socket connection.

                TcpClient tcpClient = new TcpClient();

                IPAddress ipAddress = Dns.GetHostEntry(fqdnaddress).AddressList[0];

                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);



                tcpClient.Connect(ipEndPoint);



            }

            catch (Exception ex)

            {

                success = false;

                return false;

                throw new Exception("Operation went wrong", ex);
                

            }

            finally

            {

                timer.Stop();
                
 




            }
            return true;

        }
    }
}

