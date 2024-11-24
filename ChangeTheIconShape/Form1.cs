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
            NewEdit();
            usbReminderControl.Dispose();

        }

        bool alreadyEditedDevice = false;

        public void NewEdit()
        {
            alreadyEditedDevice = true;
            ChangeScene(new EditDevice());
        }

        public void NoSupport()
        {
            ChangeScene(new DeviceNotSupported());
        }

        private void ChangeScene(Control targetControl)
        {
            controlContainer.Controls.Add(targetControl);

            if (controlContainer.Controls.Count > 1)
            {

                var oldControl = controlContainer.Controls[0];
                controlContainer.Controls.Remove(oldControl);

                try
                {
                    oldControl.Dispose();
                }
                finally
                {

                }

                AnimateTransition();
            }
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
            if (usbReminderControl != null && usbReminderControl.cuiButton1 != null)
            {
                usbReminderControl.cuiButton1.Click -= Form1_Click;
                usbReminderControl.Dispose();
            }

            usbReminderControl = new usbDebugRemind();
            usbReminderControl.cuiButton1.Click += Form1_Click;
            ChangeScene(usbReminderControl);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (alreadyEditedDevice && (controlContainer.Controls.Count > 0) == false)
            {
                NewEdit();
            }
        }
    }
}
