using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
                Console.WriteLine(ex.ToString());
            }
        }

        int[] ports;
        IPAddress target;
        int timeout;
        string logFolderName;
        System.Collections.Generic.Queue<int> queue;

        void Start(string[] args)
        {
            Console.WriteLine("TCPInterceptor");
            if (!GetParams()) return;
            StartListeners();
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        bool GetParams()
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
            logFolderName = str;

            return true;
        }

    }
}
