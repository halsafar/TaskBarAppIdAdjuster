﻿using Microsoft.WindowsAPICodePack.Taskbar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        private DateTime _settingLastWriteTime;

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
            catch (Exception e)
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

                Process[] processes = Process.GetProcessesByName(taskSetting.Name);
                foreach (Process process in processes)
                {
                    if (process.MainWindowTitle.Length <= 0)
                    {
                        Console.WriteLine("Process matched but has no Window Title, can safely assume no Window, not randomizing: " + process.ProcessName);
                        continue;
                    }

                    var identifier = new Tuple<String, int>(taskSetting.Name, process.Id);
                    if (_adjustedProcesses.Contains(identifier))
                    {
                        Console.WriteLine("Process {0}-{1} has already been adjusted.", taskSetting.Name, process.Id);
                        continue;
                    }

                    // Perform desired action on process
                    if (taskSetting.Action == TaskAction.Ungroup)
                    {
                        Console.WriteLine("Setting process {0} to random group", process.ProcessName);
                        TaskbarManager.Instance.SetApplicationIdForSpecificWindow(process.MainWindowHandle, Guid.NewGuid().ToString());
                    }
                    else if (taskSetting.Action == TaskAction.Group)
                    {
                        Console.WriteLine("Not supported yet...");
                    }

                    // Store so we don't try to process it again.
                    _adjustedProcesses.Add(identifier);
                }
            }
        }
    }
}
