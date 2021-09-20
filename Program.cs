using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ProcessMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3) {
                Usage();
                return;
            }

            if (!(int.TryParse(args[1], out int lifetime) && int.TryParse(args[2], out int interval))) {
                Usage();
                return;
            }

            TimeSpan procMaxLifetime = TimeSpan.FromMinutes(lifetime), pollInterval = TimeSpan.FromMinutes(interval);
            
            while (true) {
                var processes = Process.GetProcessesByName(args[0]).ToList();
                DateTime now = DateTime.Now;
                for (int i = processes.Count - 1; i >= 0; i--) {
                    TestAndKillProcess(processes[i], now, procMaxLifetime);
                }
                TimeSpan wait = pollInterval - (DateTime.Now - now);
                if (wait > TimeSpan.Zero)
                    Thread.Sleep(wait);
            }
        }

        private static void TestAndKillProcess(Process process, DateTime now, TimeSpan maxLifetime)
        {
            if (now - process.StartTime > maxLifetime) {
                try {
                    process.Kill();
                    process.WaitForExit();
                    Console.WriteLine($"Process { process.ProcessName } with id { process.Id } has been killed.");
                }
                catch (Exception e) {
                    Console.WriteLine($"Can't kill process { process.ProcessName } with id { process.Id }.");
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static void Usage()
        {
            string ExeName = Process.GetCurrentProcess().ProcessName;
            Console.WriteLine($"Usage: { ExeName } <process name> <max lifetime> <poll interval>.");
        }
    }
}
