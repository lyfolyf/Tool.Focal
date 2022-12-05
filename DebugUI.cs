using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lead.Tool.Focal
{
    public partial class DebugUI : UserControl
    {
        FocalTool _Proxy;
        public DebugUI(FocalTool proxy)
        {
            InitializeComponent();
            _Proxy = proxy;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _Proxy.Init();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            _Proxy.Start();
        }

        private void buttonTerminate_Click(object sender, EventArgs e)
        {
            _Proxy.Terminate();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_Proxy.State == Interface.IToolState.ToolInit)
            {
                buttonInit.BackColor = Color.Green;
                buttonStart.BackColor = Color.Gray;
                buttonTerminate.BackColor = Color.Gray;
            }
            if (_Proxy.State == Interface.IToolState.ToolRunning)
            {
                buttonInit.BackColor = Color.Green;
                buttonStart.BackColor = Color.Green;
                buttonTerminate.BackColor = Color.Gray;
            }
            if (_Proxy.State == Interface.IToolState.ToolTerminate)
            {
                buttonInit.BackColor = Color.Gray;
                buttonStart.BackColor = Color.Gray;
                buttonTerminate.BackColor = Color.Red;
            }
        }
    }
}
