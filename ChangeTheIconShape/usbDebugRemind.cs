using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ChangeTheIconShape.EditDevice;

namespace ChangeTheIconShape
{
    public partial class usbDebugRemind : UserControl
    {
        public usbDebugRemind()
        {
            InitializeComponent();
        }


        public string ConnectedDevice
        {
            get; private set;
        }

        static readonly string detectNoDaemonString = "daemon not running";
        static readonly string detectNoDaemonString2 = "server is out of date";

        static readonly string detectPhoneName = "shell settings get global device_name";

        static readonly string detectDevicesString = "List of devices attached";

        public struct commandResponse
        {
            public string content;
            public bool daemonRunning
            {
                get
                {
                    return (content.StartsWith(detectNoDaemonString) || content.StartsWith(detectNoDaemonString2)) == false;
                }
            }
        }

        void setState(byte state)
        {
            if (state == 0)
            {
                setState("Checking for devices..");
            }
            else if (state == 2)
            {
                setState("No device detected.");
            }
        }

        void setState(byte state, Color textColor)
        {
            if (state == 0)
            {
                setState("Checking for devices.", textColor);
            }
            else if (state == 2)
            {
                setState("Starting ADB server..", textColor);
            }
        }

        void setState(string state, Color textColor)
        {
            cuiLabel4.ForeColor = textColor;
            cuiLabel4.Content = state;
        }

        void setState(string state)
        {
            cuiLabel4.ForeColor = Color.Gray;
            cuiLabel4.Content = state;
        }

        static string createAdbCommand(string command)
        {
            return "\"" + "\"" + Directory.GetCurrentDirectory() + "\\adb\\adb.exe" + "\" " + command + "\"";
        }

        struct boolStringStruct
        {
            public bool boolValue;
            public string content;
        }

        const string command_getIconShapes = "shell cmd overlay list | findstr \"com.android.theme.icon\"";

        async Task<boolStringStruct> detectIfOnlyOneDeviceConnected(string responseMessage)
        {
            if (responseMessage.StartsWith(detectDevicesString))
            {
                responseMessage = responseMessage.Substring(detectDevicesString.Length - 1);
                string[] devices = responseMessage.Split('\n');

                List<string> actualDevices = new List<string>();

                foreach (string device in devices)
                {
                    if (device.Trim().EndsWith("device"))
                    {
                        string sanitizedDevice = (device.Trim().Substring(0, device.Length - "device".Length - 1)).Trim();
                        if (sanitizedDevice != "")
                        {
                            actualDevices.Add(sanitizedDevice);
                        }
                    }
                }

                if (actualDevices.Count == 1)
                {
                    if (!await checkIfSupports())
                    {
                        (Form1.ActiveForm as Form1)?.NoSupport();
                    }

                    return new boolStringStruct() { content = actualDevices.First(), boolValue = true };
                }
            }

            return new boolStringStruct() { boolValue = false };
        }

        private async Task<bool> checkIfSupports()
        {
            commandResponse iconShapesResponse = await RunCommand(command_getIconShapes);
            if (iconShapesResponse.daemonRunning)
            {
                foreach (string _ in iconShapesResponse.content.Split('\n'))
                {
                    if (_.Contains("com.android.theme.icon."))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool previouslyDetected = false;

        private async void cuiLabel4_Load(object sender, EventArgs e)
        {
            setState(0);
            while (!IsDisposed)
            {
                commandResponse devicesResponse = await runCommand("devices");
                boolStringStruct deviceCheckResponse = await detectIfOnlyOneDeviceConnected(devicesResponse.content);

                if (devicesResponse.daemonRunning && deviceCheckResponse.boolValue)
                {
                    bool detectedNow = cuiLabel4.Content.StartsWith("Connected");

                    if (detectedNow != previouslyDetected && detectedNow == false)
                    {
                        detectedLabel.Visible = true;
                    }
                    else
                    {
                        detectedLabel.Visible = false;
                    }


                    previouslyDetected = false;

                    if (deviceCheckResponse.boolValue)
                    {

                        commandResponse nameResponse = await runCommand(detectPhoneName);
                        if (nameResponse.daemonRunning)
                        {
                            Enabled = true;


                            previouslyDetected = true;

                            ConnectedDevice = deviceCheckResponse.content;

                            cuiPictureBox1.ImageTint = Color.FromArgb(0, 101, 255);
                            cuiButton1.NormalBackground = Color.FromArgb(0, 101, 255);

                            //MessageBox.Show(nameResponse.content);
                            setState(("Connected to " + deviceCheckResponse.content + $" ({nameResponse.content})!").Replace("\n", ""), Color.White);

                            detectedLabel.Visible = false;
                        }
                    }
                    else
                    {
                        ConnectedDevice = "";

                        cuiButton1.NormalBackground = Color.FromArgb(32, 32, 32);
                        cuiPictureBox1.ImageTint = Color.White;
                        setState(0, Color.Gray);
                    }
                }
                else
                {

                    ConnectedDevice = "\n";
                    previouslyDetected = false;
                    setState(2);
                }

                previouslyDetected = devicesResponse.daemonRunning;
            }
        }

        public static async Task<commandResponse> runCommand(string inputCommand)
        {
            string formattedCommand = createAdbCommand(inputCommand);

            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {formattedCommand}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (Debugger.IsAttached)
            {
                //Clipboard.SetText(formattedCommand);
                //MessageBox.Show($"Running {formattedCommand}");
            }

            commandResponse response = new commandResponse();

            using (Process process = new Process { StartInfo = processInfo, EnableRaisingEvents = true })
            {
                StringBuilder output = new StringBuilder();
                StringBuilder error = new StringBuilder();

                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                        output.AppendLine(args.Data);
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                        error.AppendLine(args.Data);
                };

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await Task.Delay(1000);

                response.content = output.ToString();

                if (error.Length > 0)
                {
                    response.content += $"\nErrors:\n{error}";
                    (Form1.ActiveForm as Form1)?.NoSupport();
                }
            }

            return response;
        }

    }
}
