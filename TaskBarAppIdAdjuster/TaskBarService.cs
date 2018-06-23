using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using Microsoft.WindowsAPICodePack.Taskbar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;


namespace TaskBarAppIdAdjuster
{
    class TaskBarService
    {
        private const int SLEEP_TIME = 3000;

        private Thread _thread = null;

        private bool _running = false;

        private Settings _settings = null;

        private HashSet<Tuple<String, int>> _adjustedProcesses = new HashSet<Tuple<String, int>>();

        /// <summary>
        /// Constructor
        /// </summary>
        public TaskBarService()
        {

        }

        /// <summary>
        /// Start the service.
        /// </summary>
        public void Start()
        {
            if (this._running)
            {
                Console.WriteLine("Already running.");
                return;
            }

            _settings = Settings.Load();

            _thread = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                MainLoop();
            });
            _thread.Start();
        }

        /// <summary>
        /// Stop the service.
        /// </summary>
        public void Stop()
        {
            if (_thread == null || !_thread.IsAlive)
            {
                Console.WriteLine("Thread is already stopped or was never started.");
                return;
            }

            this._running = false;
            _thread.Join();
        }

        /// <summary>
        /// Main Service Loop
        /// </summary>
        private void MainLoop()
        {
            this._running = true;

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e) {
                e.Cancel = true;
                this._running = false;
            };

            HandleProcesses();
            Console.WriteLine("Watching processes, press Ctrl+C to quit.");
            while (this._running)
            {
                Thread.Sleep(SLEEP_TIME);
                HandleProcesses();
            }

            Console.WriteLine("Goodbye");
        }

        /// <summary>
        /// Iterate over processes, pick out the ones with valid windows and process them.
        /// </summary>
        private void HandleProcesses()
        {
            foreach (String processName in _settings.ApplicationsToRandomize)
            {
                Console.WriteLine("Searching for any process matching the name: {0}", processName);

                Process[] processes = Process.GetProcessesByName(processName);
                foreach (Process process in processes)
                {
                    if (process.MainWindowTitle.Length <= 0)
                    {
                        Console.WriteLine("Process matched but has no Window Title, can safely assume no Window, not randomizing: " + process.ProcessName);
                        continue;
                    }

                    var identifier = new Tuple<String, int>(processName, process.Id);
                    if (_adjustedProcesses.Contains(identifier))
                    {
                        Console.WriteLine("Process {0}-{1} has already been adjusted.", processName, process.Id);
                        continue;
                    }

                    Console.WriteLine("Setting process {0} to random group", process.ProcessName);
                    TaskbarManager.Instance.SetApplicationIdForSpecificWindow(process.MainWindowHandle, Guid.NewGuid().ToString());

                    _adjustedProcesses.Add(identifier);
                }
            }


            /*Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                // If it has a window title it must be on a taskbar somewhere.
                if (process.MainWindowTitle.Length > 0)
                {
                    Console.WriteLine("Process: " + process.ProcessName);
                }

                process.Id

                if (_settings.ApplicationsToRandomize.Contains(process.ProcessName))
                {
                    Console.WriteLine("Setting to random group");
                    
                    TaskbarManager.Instance.SetApplicationIdForSpecificWindow(process.MainWindowHandle, Guid.NewGuid().ToString());
                }
            }*/
        }
    }
}
