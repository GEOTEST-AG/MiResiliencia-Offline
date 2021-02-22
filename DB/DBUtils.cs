using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.DB
{
    /// <summary>
    /// Handling the local portable Postgres DB
    /// </summary>
    public sealed class DBUtils
    {
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] uint Msg, int wParam, int lParam);
        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP = 0x0101;
        const int WM_CHAR = 0x0102;

        private static System.Diagnostics.Process _process { get; set; }
        private IntPtr hWnd;

        private static DBUtils instance = null;
        private static readonly object padlock = new object();

        DBUtils()
        {
        }

        public static DBUtils Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new DBUtils();
                    }
                    return instance;
                }
            }
        }

        /// <summary>
        /// Start local portable Postgres DB
        /// </summary>
        public void StartLocalDB()
        {
            // check if localDB is already running
            try
            {
                NpgsqlConnection conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["ResTBLocalDB"].ConnectionString);
                conn.Open();
                if (conn.State == System.Data.ConnectionState.Open) return;
            }
            catch (Exception e)
            {
                // localDB not yet startet
            }
            string postablePSDir = ConfigurationManager.AppSettings["PortablePostgreSQLDirectory"];
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ResTBDesktop";
            postablePSDir = postablePSDir.Replace("{appdata}", appdata);

            // start the portable PostgreSQL
            _process = new System.Diagnostics.Process();
            ProcessStartInfo si = new System.Diagnostics.ProcessStartInfo();
            //si.CreateNoWindow = true;
            si.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            si.FileName = postablePSDir + "\\PostgreSQLPortable.exe";
            si.UseShellExecute = false;
            
            si.RedirectStandardInput = true;
            si.RedirectStandardOutput = true;
            _process.StartInfo = si;
            _process.Start();

            // hide the PostgreSQL Portable-window

            // Wait until ready
            bool ready = false;
            while(!ready)
            {
                try
                {
                    NpgsqlConnection conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["ResTBLocalDB"].ConnectionString);
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                        ready = true;
                    conn.Close();

                    hWnd = WndSearcher.SearchForWindow("ConsoleWindowClass","PostgreSQL Portable");
                    ShowWindow(hWnd, 0);
                }
                catch (Exception ex)
                {
                    // not yet startet
                }
            }
        }

        /// <summary>
        /// Stop local portable Postgres DB
        /// </summary>
        public void StopLocalDB()
        {
            if (_process == null) return;

            uint wparam = 0 << 29 | 0;
            string msg = @"\q";
            int i = 0;
            for (i = 0; i < msg.Length; i++)
            {
                //PostMessage(child, WM_KEYDOWN, (IntPtr)Keys.Enter, (IntPtr)wparam);
                PostMessage(hWnd, WM_CHAR, (int)msg[i], 0);
            }
            PostMessage(hWnd, WM_KEYDOWN, (IntPtr)13, (IntPtr)wparam);

            _process.WaitForExit();
        }
    }
}
