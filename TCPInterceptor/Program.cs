using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPInterceptor
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var Program = new Program();
                Program.Start(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled exception caught!");
                Console.WriteLine(ex.ToString());
                if (System.Diagnostics.Debug.Listeners.Count != 0)
                {
                    Console.WriteLine("Press enter to exit");
                    Console.ReadLine();
                }
            }
        }

        int[] ports;
        IPAddress target;
        int timeout;
        string logFolderName;
        System.Collections.Generic.Queue<int> queue;

        public void Start(string[] args)
        {
            Console.WriteLine("TCPInterceptor");
            if (!GetParams()) return;
            Logger logger = new Logger(logFolderName, timeout);
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                foreach (int port in ports)
                {
                    var listener = new TCPListener(port, target, logger);
                    listener.Start(token);
                }
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
                cancellationTokenSource.Cancel();
                System.Threading.Thread.Sleep(200);
            }
        }

        private bool GetParams()
        {
            Console.Write("Enter input ports separated by commas: ");
            var str = Console.ReadLine();
            if (str == "") return false;
            ports = str.Split(',').Select(x => int.Parse(x)).ToArray();

            Console.Write("Enter target IP address: ");
            str = Console.ReadLine();
            if (str == "") return false;
            target = IPAddress.Parse(str);

            Console.Write("Timeout sending along data: ");
            str = Console.ReadLine();
            if (str == "") return false;
            timeout = int.Parse(str);

            Console.Write("Log folder name: ");
            str = Console.ReadLine();
            if (str == "") return false;
            str = (new System.IO.DirectoryInfo(str)).FullName;
            logFolderName = str;

            Console.WriteLine("Writing log to: " + logFolderName);
            Console.Write("Confirm log folder (y/n): ");
            if (!Console.ReadLine().ToLower().StartsWith("y")) return false;
            if (!System.IO.Directory.Exists(logFolderName))
            {
                System.IO.Directory.CreateDirectory(logFolderName);
            }

            return true;
        }

    }
}
