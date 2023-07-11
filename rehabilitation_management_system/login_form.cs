using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using dal;

namespace rehabilitation_management_system
{
    public partial class login_form : Form
    {
        DateTime startDate = DateTime.Now;
        public string TAG;

        // Timers namespace rather than Threading
        System.Timers.Timer elapsed_timer = new System.Timers.Timer(); // Doesn't require any args
        private int _TimeCounter = 0;
        DateTime _startDate = DateTime.Now;

        public List<notificationdto> _lstnotificationdto = new List<notificationdto>();

        private event EventHandler<notificationmessageEventArgs> _notificationmessageEventname;
        public login_form()
        {
            InitializeComponent();

            TAG = this.GetType().Name;

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);
            Application.ThreadException += new ThreadExceptionEventHandler(ThreadException);

            //Subscribing to the event: 
            //Dynamically:
            //EventName += HandlerName;
            _notificationmessageEventname += notificationmessageHandler;

            _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs("finished login_form initialization", TAG));
        }

        private void login_form_Load(object sender, EventArgs e)
        {
            string title = "login - Copyright ©  " + DateTime.Now.Year.ToString() + " - All Rights Reserved";
            this.Text = title;

            this.txtusername.Focus();

            txtusername.Text = "admin";
            txtpassword.Text = "admin";

            //initialize current running time timer
            elapsed_timer.Interval = 1000;
            elapsed_timer.Elapsed += elapsed_timer_Elapsed; // Uses an event instead of a delegate
            elapsed_timer.Start(); // Start the timer

            //app version
            var _buid_version = Application.ProductVersion;
            groupBox1.Text = "build version - " + _buid_version;

            _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs("finished login_form load", TAG));

        }
        private void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            this._notificationmessageEventname.Invoke(sender, new notificationmessageEventArgs(ex.Message, TAG));
        }

        private void ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            this._notificationmessageEventname.Invoke(sender, new notificationmessageEventArgs(ex.Message, TAG));
        }

        //Event handler declaration:
        public void notificationmessageHandler(object sender, notificationmessageEventArgs args)
        {
            try
            {
                /* Handler logic */

                //Invoke(new Action(() =>
                //{

                notificationdto _notificationdto = new notificationdto();

                DateTime currentDate = DateTime.Now;
                String dateTimenow = currentDate.ToString("dd-MM-yyyy HH:mm:ss tt");

                String _logtext = Environment.NewLine + "[ " + dateTimenow + " ]   " + args.message;

                _notificationdto._notification_message = _logtext;
                _notificationdto._created_datetime = dateTimenow;
                _notificationdto.TAG = args.TAG;

                Log.WriteToErrorLogFile(new Exception(args.message)); 

                _lstnotificationdto.Add(_notificationdto);

                var _lstmsgdto = from msgdto in _lstnotificationdto
                                 orderby msgdto._created_datetime descending
                                 select msgdto._notification_message;

                String[] _logflippedlines = _lstmsgdto.ToArray();

                txtlog.Lines = _logflippedlines;
                txtlog.ScrollToCaret();

                //}));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        private void elapsed_timer_Elapsed(object sender, EventArgs e)
        {
            try
            {
                _TimeCounter++;
                DateTime nowDate = DateTime.Now;
                TimeSpan t = nowDate - _startDate;
                lbltimelapsed.Text = string.Format("{0} : {1} : {2} : {3}", t.Days, t.Hours, t.Minutes, t.Seconds);

                DateTime currentDate = DateTime.Now;
                String dateTimenow = currentDate.ToString("dd-MM-yyyy HH:mm:ss tt");

                lblrunningtime.Text = dateTimenow;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private void loginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loginToolStripMenuItem.Text = "loging in...";
            try
            {
                string username = "";
                string password = "";
                string msg = "";

                if (!string.IsNullOrEmpty(txtusername.Text))
                {
                    username = txtusername.Text;
                }
                else
                {
                    msg = "username cannot be null.";
                    _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(msg, TAG));
                    errorProvider.SetError(txtusername, "username cannot be null.");
                }

                if (!string.IsNullOrEmpty(txtpassword.Text))
                {
                    password = txtpassword.Text;
                }
                else
                {
                    msg = "password cannot be null.";
                    _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(msg, TAG));
                    errorProvider.SetError(txtpassword, "password cannot be null.");
                }

                if (msg.Length > 0)
                {
                    _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs("validation error...", TAG));
                    loginToolStripMenuItem.Text = "login";
                    this.txtusername.Focus();

                    return;
                }

                //weight_ui_dto _weight_ui_dto = new weight_ui_dto();
                //_weight_ui_dto.weight_weight = weight;
                //_weight_ui_dto.weight_date = date;
                //_weight_ui_dto.weight_app = "c-sharp";

                responsedto response_dto = new responsedto();

                //  response_dto = dalutilz.createweightingservice(_weight_ui_dto);
                //string response_msg = response_dto.responsesuccessmessage + "\n" + response_dto.responseerrormessage;

                if (response_dto.isresponseresultsuccessful)
                {
                    //helper.NotifyMessage("success", response_msg);
                    //helper.LogEventViewer(new Exception(response_msg));

                    //_notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(response_msg, TAG));

                    //MessageBox.Show(response_msg, "create record", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    main_form _main_form = new main_form() { Owner = this };
                    _main_form.Show();
                    this.Hide();

                }
                else
                {
                    this.txtusername.Focus();

                    //helper.NotifyMessage("error", response_msg);
                    //helper.LogEventViewer(new Exception(response_msg));

                    //_notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(response_msg, TAG));

                    //MessageBox.Show(response_msg, "create record", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                main_form _main__form = new main_form() { Owner = this };
                _main__form.Show();
                this.Hide();

            }
            catch (Exception ex)
            {
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                Utils.ShowError(ex);
            }
            loginToolStripMenuItem.Text = "login";

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnlogin_Click(object sender, EventArgs e)
        {
            loginToolStripMenuItem_Click(sender, e);
        }

        private void btnexit_Click(object sender, EventArgs e)
        {
            exitToolStripMenuItem_Click(sender, e);
        }
    }
}
