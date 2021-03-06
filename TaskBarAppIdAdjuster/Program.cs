﻿using System;
using System.Windows.Forms;

namespace TaskBarAppIdAdjuster
{
    public class TaskBarAppIdApplicationContext : ApplicationContext
    {
        private NotifyIcon trayIcon;

        private TaskBarService _taskBarService = null;

        private Form1 _form = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public TaskBarAppIdApplicationContext()
        {
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);

            // Blank form, not intended for use, just instantiate so we can get the icon for now
            _form = new Form1();

            // Init the task bar watcher service
            _taskBarService = new TaskBarService();

            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = _form.Icon,
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Start", OnStart),
                    new MenuItem("Open Log", OnOpenLog),
                    new MenuItem("Exit", OnExit)                    
                }),
                Visible = true
            };            
        }

        /// <summary>
        /// Start the 'daemon'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnStart(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item.Text == "Start")
            {                
                _taskBarService.Start();

                item.Text = "Stop";
            }
            else
            {
                _taskBarService.Stop();
                item.Text = "Start";
            }

        }

        /// <summary>
        /// Open a console up and bind stdout to it.  Console.WriteLine will start appearing there.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnOpenLog(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item.Text == "Open Log")
            {
                NativeConsole.OpenConsole();

                item.Text = "Close Log";
            }
            else
            {
                NativeConsole.CloseConsole();

                item.Text = "Open Log";
            }
            
        }

        /// <summary>
        /// Shutdown.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// Handle Application Exit.  Gracefully shutdown as best we can.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnApplicationExit(object sender, EventArgs e)
        {
            if (_taskBarService != null)
            {
                _taskBarService.Stop();
            }

            trayIcon.Visible = false;

            if (_form != null)
            {
                _form.Close();
            }
        }
    }


    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);        
            Application.Run(new TaskBarAppIdApplicationContext());            
        }        
    }
}
