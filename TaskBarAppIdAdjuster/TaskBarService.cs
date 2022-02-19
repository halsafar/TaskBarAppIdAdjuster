using Microsoft.WindowsAPICodePack.Taskbar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;


namespace TaskBarAppIdAdjuster
{
    using TaskEntryTuple = Tuple<String, IntPtr>;

    class TaskBarService
    {
        private const int SLEEP_TIME = 5000;

        private Thread _thread = null;

        private bool _running = false;

        private Settings _settings = null;

        private HashSet<TaskEntryTuple> _adjustedProcesses = new HashSet<TaskEntryTuple>();

        private DateTime _settingLastWriteTime;

        public Settings Settings
        {
            get { return _settings; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public TaskBarService()
        {
            RefreshConfig();
        }

        /// <summary>
        /// Check if we need to refresh the settings.
        /// </summary>
        public void RefreshConfig()
        {
            try
            {
                if (_settings == null)
                {
                    _settings = Settings.Load();
                    _settingLastWriteTime = File.GetLastWriteTimeUtc(Settings.ConfigPath());
                }
                else
                {
                    DateTime curWriteTime = File.GetLastWriteTimeUtc(Settings.ConfigPath());
                    long sub = (long)(curWriteTime - _settingLastWriteTime).TotalMilliseconds;
                    if (sub > 0)
                    {
                        Console.WriteLine("Detected change to settings file...");
                        _settings = Settings.Load();
                        _settingLastWriteTime = curWriteTime;
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to load config file, no actions will be taken until the file is fixed...");
                _settings = null;
            }
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
                RefreshConfig();
                HandleProcesses();
            }

            Console.WriteLine("Goodbye");
        }

        /// <summary>
        /// Iterate over processes, pick out the ones with valid windows and process them.
        /// </summary>
        private void HandleProcesses()
        {
            if (_settings == null)
            {
                return;
            }

            foreach (TaskSetting taskSetting in _settings.ApplicationsToRandomize)
            {
                Console.WriteLine("Searching for any process matching the name: {0}", taskSetting.Name);

                List<Process> processes = new List<Process>();

                // support older configs where Name == Rules[0]
                if (taskSetting.Rules == null && !String.IsNullOrEmpty(taskSetting.Name))
                {
                    processes.AddRange(Process.GetProcessesByName(taskSetting.Name));
                }
                else if (taskSetting.Rules.Count > 0)
                {
                    foreach (string rule in taskSetting.Rules)
                    {
                        processes.AddRange(Process.GetProcessesByName(rule));
                    }
                }
                else
                {
                    // invalid rule, don't log spam
                    return;
                }
                
                foreach (Process process in processes)
                {
                    // Assume if main process has no window then the rest do not?
                    if (process.MainWindowTitle.Length <= 0)
                    {
                        Console.WriteLine("Process matched but has no Window Title, can safely assume no Window, not randomizing: " + process.ProcessName);
                        continue;
                    }

                    // Iterate over windows
                    var handles = NativeWindowHelpers.EnumerateProcessThreadWindowHandles(process.Id);
                    foreach (var handle in handles)
                    {
                        var identifier = new TaskEntryTuple(taskSetting.Name, handle);

                        bool isWindowVisible = NativeWindowHelpers.IsWindowVisible(handle);
                        if (!isWindowVisible)
                        {
                            // Not visible, do not adjust as when it becomes visible the ID might be forcefully set
                            continue;
                        }

                        if (_adjustedProcesses.Contains(identifier))
                        {
                            Console.WriteLine("Process {0}-{1} has already been adjusted.", taskSetting.Name, process.Id);
                            continue;
                        }
                        
                        // Perform desired action on process
                        if (taskSetting.Action == TaskAction.Ungroup)
                        {
                            Guid g = Guid.NewGuid();
                            Console.WriteLine("Setting process {0} to random group {1}", process.ProcessName, g.ToString());
                            TaskbarManager.Instance.SetApplicationIdForSpecificWindow(handle, g.ToString());
                        }
                        else if (taskSetting.Action == TaskAction.Group)
                        {
                            String groupId = "tbAdjusterGroup_" + taskSetting.Name;
                            Console.WriteLine("Setting process {0} to specific group {1}", process.ProcessName, groupId);
                            TaskbarManager.Instance.SetApplicationIdForSpecificWindow(handle, groupId);
                        }

                        // Store so we don't try to process it again.
                        _adjustedProcesses.Add(identifier);
                    }
                }
            }

            // keep the list trim, remove dead processes
            /*_adjustedProcesses.RemoveWhere(delegate (TaskEntryTuple taskEntry) {
                try
                {
                    Process idPid = Process.GetProcessById(taskEntry.Item2);
                    if (idPid.ProcessName.Equals(taskEntry.Item1))
                    {
                        return false;
                    }
                    return true;
                }
                catch (System.ArgumentException)
                {
                    // Means the ID is invalid, so it is not running
                    return true;
                }
            });  */    
            
        }
    }
}
