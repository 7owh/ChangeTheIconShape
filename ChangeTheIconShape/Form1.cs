using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChangeTheIconShape
{
    public partial class Form1 : Form
    {
        usbDebugRemind usbReminderControl;
        public Form1()
        {
            InitializeComponent();
            StartOver();
        }

        private void Form1_Click(object sender, EventArgs e)
        {
            usbReminderControl.cuiButton1.Click -= Form1_Click;
            NewEdit(usbReminderControl?.device_name_label.Content);
            usbReminderControl?.Dispose();

        }

        public void NewEdit(string device)
        {
            ChangeScene(new EditDevice(device));
        }

        public void NoSupport(string deviceName)
        {
     
                ChangeScene(new DeviceNotSupported());
            
        }

        private void ChangeScene(Control targetControl)
        {
            if (controlContainer.Controls.Count > 1)
            {

                var oldControl = controlContainer.Controls[0];

                try
                {
                    oldControl?.Dispose();
                }
                finally
                {
                }

                controlContainer.Controls?.Clear();
            }


            controlContainer.Controls.Add(targetControl);

            AnimateTransition();
        }

        private void cuiButton1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cuiButton2_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        public async void AnimateTransition()
        {
            cuiControlAnimator1.TargetControl.Location = cuiControlAnimator1.TargetLocation;
            await Task.Delay(CuoreUI.Drawing.LazyInt32TimeDelta);
            cuiControlAnimator1.TargetControl.Location = new System.Drawing.Point(8, 32);
            await cuiControlAnimator1.AnimateLocation();
        }

        public void StartOver()
        {
            controlContainer.Controls.Clear();

            if (usbReminderControl != null)
            {
                usbReminderControl?.Dispose();
            }

            usbReminderControl = new usbDebugRemind();
            usbReminderControl.cuiButton1.Click += Form1_Click;
            ChangeScene(usbReminderControl);
        }
    }
}
