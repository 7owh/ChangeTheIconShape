using CuoreUI.Controls;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ChangeTheIconShape.usbDebugRemind;

namespace ChangeTheIconShape
{
    public partial class DeviceNotSupported : UserControl
    {
        public DeviceNotSupported()
        {
            InitializeComponent();
            InitializeText();
        }

        string device_name = "";

        public DeviceNotSupported(string deviceName)
        {
       
            InitializeComponent();
            InitializeText();
        }

        private async void InitializeText()
        {
            if (device_name.Trim() == "")
            {
                if (Form1.ActiveForm != null)
                {
                    var form = (Form1.ActiveForm as Form1);

                    form.controlContainer.Visible = false;
                    device_name = await getDeviceName();
                    form.controlContainer.Visible = true;
                }
                else
                {
                    device_name = await getDeviceName();
                }
            }

            cuiLabel3.Content = (Regex.Unescape(cuiLabel3.Content).Replace("{device}", device_name)).Replace("\n'", "'");
        }

        private async Task<string> getDeviceName()
        {
            commandResponse nameResponse = await runCommand(detectPhoneName);

            if (nameResponse.daemonRunning)
            {
                return nameResponse.content;
            }
            else
            {
                if (!IsDisposed)
                {
                    (Form1.ActiveForm as Form1)?.StartOver();
                    Dispose();
                }

                return "";
            }
        }

        static readonly string detectPhoneName = "shell settings get global device_name";

        private void cuiButton1_Click(object sender, System.EventArgs e)
        {
            if (!IsDisposed)
            {
                (Form1.ActiveForm as Form1)?.StartOver();
                Dispose();
            }
        }
    }
}
