using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using dal;
using System.IO;
using Microsoft.Win32;
using System.Threading;
using System.Net;
using System.Diagnostics;

namespace rehabilitation_management_system
{
    public partial class main_form : Form
    {
        private int updateStatusCounter = 60;
        private int loggedinTimeCounter = 0;
        DateTime _startTime = DateTime.Now;
        string _template;
        private string TRIAL_PERIOD = "370";
        int max_msg_length = 0;
        int collect_extras_seconds_counta = 0;
        string UPLOAD_DOWNLOAD_WIZARD_FILE_NAME = string.Empty;
        /* to use a BackgroundWorker object to perform time-intensive operations in a background thread.
            You need to:
            1. Define a worker method that does the time-intensive work and call it from an event handler for the DoWork
            event of a BackgroundWorker.
            2. Start the execution with RunWorkerAsync. Any argument required by the worker method attached to DoWork
            can be passed in via the DoWorkEventArgs parameter to RunWorkerAsync.
            In addition to the DoWork event the BackgroundWorker class also defines two events that should be used for
            interacting with the user interface. These are optional.
            The RunWorkerCompleted event is triggered when the DoWork handlers have completed.
            The ProgressChanged event is triggered when the ReportProgress method is called. */
        BackgroundWorker bgWorker = new BackgroundWorker();
        private string current_action = string.Empty;
        //task start time
        DateTime _task_start_time = DateTime.Now;
        //task end time
        DateTime _task_end_time = DateTime.Now;
        private string TAG;
        private List<notificationdto> _lstnotificationdto = new List<notificationdto>();
        //Event declaration:
        //event for publishing messages to output
        private event EventHandler<notificationmessageEventArgs> _notificationmessageEventname;

        public main_form()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);
            Application.ThreadException += new ThreadExceptionEventHandler(ThreadException);

            TAG = this.GetType().Name;

            //Subscribing to the event: 
            //Dynamically:
            //EventName += HandlerName;
            _notificationmessageEventname += notificationmessageHandler;

