using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ChangeTheIconShape.usbDebugRemind;

#pragma warning disable CS0649

namespace ChangeTheIconShape
{
    public partial class EditDevice : UserControl
    {
        public EditDevice()
        {
            InitializeComponent();
        }

        const string command_getIconShapes = "shell cmd overlay list | findstr \"com.android.theme.icon\"";

        public struct IconShape
        {
            public string name;
            private bool privateSelected;
            public bool Selected
            {
                get
                {
                    return privateSelected;
                }
                set
                {
                    privateSelected = value;
                    _ = ToggleShape(name, value);
                }
            }
        }

        async static Task ToggleShape(string shapename, bool enable)
        {
            string tempShapeName = "com.android.theme.icon." + shapename;

            string command_toggleShape;
            if (enable)
            {
                command_toggleShape = "shell cmd overlay enable ";
            }
            else
            {
                command_toggleShape = "shell cmd overlay disable ";
            }

            command_toggleShape += tempShapeName;

            await RunCommand(command_toggleShape);
        }

        const string selectedString = "[x]";
        const string deselectedString = "[ ]";

        List<IconShape> iconShapes = new List<IconShape>();

        bool pleaseStop = false;

        private async void EditDevice_Load(object sender, EventArgs e)
        {
            Enabled = false;

            retrievalContainer.Visible = true;
            cuiLabel2.Content = "Retrieving available shapes..";

            while (!IsDisposed)
            {

                List<IconShape> tempIconShapes = new List<IconShape>();

                commandResponse iconShapesResponse = await RunCommand(command_getIconShapes);
                if (iconShapesResponse.daemonRunning)
                {
                    foreach (string _ in iconShapesResponse.content.Split('\n'))
                    {
                        if (!_.Contains("com.android.theme.icon."))
                        {
                            continue;
                        }

                        IconShape iconShape = new IconShape();

                        string shapeString = _.Trim();

                        if (shapeString == "")
                        {
                            continue;
                        }

                        if (shapeString.StartsWith(selectedString))
                        {
                            iconShape.Selected = true;
                            shapeString = shapeString.Substring(selectedString.Length);

                        }
                        else if (shapeString.StartsWith(deselectedString))
                        {
                            iconShape.Selected = false;
                            shapeString = shapeString.Substring(deselectedString.Length);

                        }

                        shapeString = shapeString.Trim().Substring("com.android.theme.icon.".Length);

                        iconShape.name = shapeString;

                        tempIconShapes.Add(iconShape);
                    }
                }

                if (pleaseStop == false)
                {
                    retrievalContainer.Visible = false;
                    Enabled = true;
                }

                if (ListsAreDifferent(iconShapes, tempIconShapes))
                {
                    iconShapes = tempIconShapes;
                    cuiListbox1.Items.Clear();

                    foreach (IconShape shape in iconShapes)
                    {
                        cuiListbox1.Items.Add(shape.name);
                        if (shape.Selected)
                        {
                            cuiListbox1.SelectedItem = shape.name;
                            SelectedIconShape = shape.name;
                        }
                    }

                    CheckIfShapeExistsInResourcesAndIfSoSetImage(cuiListbox1.SelectedItem as string);
                }

                if (tempIconShapes.Count < 1)
                {
                    (Form1.ActiveForm as Form1)?.NoSupport();
                    Dispose();
                }
                else
                {
                    if (pleaseStop == false)
                    {
                        Enabled = true;
                    }
                }

                await Task.Delay(1000);
            }
        }

        string SelectedIconShape = "";

        private void CheckIfShapeExistsInResourcesAndIfSoSetImage(string selectedItem)
        {
            System.Resources.ResourceManager resourceManager = Properties.Resources.ResourceManager;
            var resourceNames = resourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, true, true);

            foreach (System.Collections.DictionaryEntry entry in resourceNames)
            {
                if (entry.Value is Bitmap bmp)
                {
                    if (entry.Key.ToString().Equals(selectedItem, StringComparison.OrdinalIgnoreCase))
                    {
                        cuiPictureBox1.Content = bmp;
                        break;
                    }
                }
            }

            cuiLabel3.Content = $"APPLIED: {SelectedIconShape}\nSELECTED: {selectedItem}";
        }


        private bool ListsAreDifferent(List<IconShape> iconShapes, List<IconShape> tempIconShapes)
        {
            return !iconShapes.Any(icon => tempIconShapes.Any(temp => icon.name == temp.name && icon.Selected == temp.Selected));
        }

        static string CreateAdbCommand(string command)
        {
            return "\"" + "\"" + Directory.GetCurrentDirectory() + "\\adb\\adb.exe" + "\" " + command + "\"";
        }

        struct BoolStringStruct
        {
            public bool BoolValue;
            public string Content;
        }

        public static async Task<commandResponse> RunCommand(string inputCommand)
        {
            string formattedCommand = CreateAdbCommand(inputCommand);

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
                //.SetText(formattedCommand);
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
                }
            }

            return response;
        }

        private void cuiListbox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckIfShapeExistsInResourcesAndIfSoSetImage(cuiListbox1.SelectedItem as string);
        }

        public Panel controlContainer
        {
            get
            {
                foreach (Control c in Form1.ActiveForm.Controls)
                {
                    if (c is Panel || c.Tag.ToString() == "this")
                    {
                        return c as Panel;
                    }
                }

                return null;
            }
        }

        private async void cuiButton1_Click(object sender, EventArgs e)
        {
            Enabled = false;

            retrievalContainer.Visible = true;
            pleaseStop = true;

            int i = 1;

            _ = ToggleShape(cuiListbox1.SelectedItem as string, true);

            foreach (IconShape shape in iconShapes)
            {
                cuiLabel2.Content = $"Applying, please wait! ({i}/{iconShapes.Count})";

                if (shape.name != (cuiListbox1.SelectedItem as string))
                {
                    await ToggleShape(shape.name, false);
                }
                i++;
            }

            while (!IsDisposed)
            {
                (Form1.ActiveForm as Form1)?.NewEdit();
                this?.Dispose();
            }
        }
    }
}
