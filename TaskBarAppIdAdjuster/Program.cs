using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace TaskBarAppIdAdjuster
{
    public class TaskBarAppIdApplicationContext : ApplicationContext
    {
        private NotifyIcon trayIcon;

        private TaskBarService _taskBarService = null;

        private Form1 _form = null;

        private Process _logViewerProcess = null;

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

            // Gather defaults if we have valid settings
            bool autoStartEnabled = false;
            if (_taskBarService.Settings != null)
            {
                autoStartEnabled = _taskBarService.Settings.AutoStart;
            }

            // Create menu items
            MenuItem startStopMenu = new MenuItem(autoStartEnabled ? "Stop" : "Start", OnStart);

            MenuItem autoStartMenu = new MenuItem("Auto Start on Launch", OnToggleAutoStart);
            autoStartMenu.Checked = autoStartEnabled;

            MenuItem openLogMenu = new MenuItem("Open Log", OnOpenLog);

            MenuItem exitMenu = new MenuItem("Exit", OnExit);

            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = _form.Icon,
                ContextMenu = new ContextMenu(new MenuItem[] {
                    startStopMenu,
                    autoStartMenu,
                    openLogMenu,
                    exitMenu
                }),
                Visible = true
            };

            if (autoStartEnabled)
            {
                _taskBarService.Start();
            }
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

        void OnToggleAutoStart(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;

            _taskBarService.Settings.AutoStart = !_taskBarService.Settings.AutoStart;
            _taskBarService.Settings.Save();

            item.Checked = _taskBarService.Settings.AutoStart;
        }

        /// <summary>
        /// Open a console up and bind stdout to it.  Console.WriteLine will start appearing there.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnOpenLog(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            _logViewerProcess = Process.Start(@"TaskBarAppIdAdjuster.log");
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
