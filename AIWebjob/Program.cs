using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace AIWebjob
{
    class Program
    {
        public TelemetryClient client;
        static void Main(string[] args)
        {
            //using literal example. You should probably change this to pull from a config file or something
            //TelemetryClient telemetryClient = new TelemetryClient();
            var reader = new System.Configuration.AppSettingsReader();
            string key = reader.GetValue("appinsightskey", typeof(string)).ToString();
            TelemetryConfiguration.Active.InstrumentationKey = key;

            TelemetryClient client = new TelemetryClient();
            List<target> targets = JsonConvert.DeserializeObject<List<target>>(System.IO.File.ReadAllText(@"targets.json"));

            foreach (target t in targets)
            {
                tracknetresult(t.site, System.Convert.ToInt32(t.port), client);

            }
            System.Diagnostics.Trace.Write("Complete!");
            client.Flush();
            // Allow some time for flushing before shutdown.
            System.Threading.Thread.Sleep(5000);
        }
        static void tracknetresult(string fqdnaddress, int port, TelemetryClient telemetryClient)
        {
            
            
            if (fqdnaddress.Length < 1)
                fqdnaddress = "www.msn.com";
            if (port == 0)
                port = 80;


     
            // You can probably remove this trace statment, simply for debugging
 //           teleTrackTrace("Hello From our sample!");

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
                telemetryClient.TrackException(ex);
                throw new Exception("Operation went wrong", ex);
            }
            finally
            {
                timer.Stop();
                telemetryClient.TrackDependency("NetConnectionTest ", fqdnaddress, " port=" + port, startTime, timer.Elapsed, success);


            }
        }

    }
    public class target
    {

        public string site { get; set; }
        public string port { get; set; }

    }

}
