using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace csipc
{
    /// <summary>
    /// Implementation of process communication via WM_COPYDATA
    /// </summary>
    public class IPC : NativeWindow
    {
        /// <summary> Delegate for IPC.OnMessage event. </summary>
        /// <param name="message">Message that was received</param>
        /// <param name="port">Port that received the message</param>
        public delegate void MessageAction(string message, int port);

        /// <summary>Event fired when message arrives</summary>
        public event MessageAction OnMessage;

        #region Private fields
        IntPtr id = new IntPtr(951753);
        const int WM_COPYDATA = 74;

        [StructLayout(LayoutKind.Sequential)]
        struct COPYDATASTRUCT
        {
            public IntPtr dwData; // int > IntPtr (x64)
            public int cbData;
            public IntPtr lpData; // int > IntPtr (x64)
        }

        #endregion

        #region Win32 imports
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);
        #endregion

        ///<summary> Creates IPC object. </summary>
        ///<param name="host">Form object that will monitor and accept communication with other process</param>
        public IPC()
        {
            // make it the appropriate
            IntPtr handle = (IntPtr)FindWindow(null, "Windows PowerShell ISE"); 
            this.AssignHandle(handle);
        }
        public IPC(Form host) { this.AssignHandle(host.Handle); }
        public IPC(IntPtr hWnd) { this.AssignHandle(hWnd); }


        ///<summary> Find window by title </summary>
        ///<param name="WinTitle">Window title, case insensitive</param>
        public static IntPtr WinExist(string WinTitle)
        {
            return FindWindow(null, WinTitle);
        }

        public static string ToShiftJis(string unicodeStrings)
        {
            Encoding unicode = Encoding.Unicode;
            byte[] unicodeByte = unicode.GetBytes(unicodeStrings);
            Encoding s_jis = Encoding.GetEncoding("shift_jis");
            byte[] s_jisByte = Encoding.Convert(unicode, s_jis, unicodeByte);
            char[] s_jisChars = new char[s_jis.GetCharCount(s_jisByte, 0, s_jisByte.Length)];
            s_jis.GetChars(s_jisByte, 0, s_jisByte.Length, s_jisChars, 0);
            return new string(s_jisChars);
        } 

        ///<summary>Send the message to another process (receiver) using WM_COPYDATA.</summary>
        ///<param name="hHost">Handle of the receiver</param>
        ///<param name="msg">Message to be sent</param>
        ///<param name="port">Port on which to send the message</param>
        public bool Send(IntPtr hHost, string msg, int port)
        {
            COPYDATASTRUCT cd = new COPYDATASTRUCT();
            cd.dwData = new IntPtr(port);
            // A Unicode string consists of 2 bytes per character plus a 2-byte null terminator.
            cd.cbData = (msg.Length + 1) * 2; 
            
            // stop calling ToInt32() and keep it as an IntPtr. (x64)
            IntPtr pText = Marshal.StringToHGlobalUni(msg);
            try
            {
                cd.lpData = pText;
                // It's safer to receive the return value of SendMessage as an IntPtr as well.
                IntPtr result = SendMessage(hHost, WM_COPYDATA, id, ref cd);
                return result != IntPtr.Zero;
            }
            finally
            {
                // Release memory after transmission is complete.
                Marshal.FreeHGlobal(pText);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_COPYDATA && m.WParam == id)
            {
                COPYDATASTRUCT cd = (COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(COPYDATASTRUCT));
                // The second argument, specifies the number of characters excluding the terminating character.
                string data = Marshal.PtrToStringUni(cd.lpData, (cd.cbData / 2) - 1);

                if (OnMessage != null)
                    OnMessage(data, cd.dwData.ToInt32());
                else
                    MessageBox.Show("Received: " + data);
                
                m.Result = new IntPtr(1); // set an appropriate value
                return;
            }
            base.WndProc(ref m);
        }
    }
}
