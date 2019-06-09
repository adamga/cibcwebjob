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

namespace AIWebjob
{
    class Program
    {
        static void Main(string[] args)
        {
//using literal example. You should probably change this to pull from a config file or something

            Functions func = new Functions();
            func.tracknetresult("www.msn.com", 80);
        }
    }

    public class Functions
    {
       

        public void tracknetresult(string fqdnaddress, int port)
        {
            TelemetryClient telemetryClient = new TelemetryClient();

            if (fqdnaddress.Length<1)
                fqdnaddress = "www.msn.com";
            if (port == 0)
                port = 80;
            //You should probably take out this literal instrumentation key and pull it from a config file or something
            TelemetryConfiguration.Active.InstrumentationKey = "instrumentationkeyhere";

            // You can probably remove this trace statment, simply for debugging
            telemetryClient.TrackTrace("Hello From our sample!");

            var success = false;
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                success = false;
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
                telemetryClient.TrackDependency("NetConnectionTest ", " target=" + fqdnaddress, " port=" + port, startTime, timer.Elapsed, success);
                
                System.Diagnostics.Trace.Write("Complete!");
                telemetryClient.Flush();
                // Allow some time for flushing before shutdown.
                System.Threading.Thread.Sleep(5000);
            }
        }


    }
}
