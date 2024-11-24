using System.Windows.Forms;

namespace ChangeTheIconShape
{
    public partial class DeviceNotSupported : UserControl
    {
        public DeviceNotSupported()
        {
            InitializeComponent();
        }

        private void cuiButton1_Click(object sender, System.EventArgs e)
        {
            (Form1.ActiveForm as Form1)?.StartOver();
        }
    }
}
