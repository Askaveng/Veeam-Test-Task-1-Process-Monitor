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

            var processes = Process.GetProcessesByName(args[0]).ToList();
            bool trackAll = false;

            if (processes.Count == 0) {
                Console.WriteLine("Can't find a process with specified name.");
                return;
            }
            
            if (processes.Count > 1) {
                Console.WriteLine("Found multiple processes with specified name. " +
                    "Please specify the process id or type \"all\" to track all the processes with this name. " + 
                    "List of processes with their id's:");
                foreach (var process in processes) {
                    Console.WriteLine(process.Id + "\t" + process.MainWindowTitle);
                }
                string decision;
                while (true) {
                    decision = Console.ReadLine();
                    if (int.TryParse(decision, out int processId)) {
                        trackAll = false;
                        processes[0] = Process.GetProcessById(processId);
                        break;
                    }
                    if (decision.ToLower() == "all") {
                        trackAll = true;
                        break;
                    }
                    Console.WriteLine("Can't parse the command, please try again.");
                }
            }

            if (!(int.TryParse(args[1], out int lifetime) && int.TryParse(args[2], out int interval))) {
                Usage();
                return;
            }

            TimeSpan procMaxLifetime = TimeSpan.FromMinutes(lifetime), pollInterval = TimeSpan.FromMinutes(interval);
            
            while (true) {
                DateTime now = DateTime.Now;
                if (trackAll) {
                    for (int i = processes.Count - 1; i >= 0; i--) {
                        if (TestAndKillProcess(processes[i], now, procMaxLifetime)) {
                            processes.RemoveAt(i);
                        }
                    }
                    if (processes.Count == 0) return;
                } else {
                    if (TestAndKillProcess(processes[0], now, procMaxLifetime))
                        return;
                }
                TimeSpan wait = pollInterval - (DateTime.Now - now);
                if (wait > TimeSpan.Zero)
                    Thread.Sleep(wait);
            }
        }

        private static bool TestAndKillProcess(Process process, DateTime now, TimeSpan maxLifetime)
        {
            if (now - process.StartTime > maxLifetime) {
                try {
                    process.Kill();
                    process.WaitForExit();
                    Console.WriteLine($"Process { process.ProcessName } with id { process.Id } has been killed.");
                    return true;
                }
                catch (Exception e) {
                    Console.WriteLine($"Can't kill process { process.ProcessName } with id { process.Id }.");
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
            return false;
        }

        private static void Usage()
        {
            string ExeName = Process.GetCurrentProcess().ProcessName;
            Console.WriteLine($"Usage: { ExeName } <process name> <max lifetime> <poll interval>.");
        }
    }
}
