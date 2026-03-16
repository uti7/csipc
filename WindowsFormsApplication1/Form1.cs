using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using csipc;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private csipc.IPC ci;

        public Form1()
        {
            InitializeComponent();
            ci = new csipc.IPC(this);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string str = "hello, mr. interop";
            IntPtr h = csipc.IPC.WinExist("csipc_test.ahk");
            ci.Send(h, str, 96);
        }
    }
}