            _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs("finished MainForm initialization", TAG));

        }

        private void main_form_Load(object sender, EventArgs e)
        {
            try
            {
                this.lblselecteddatabase.Text = string.Empty;
                this.lblversion.Text = string.Empty;
                this.lbltimelapsed.Text = string.Empty;
                this.lblStatusUpdates.Text = string.Empty;
                this.lbl_info.Text = string.Empty;
                this.lblrunningtime.Text = string.Empty;
                this.lblLoggedInUser.Text = string.Empty;

                current_action = "load";

                _task_start_time = DateTime.Now;

                //This allows the BackgroundWorker to be cancelled in between tasks
                bgWorker.WorkerSupportsCancellation = true;
                //This allows the worker to report progress between completion of tasks...
                //this must also be used in conjunction with the ProgressChanged event
                bgWorker.WorkerReportsProgress = true;

                //this assigns event handlers for the backgroundWorker
                bgWorker.DoWork += bgWorker_DoWork;
                bgWorker.RunWorkerCompleted += bgWorker_WorkComplete;
                /* When you wish to have something occur when a change in progress
                    occurs, (like the completion of a specific task) the "ProgressChanged"
                    event handler is used. Note that ProgressChanged events may be invoked
                    by calls to bgWorker.ReportProgress(...) only if bgWorker.WorkerReportsProgress
                    is set to true. */
                bgWorker.ProgressChanged += bgWorker_ProgressChanged;

                //tell the backgroundWorker to raise the "DoWork" event, thus starting it.
                //Check to make sure the background worker is not already running.
                if (!bgWorker.IsBusy)
                    bgWorker.RunWorkerAsync();

                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs("finished MainForm load", TAG));

            }
            catch (Exception ex)
            {
                Utils.ShowError(ex);
            }
        }

        private void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            Utils.LogEventViewer(ex);
            Log.WriteToErrorLogFile_and_EventViewer(ex);
            _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
        }

        private void ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            Utils.LogEventViewer(ex);
            Log.WriteToErrorLogFile(ex);
            _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
        }

        //Event handler declaration:
        private void notificationmessageHandler(object sender, notificationmessageEventArgs args)
        {
            try
            {
                /* Handler logic */


                notificationdto _notificationdto = new notificationdto();

                DateTime currentDate = DateTime.Now;
                String dateTimenow = currentDate.ToString("dd-MM-yyyy HH:mm:ss tt");

                String _logtext = Environment.NewLine + "[ " + dateTimenow + " ]   " + args.message;

                _notificationdto._notification_message = _logtext;
                _notificationdto._created_datetime = dateTimenow;
                _notificationdto.TAG = args.TAG;

                _lstnotificationdto.Add(_notificationdto);
                Console.WriteLine(args.message);

                var _lstmsgdto = from msgdto in _lstnotificationdto
                                 orderby msgdto._created_datetime descending
                                 select msgdto._notification_message;

                String[] _logflippedlines = _lstmsgdto.ToArray();

                if (_logflippedlines.Length > 5000)
                {
                    _lstnotificationdto.Clear();
                }

                txtlog.Lines = _logflippedlines;
                txtlog.ScrollToCaret();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void NavigateToHomePage()
        {
            try
            {
                string help_file = "index.html";

                string base_directory = AppDomain.CurrentDomain.BaseDirectory;
                string help_path = Path.Combine(base_directory, "help");
                string help_file_path = Path.Combine(help_path, help_file);

                FileInfo fi = new FileInfo(help_file_path);

                if (fi.Exists)
                    this.webBrowser.Navigate(fi.FullName);
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Utils.LogEventViewer(ex);
            }
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.ApplicationExitCall)
            {
                try
                {
                    collect_admin_info_in_background_worker_thread();
                    CloseAllOpenForms();
                }
                catch (Exception ex)
                {
                    _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                    Utils.LogEventViewer(ex);
                }
            }
            if (e.CloseReason == CloseReason.UserClosing)
            {
                try
                {
                    collect_admin_info_in_background_worker_thread();
                    CloseAllOpenForms();
                }
                catch (Exception ex)
                {
                    _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                    Utils.LogEventViewer(ex);
                }
            }
            if (e.CloseReason == CloseReason.WindowsShutDown)
            {
                try
                {
                    collect_admin_info_in_background_worker_thread();
                    CloseAllOpenForms();
                }
                catch (Exception ex)
                {
                    _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                    Utils.LogEventViewer(ex);
                }
            }
            if (e.CloseReason == CloseReason.FormOwnerClosing)
            {
                try
                {
                    collect_admin_info_in_background_worker_thread();
                    CloseAllOpenForms();
                }
                catch (Exception ex)
                {
                    _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                    Utils.LogEventViewer(ex);
                }
            }
            if (e.CloseReason == CloseReason.MdiFormClosing)
            {
                try
                {
                    collect_admin_info_in_background_worker_thread();
                    CloseAllOpenForms();
                }
                catch (Exception ex)
                {
                    _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                    Utils.LogEventViewer(ex);
                }
            }
            if (e.CloseReason == CloseReason.None)
            {
                try
                {
                    collect_admin_info_in_background_worker_thread();
                    CloseAllOpenForms();
                }
                catch (Exception ex)
                {
                    _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                    Utils.LogEventViewer(ex);
                }
            }
            if (e.CloseReason == CloseReason.TaskManagerClosing)
            {
                try
                {
                    collect_admin_info_in_background_worker_thread();
                    CloseAllOpenForms();
                }
                catch (Exception ex)
                {
                    _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                    Utils.LogEventViewer(ex);
                }
            }
        }
        private void CloseAllOpenForms()
        {
            try
            {

                NotifyMessage(Utils.APP_NAME, "Exiting...");

                List<Form> openForms = new List<Form>();
                foreach (Form f in Application.OpenForms)
                {
                    openForms.Add(f);
                }

                foreach (Form f in openForms)
                {
                    if (f.Name != "MainForm")
                        f.Close();
                }

                try
                {
                    string registryPath = "SOFTWARE\\" + Application.CompanyName + "\\" + Application.ProductName + "\\" + Application.ProductVersion;
                    RegistryKey MyReg = Registry.CurrentUser.CreateSubKey(registryPath);

                    DateTime currentDate = DateTime.Now;
                    String dateTimenow = currentDate.ToString("dd-MM-yyyy HH:mm:ss tt");

                    MyReg.SetValue("Last Usage Date", dateTimenow);

                    SplashScreen.ShowSplashScreen();

                    Application.DoEvents();

                    //SplashScreen.SetStatus("creating a backup of database  [" + system.Database + "]");

                    //DatabaseControlPanelForm control_panel = new DatabaseControlPanelForm(_notificationmessageEventname);

                    DateTime now = DateTime.Now;
                    string formatted_file_name = now.Day + "_" + now.Month + "_" + now.Year + "_" + now.Hour + "_" + now.Minute + "_" + now.Second;

                    string base_directory = Utils.get_application_path();
                    string back_up_path = Utils.build_file_path(base_directory, "database_backup");

                    //bool back_up_successfull = control_panel.backup_database_automatically(system.Server, system.Database, back_up_path, formatted_file_name);

                    //if (back_up_successfull)
                    //{
                    //    SplashScreen.SetStatus("backup of database [" + system.Database + "] successfull.");

                    //}
                    //else
                    //{
                    //    SplashScreen.SetStatus("backup of database  [" + system.Database + "] failed.");
                    //}

                    System.Threading.Thread.Sleep(2000);

                    SplashScreen.CloseForm();

                }
                catch (Exception ex)
                {
                    _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                    Log.WriteToErrorLogFile_and_EventViewer(ex);
                }
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Log.WriteToErrorLogFile_and_EventViewer(ex);
            }
        }

        private void LoadHelpers()
        {
            try
            {
                string AssemblyProduct = app_assembly_info.AssemblyProduct;
                string AssemblyVersion = app_assembly_info.AssemblyVersion;
                string AssemblyCopyright = app_assembly_info.AssemblyCopyright;
                string AssemblyCompany = app_assembly_info.AssemblyCompany;
                this.Text += AssemblyProduct;
                //this.lblselecteddatabase.Text = "Selected Database:     " + system.Database;
                //_notificationmessageEventname.Invoke(this, new notificationmessageEventArgs("Selected Database [ " + system.Database + " ]", TAG));
                this.lblversion.Text = "Version:     " + AssemblyVersion;
                this.lblrunningtime.Text = DateTime.Today.ToShortDateString();
                this.toolStripStatusLabel3.Visible = false;

                loggedInTimer.Tick += new EventHandler(loggedInTimer_Tick);
                loggedInTimer.Interval = 1000; // 1 second
                loggedInTimer.Start();
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Utils.ShowError(ex);
            }
        }
        private void loggedInTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                loggedinTimeCounter++;
                DateTime nowDate = DateTime.Now;
                TimeSpan t = nowDate - _startTime;
                lbltimelapsed.Text = string.Format("{0} : {1} : {2} : {3}", t.Days, t.Hours, t.Minutes, t.Seconds);

                if (loggedinTimeCounter == collect_extras_seconds_counta)
                {
                    collect_admin_info_in_background_worker_thread();
                }
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Utils.LogEventViewer(ex);
            }
        }

        private bool NotifyMessage(string _Title, string _Text)
        {
            try
            {
                appNotifyIcon.Text = Utils.APP_NAME;
                appNotifyIcon.Icon = new Icon("Resources/Icons/Dollar.ico");
                appNotifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                appNotifyIcon.ContextMenuStrip = contextMenuStripSystemNotification;
                appNotifyIcon.BalloonTipTitle = _Title;
                appNotifyIcon.BalloonTipText = _Text;
                appNotifyIcon.Visible = true;
                appNotifyIcon.ShowBalloonTip(900000);

                return true;
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Utils.LogEventViewer(ex);
                return false;
            }
        }
        private void collect_admin_info_in_background_worker_thread()
        {
            try
            {
                current_action = "collect";

                _task_start_time = DateTime.Now;

                //This allows the BackgroundWorker to be cancelled in between tasks
                bgWorker.WorkerSupportsCancellation = true;
                //This allows the worker to report progress between completion of tasks...
                //this must also be used in conjunction with the ProgressChanged event
                bgWorker.WorkerReportsProgress = true;

                //this assigns event handlers for the backgroundWorker
                bgWorker.DoWork += bgWorker_DoWork;
                bgWorker.RunWorkerCompleted += bgWorker_WorkComplete;
                /* When you wish to have something occur when a change in progress
                    occurs, (like the completion of a specific task) the "ProgressChanged"
                    event handler is used. Note that ProgressChanged events may be invoked
                    by calls to bgWorker.ReportProgress(...) only if bgWorker.WorkerReportsProgress
                    is set to true. */
                bgWorker.ProgressChanged += bgWorker_ProgressChanged;

                //tell the backgroundWorker to raise the "DoWork" event, thus starting it.
                //Check to make sure the background worker is not already running.
                if (!bgWorker.IsBusy)
                    bgWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Utils.LogEventViewer(ex);
            }
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                //this is the method that the backgroundworker will perform on in the background thread without blocking the UI.
                /* One thing to note! A try catch is not necessary as any exceptions will terminate the
                backgroundWorker and report the error to the "RunWorkerCompleted" event */
                if (current_action.Equals("load"))
                {
                    try
                    {
                        TRIAL_PERIOD = System.Configuration.ConfigurationManager.AppSettings["TRIAL_PERIOD"];
                        max_msg_length = int.Parse(System.Configuration.ConfigurationManager.AppSettings["MAX_MSG_LENGTH"]);
                        collect_extras_seconds_counta = int.Parse(System.Configuration.ConfigurationManager.AppSettings["COLLECT_EXTRAS_SECONDS_COUNTA"]);
                        UPLOAD_DOWNLOAD_WIZARD_FILE_NAME = System.Configuration.ConfigurationManager.AppSettings["UPLOAD_DOWNLOAD_WIZARD_FILE_NAME"];

                        Write_To_Current_User_Registery_on_App_first_start();

                        Write_To_Local_Machine_Registery_on_App_first_start();

                        NavigateToHomePage();

                        //LogIn();

                        Invoke(new System.Action(() =>
                        {
                            //DisableAllMenus();

                            //Authorize();

                            LoadHelpers();
                        }));

                        WriteToCurrentUserRegistery();

                        check_if_app_is_actively_licensed();

                    }
                    catch (Exception ex)
                    {
                        _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                        Log.WriteToErrorLogFile_and_EventViewer(ex);
                    }
                }
                else if (current_action.Equals("collect"))
                {
                    try
                    {
                        CollectAdminExtraInfo();
                        CollectAdminAppInfo();
                    }
                    catch (Exception ex)
                    {
                        _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                        Log.WriteToErrorLogFile_and_EventViewer(ex);
                    }
                }
                else if (current_action.Equals("home"))
                {
                    try
                    {
                        string url = System.Configuration.ConfigurationManager.AppSettings["WEB_VERSION_URL"];

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        if (response.StatusDescription.Equals("OK"))
                        {
                            Invoke(new System.Action(() =>
                            {
                                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(string.Format("loading [{0}]", url), TAG));
                            }));

                            webBrowser.Invoke(new System.Action(() =>
                            {
                                this.webBrowser.Navigate(url);
                            }));

                        }
                    }
                    catch (Exception ex)
                    {
                        _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                        Log.WriteToErrorLogFile_and_EventViewer(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Utils.LogEventViewer(ex);
            }
        }

        /*This is how the ProgressChanged event method signature looks like:*/
        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Things to be done when a progress change has been reported
            /* The ProgressChangedEventArgs gives access to a percentage,
            allowing for easy reporting of how far along a process is*/
            int progress = e.ProgressPercentage;
        }

        private void bgWorker_WorkComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                bgWorker.DoWork -= bgWorker_DoWork;
                bgWorker.RunWorkerCompleted -= bgWorker_WorkComplete;
                bgWorker.ProgressChanged -= bgWorker_ProgressChanged;
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Utils.LogEventViewer(ex);
            }
        }

        private void updateStatusTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                updateStatusCounter--;
                if (updateStatusCounter == 0)
                {
                    lblStatusUpdates.Text = string.Empty;
                    toolStripStatusLabel3.Visible = false;
                    updateStatusTimer.Stop();
                }
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Utils.LogEventViewer(ex);
            }
        }

        private bool WriteToCurrentUserRegistery()
        {
            try
            {
                string registryPath = "SOFTWARE\\" + Application.CompanyName + "\\" + Application.ProductName + "\\" + Application.ProductVersion;
                RegistryKey MyReg = Registry.CurrentUser.CreateSubKey(registryPath);
                MyReg.SetValue("Company Name", Application.CompanyName);
                MyReg.SetValue("Application Name", Application.ProductName);
                MyReg.SetValue("Version", Application.ProductVersion);
                MyReg.SetValue("Launch Date", DateTime.Now.ToString());
                MyReg.SetValue("Trial Period", TRIAL_PERIOD);
                MyReg.SetValue("Developer", "Kevin Mutugi");
                MyReg.SetValue("Copyright", "Copyright ©  2015 - 2040");
                MyReg.SetValue("GUID", "baedce16-cf28-4a20-a5f3-4c698c242d99");
                MyReg.SetValue("TradeMark", "Soft Books Suite");
                MyReg.SetValue("Phone-Safaricom1", "+254717769329");
                MyReg.SetValue("Phone-Safaricom2", "+254740538757");
                MyReg.SetValue("Email", "kevin@softwareproviders.co.ke");
                MyReg.SetValue("Gmail", "kevinmk30@gmail.com");
                MyReg.SetValue("Company Website", "www.softwareproviders.co.ke");
                MyReg.SetValue("Company Email", "softwareproviders254@gmail.com");
                MyReg.Close();
                return true;
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Log.WriteToErrorLogFile_and_EventViewer(ex);
                return false;
            }
        }
        private void Write_To_Current_User_Registery_on_App_first_start()
        {
            try
            {
                string registryPath = "SOFTWARE\\" + Application.CompanyName + "\\" + Application.ProductName + "\\" + Application.ProductVersion;

                DateTime currentDate = DateTime.Now;
                String dateTimenow = currentDate.ToString("dd-MM-yyyy HH:mm:ss tt");
                string keyvaluedata = string.Empty;

                using (RegistryKey MyReg = Registry.CurrentUser.OpenSubKey(registryPath, RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl))
                {
                    if (MyReg != null)
                    {
                        keyvaluedata = string.Format("{0}", MyReg.GetValue("First Usage Time", "").ToString());
                    }
                }

                if (keyvaluedata.Length == 0)
                {
                    RegistryKey MyReg = Registry.CurrentUser.CreateSubKey(registryPath);

                    MyReg.SetValue("First Usage Time", dateTimenow);

                    string mac_address = Utils.GetMACAddress();
                    Console.WriteLine("Mac Address [ " + mac_address + " ]");
                    MyReg.SetValue("Mac Address", mac_address);

                    string computer_name = Utils.get_computer_name();
                    Console.WriteLine("Computer Name [ " + computer_name + " ]");
                    MyReg.SetValue("Computer Name", computer_name);

                    MyReg.Close();
                }

            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Log.WriteToErrorLogFile_and_EventViewer(ex);
            }
        }
        private void Write_To_Local_Machine_Registery_on_App_first_start()
        {
            try
            {
                string registryPath = "SOFTWARE\\" + Application.CompanyName + "\\" + Application.ProductName + "\\" + Application.ProductVersion;

                DateTime currentDate = DateTime.Now;
                String dateTimenow = currentDate.ToString("dd-MM-yyyy HH:mm:ss tt");
                string keyvaluedata = string.Empty;

                using (RegistryKey MyReg = Registry.LocalMachine.OpenSubKey(registryPath, RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl))
                {
                    if (MyReg != null)
                    {
                        keyvaluedata = string.Format("{0}", MyReg.GetValue("First Usage Time", "").ToString());
                    }
                }

                if (keyvaluedata.Length == 0)
                {
                    RegistryKey MyReg = Registry.LocalMachine.CreateSubKey(registryPath);

                    MyReg.SetValue("First Usage Time", dateTimenow);

                    string mac_address = Utils.GetMACAddress();
                    Console.WriteLine("Mac Address [ " + mac_address + " ]");
                    MyReg.SetValue("Mac Address", mac_address);

                    string computer_name = Utils.get_computer_name();
                    Console.WriteLine("Computer Name [ " + computer_name + " ]");
                    MyReg.SetValue("Computer Name", computer_name);

                    MyReg.Close();
                }

            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Log.WriteToErrorLogFile_and_EventViewer(ex);
            }
        }
        private bool WriteToCurrentUserRegisteryonAppClose(string _totalLoggedinTime)
        {
            try
            {
                string _totalusagetime = this.ReadFromCurrentUserRegisteryTotalUsageTime();
                string _lastusagetime = this.ReadFromCurrentUserRegisteryLastUsageTime();

                TimeSpan ts = TimeSpan.Parse(_lastusagetime);
                TimeSpan _tust = TimeSpan.Parse(_totalLoggedinTime);
                TimeSpan _tts = _tust + ts;

                this.DeleteCurrentUserRegistery();

                string registryPath = "SOFTWARE\\" + Application.CompanyName + "\\" + Application.ProductName + "\\" + Application.ProductVersion;
                RegistryKey MyReg = Registry.CurrentUser.CreateSubKey(registryPath);
                MyReg.SetValue("Last Usage Time", _totalLoggedinTime);
                MyReg.SetValue("Total Usage Time", _tts);
                MyReg.Close();
                return true;
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Log.WriteToErrorLogFile_and_EventViewer(ex);
                return false;
            }
        }
        private bool DeleteCurrentUserRegistery()
        {
            try
            {

                string registryPath = "SOFTWARE\\" + Application.CompanyName + "\\" + Application.ProductName + "\\" + Application.ProductVersion;
                using (RegistryKey MyReg = Registry.CurrentUser.OpenSubKey(registryPath, RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl))
                {
                    MyReg.DeleteValue("Last Usage Time");
                    MyReg.DeleteValue("Total Usage Time");
                }
                return true;
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Log.WriteToErrorLogFile_and_EventViewer(ex);
                return false;
            }
        }
        private string ReadFromCurrentUserRegisteryEXP()
        {
            try
            {
                string registryPath = "SOFTWARE\\" + Application.CompanyName + "\\" + Application.ProductName + "\\" + Application.ProductVersion;
                using (RegistryKey MyReg = Registry.CurrentUser.OpenSubKey(registryPath, RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl))
                {
                    string keyvaluedata = string.Format("{0}", MyReg.GetValue("Trial Period", TRIAL_PERIOD).ToString());
                    return keyvaluedata;
                }
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Log.WriteToErrorLogFile_and_EventViewer(ex);
                return null;
            }
        }
        private string ReadFromCurrentUserRegisteryStartDate()
        {
            try
            {
                string registryPath = "SOFTWARE\\" + Application.CompanyName + "\\" + Application.ProductName + "\\" + Application.ProductVersion;
                using (RegistryKey MyReg = Registry.CurrentUser.OpenSubKey(registryPath, RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl))
                {
                    string keyvaluedata = string.Format("{0}", MyReg.GetValue("Launch Date", DateTime.Now.ToString()).ToString());
                    return keyvaluedata;
                }
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Log.WriteToErrorLogFile_and_EventViewer(ex);
                return null;
            }
        }
        private string ReadFromCurrentUserRegisteryTotalUsageTime()
        {
            try
            {
                string registryPath = "SOFTWARE\\" + Application.CompanyName + "\\" + Application.ProductName + "\\" + Application.ProductVersion;
                using (RegistryKey MyReg = Registry.CurrentUser.OpenSubKey(registryPath, RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl))
                {
                    string keyvaluedata = string.Format("{0}", MyReg.GetValue("Total Usage Time", 0).ToString());
                    return keyvaluedata;
                }
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Log.WriteToErrorLogFile_and_EventViewer(ex);
                return null;
            }
        }
        private string ReadFromCurrentUserRegisteryLastUsageTime()
        {
            try
            {
                string registryPath = "SOFTWARE\\" + Application.CompanyName + "\\" + Application.ProductName + "\\" + Application.ProductVersion;
                using (RegistryKey MyReg = Registry.CurrentUser.OpenSubKey(registryPath, RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl))
                {
                    string keyvaluedata = string.Format("{0}", MyReg.GetValue("Last Usage Time", 0).ToString());
                    return keyvaluedata;
                }
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Log.WriteToErrorLogFile_and_EventViewer(ex);
                return null;
            }
        }

        private void CollectAdminAppInfo()
        {
            string template = string.Empty;
            StringBuilder sb = new StringBuilder();
            try
            {
                DateTime nowDate = DateTime.Now;
                TimeSpan t = nowDate - _startTime;

                WriteToCurrentUserRegisteryonAppClose(t.ToString());

                string loggederror = string.Empty;
                loggederror = Utils.ReadLogFile();
                string macaddrress = Utils.GetMACAddress();
                string ipAddresses = Utils.GetFormattedIpAddresses();
                DateTime _endTime = DateTime.Now;
                string _totalusagetime = this.ReadFromCurrentUserRegisteryTotalUsageTime();

                //if greater than zero then truncate
                if (max_msg_length > 0)
                {
                    string truncated_string = truncate_string_recursively(loggederror);

                    int original_length = loggederror.Length;
                    int truncated_length = truncated_string.Length;

                    loggederror = truncated_string;
                }

                //sb.Append("User [ " + LoggedInUser.UserName + " ] ");
                sb.Append("was logged in from [ " + this._startTime.ToString() + " ] ");
                sb.Append("to [ " + _endTime.ToString() + " ] ");
                sb.Append("total elapsed time [ " + lbltimelapsed.Text + " ] ");
                sb.Append("machine name [ " + FQDN.GetFQDN() + " ] ");
                sb.Append("MAC [ " + macaddrress + " ] ");
                sb.Append("ip addresses [ " + ipAddresses + " ] ");
                sb.Append("Total Usage Time [ " + _totalusagetime + " ] ");
                sb.Append("Template [ " + _template + " ] ");
                sb.Append("Logged Errors " + " [ " + loggederror + " ] ");

                template = sb.ToString();

                Console.WriteLine(template);

                if (Utils.IsConnectedToInternet())
                {
                    bool is_email_sent = Utils.SendEmail(template);
                }
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Utils.LogEventViewer(ex);
            }
            finally
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(template, TAG));
                Log.WriteToErrorLogFile_and_EventViewer(new Exception(template));
            }
        }
        private String truncate_string_recursively(string string_to_truncate)
        {
            try
            {
                string truncated_string = string_to_truncate;
                if (truncated_string.Length > max_msg_length)
                {
                    int half = truncated_string.Length / 2;
                    truncated_string = truncated_string.Substring(half);
                }
                if (truncated_string.Length > max_msg_length)
                {
                    truncated_string = truncate_string_recursively(truncated_string);
                }

                int truncated_length = truncated_string.Length;
                Console.WriteLine(truncated_length);

                return truncated_string;
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Utils.LogEventViewer(ex);
                return string_to_truncate;
            }
        }
        private bool CollectAdminExtraInfo()
        {
            try
            {
                ExecuteIPConfigCommands();

                FindComputersConectedToHost();

                GetClientExtraInfo();

                //GetHostNameandMac();

                return true;
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Utils.LogEventViewer(ex);
                return false;
            }
        }
        private void check_if_app_is_actively_licensed()
        {
            try
            {
                check_if_app_is_actively_licensed_from_db();
                check_if_app_is_actively_licensed_from_xml();
                check_if_app_is_actively_licensed_from_registry();
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Log.WriteToErrorLogFile_and_EventViewer(ex);
            }
        }

        private void check_if_app_is_actively_licensed_from_db()
        {
            try
            {
                string _mac_address = Utils.GetMACAddress();

                //tbl_LAS license_activation_store = db.tbl_LAS.Where(i => i.mac_address.Equals(_mac_address)).FirstOrDefault();

                //if (license_activation_store == null)
                //{
                //    //activate
                //    inform_user_need_to_activate();
                //}
                //else
                //{
                //    //check for expiry
                //    string mac_address = license_activation_store.mac_address;
                //    string computer_name = license_activation_store.computer_name;
                //    string activation_key = license_activation_store.activation_key;
                //    string date_activated = license_activation_store.date_activated;
                //    string next_expiry_date = license_activation_store.next_expiry_date;
                //    string days_for_expiry = license_activation_store.days_for_expiry;

                //    DateTime dateactivate = DateTime.Parse(date_activated);
                //    DateTime dateexpiry = DateTime.Parse(next_expiry_date);
                //    DateTime today = DateTime.Now;

                //    int difference_from = dateactivate.Subtract(today).Days;
                //    Console.WriteLine(difference_from);

                //    int difference_to = dateexpiry.Subtract(today).Days;
                //    Console.WriteLine(difference_to);

                //    string str_difference_from = difference_from.ToString();
                //    str_difference_from = str_difference_from.Replace("-", "");

                //    lbl_info.Visible = true;
                //    lbl_info.Text = " R     " + difference_to.ToString() + "         E     " + str_difference_from.ToString() + "  ";

                //    //expired
                //    if (difference_to <= 0)
                //    {
                //        db.tbl_LAS.DeleteObject(license_activation_store);
                //        db.SaveChanges();

                //        inform_user_need_to_activate();
                //    }
                //    else if (difference_to <= 30)
                //    {
                //        lbl_info.Text += "  License remaining [ " + difference_to + " ] days to expiration.  ";
                //        lbl_info.BackColor = Color.Red;
                //        lbl_info.ForeColor = Color.White;
                //    }
                //    else
                //    {
                //        return;
                //    }
                //}

            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Log.WriteToErrorLogFile_and_EventViewer(ex);
            }
        }

        private void check_if_app_is_actively_licensed_from_xml()
        {
            try
            {
                string activator_filename = Utils.ACTIVATOR_FILENAME;

                if (File.Exists(activator_filename))
                {
                    System.Data.DataSet ds = new System.Data.DataSet();

                    ds.ReadXml(activator_filename);

                    if (ds.Tables.Count == 0)
                    {
                        //activate 
                        inform_user_need_to_activate();
                    }
                    else
                    {

                        int count = ds.Tables[0].Rows.Count;

                        for (int i = 0; i < count; i++)
                        {
                            //ds.Tables[0].DefaultView.RowFilter = "userName = '" + userName + "' and databaseName = '" + databaseName + "' and serverName = '" + serverName + "' and system = '" + system + "'";

                            System.Data.DataTable dt = (ds.Tables[0].DefaultView).ToTable();

                            if (dt.Rows.Count > 0)
                            {
                                //check for expiry
                                string mac_address = string.Format("{0}", dt.Rows[i][0]);
                                string computer_name = string.Format("{0}", dt.Rows[i][1]);
                                string activation_key = string.Format("{0}", dt.Rows[i][2]);
                                string date_activated = string.Format("{0}", dt.Rows[i][3]);
                                string next_expiry_date = string.Format("{0}", dt.Rows[i][4]);
                                string days_for_expiry = string.Format("{0}", dt.Rows[i][5]);

                                DateTime dateactivate = DateTime.Parse(date_activated);
                                DateTime dateexpiry = DateTime.Parse(next_expiry_date);
                                DateTime today = DateTime.Now;

                                int difference_from = dateactivate.Subtract(today).Days;
                                Console.WriteLine(difference_from);

                                int difference_to = dateexpiry.Subtract(today).Days;
                                Console.WriteLine(difference_to);

                                string str_difference_from = difference_from.ToString();
                                str_difference_from = str_difference_from.Replace("-", "");

                                lbl_info.Visible = true;
                                lbl_info.Text = " R     " + difference_to.ToString() + "         E     " + str_difference_from.ToString() + "  ";

                                //expired
                                if (difference_to <= 0)
                                {
                                    ds.Tables[0].Rows[i].Delete();

                                    inform_user_need_to_activate();
                                }
                                else if (difference_to <= 30)
                                {
                                    lbl_info.Text += "  License remaining [ " + difference_to + " ] days to expiration.  ";
                                    lbl_info.BackColor = Color.Red;
                                    lbl_info.ForeColor = Color.White;
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }

                        //get data
                        string xmlData = ds.GetXml();

                        //save to file
                        ds.WriteXml(activator_filename);
                    }
                }
                else
                {
                    //activate
                    inform_user_need_to_activate();
                }
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Log.WriteToErrorLogFile_and_EventViewer(ex);
            }
        }

        private void check_if_app_is_actively_licensed_from_registry()
        {
            try
            {
                string registryPath = "SOFTWARE\\" + "Activator" + "\\" + Application.CompanyName + "\\" + Utils.APP_NAME;

                string keyvaluedata = string.Empty;

                using (Microsoft.Win32.RegistryKey MyReg = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(registryPath, Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl))
                {
                    if (MyReg != null)
                    {
                        keyvaluedata = string.Format("{0}", MyReg.GetValue("activation_key", "").ToString());

                        if (keyvaluedata.Length == 0)
                        {
                            //activate
                            inform_user_need_to_activate();
                        }
                        else
                        {
                            //check for expiry
                            string mac_address = string.Format("{0}", MyReg.GetValue("mac_address", "").ToString());
                            string computer_name = string.Format("{0}", MyReg.GetValue("computer_name", "").ToString());
                            string activation_key = string.Format("{0}", MyReg.GetValue("activation_key", "").ToString());
                            string date_activated = string.Format("{0}", MyReg.GetValue("date_activated", "").ToString());
                            string next_expiry_date = string.Format("{0}", MyReg.GetValue("next_expiry_date", "").ToString());
                            string days_for_expiry = string.Format("{0}", MyReg.GetValue("days_for_expiry", "").ToString());

                            DateTime dateactivate = DateTime.Parse(date_activated);
                            DateTime dateexpiry = DateTime.Parse(next_expiry_date);
                            DateTime today = DateTime.Now;

                            int difference_from = dateactivate.Subtract(today).Days;
                            Console.WriteLine(difference_from);

                            int difference_to = dateexpiry.Subtract(today).Days;
                            Console.WriteLine(difference_to);

                            string str_difference_from = difference_from.ToString();
                            str_difference_from = str_difference_from.Replace("-", "");

                            lbl_info.Visible = true;
                            lbl_info.Text = " R     " + difference_to.ToString() + "         E     " + str_difference_from.ToString() + "  ";

                            //expired
                            if (difference_to <= 0)
                            {
                                MyReg.DeleteValue("mac_address");
                                MyReg.DeleteValue("computer_name");
                                MyReg.DeleteValue("activation_key");
                                MyReg.DeleteValue("date_activated");
                                MyReg.DeleteValue("next_expiry_date");
                                MyReg.DeleteValue("days_for_expiry");

                                inform_user_need_to_activate();
                            }
                            else if (difference_to <= 30)
                            {
                                lbl_info.Text += "  License remaining [ " + difference_to + " ] days to expiration.  ";
                                lbl_info.BackColor = Color.Red;
                                lbl_info.ForeColor = Color.White;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        //activate
                        inform_user_need_to_activate();
                    }
                }

            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Log.WriteToErrorLogFile_and_EventViewer(ex);
            }
        }

        private void inform_user_need_to_activate()
        {
            try
            {
                Invoke(new System.Action(() =>
                {
                    //DisableApplicationMenus();
                }));
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Log.WriteToErrorLogFile_and_EventViewer(ex);
            }
        }

        private bool ExecuteIPConfigCommands()
        {
            try
            {
                System.Diagnostics.ProcessStartInfo sdpsinfo = new System.Diagnostics.ProcessStartInfo("ipconfig.exe", "-all");
                // The following commands are needed to
                //redirect the standard output.
                // This means that it will //be redirected to the
                // Process.StandardOutput StreamReader
                // u can try other console applications
                //such as  arp.exe, etc
                sdpsinfo.RedirectStandardOutput = true;
                // redirecting the standard output
                sdpsinfo.UseShellExecute = false;
                // without that console shell window
                sdpsinfo.CreateNoWindow = true;
                // Now we create a process,
                //assign its ProcessStartInfo and start it
                System.Diagnostics.Process p =
                new System.Diagnostics.Process();
                p.StartInfo = sdpsinfo;
                p.Start();
                // well, we should check the return value here...
                //  capturing the output into a string variable...
                string res = p.StandardOutput.ReadToEnd();
                _template += res;

                Debug.Write(res);
                Log.WriteToErrorLogFile_and_EventViewer(new Exception(res));
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(res, TAG));

                return true;
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Utils.LogEventViewer(ex);
                return false;
            }
        }
        private bool FindComputersConectedToHost()
        {
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo("cmd", "/c netstat -a");
                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                // Get the output into a string
                string res = proc.StandardOutput.ReadToEnd();
                _template += res;

                Debug.Write(res);
                Log.WriteToErrorLogFile_and_EventViewer(new Exception(res));
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(res, TAG));

                return true;
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Utils.LogEventViewer(ex);
                return false;
            }
        }
        private bool GetHostNameandMac()
        {
            try
            {
                System.Diagnostics.ProcessStartInfo sdpsinfo = new System.Diagnostics.ProcessStartInfo("cmd.exe", "Getmac,Hostname");
                // The following commands are needed to
                //redirect the standard output.
                // This means that it will //be redirected to the
                // Process.StandardOutput StreamReader
                // u can try other console applications
                //such as  arp.exe, etc
                sdpsinfo.RedirectStandardOutput = true;
                // redirecting the standard output
                sdpsinfo.UseShellExecute = false;
                // without that console shell window
                sdpsinfo.CreateNoWindow = true;
                // Now we create a process,
                //assign its ProcessStartInfo and start it
                System.Diagnostics.Process p =
                new System.Diagnostics.Process();
                p.StartInfo = sdpsinfo;
                p.Start();
                // well, we should check the return value here...
                //  capturing the output into a string variable...
                string res = p.StandardOutput.ReadToEnd();
                _template += res;

                Debug.Write(res);
                Log.WriteToErrorLogFile_and_EventViewer(new Exception(res));
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(res, TAG));

                return true;
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Utils.LogEventViewer(ex);
                return false;
            }
        }
        private bool GetClientExtraInfo()
        {
            try
            {
                System.Diagnostics.ProcessStartInfo sdpsinfo =
 new System.Diagnostics.ProcessStartInfo
 ("cmd.exe", " NBTSTAT -n,WHOAMI, VER, TASKLIST, GPRESULT /r, NETSTAT,  Assoc, Arp -a");
                // The following commands are needed to
                //redirect the standard output.
                // This means that it will //be redirected to the
                // Process.StandardOutput StreamReader
                // u can try other console applications
                //such as  arp.exe, etc
                sdpsinfo.RedirectStandardOutput = true;
                // redirecting the standard output
                sdpsinfo.UseShellExecute = false;
                // without that console shell window
                sdpsinfo.CreateNoWindow = true;
                // Now we create a process,
                //assign its ProcessStartInfo and start it
                System.Diagnostics.Process p =
                new System.Diagnostics.Process();
                p.StartInfo = sdpsinfo;
                p.Start();
                // well, we should check the return value here...
                //  capturing the output into a string variable...
                string res = p.StandardOutput.ReadToEnd();
                _template += res;

                Debug.Write(res);
                Log.WriteToErrorLogFile(new Exception(res));
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(res, TAG));

                return true;
            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Utils.LogEventViewer(ex);
                return false;
            }
        }
        private void institutionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            institutions_form instf = new institutions_form();
            instf.Show();
        }

        private void clientsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clients_form clief = new clients_form();
            clief.Show();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            about_form abtf = new about_form();
            abtf.Show();
        }

        private void contactUsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            contact_us_form cuf = new contact_us_form();
            cuf.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void usersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //users_form usf = new users_form();
            //usf.Show();
        }

        private void btninstitutions_Click(object sender, EventArgs e)
        {
            institutionsToolStripMenuItem_Click(sender, e);
        }

        private void btnclients_Click(object sender, EventArgs e)
        {
            clientsToolStripMenuItem_Click(sender, e);
        }

        private void btnusers_Click(object sender, EventArgs e)
        {
            usersToolStripMenuItem_Click(sender, e);
        }

        private void btnexit_Click(object sender, EventArgs e)
        {
            exitToolStripMenuItem_Click(sender, e);
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                string help_file = "help.html";

                string base_directory = AppDomain.CurrentDomain.BaseDirectory;
                string help_path = Path.Combine(base_directory, "help");
                string help_file_path = Path.Combine(help_path, help_file);

                FileInfo fi = new FileInfo(help_file_path);

                if (fi.Exists)
                    this.webBrowser.Navigate(fi.FullName);
            }
            catch (Exception ex)
            {
                Utils.LogEventViewer(ex);
            }
        }





    }
}
