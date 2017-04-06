using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CodeLib
{
    public struct item
    {
        public String title;
        public String language;
        public String keywords;
        public String code;
    }
    
    public partial class Form1 : Form
    {
        private int port = 27410;
        private String IPAddress = "192.168.1.104";
        private int BUFFER_SIZE = (1 << 20);

        private const char DELIMITER = '~';

        private String lang = String.Empty;
        private String title = String.Empty;
        private String keys = String.Empty;

        List<item> items = new List<item>();

        public Form1()
        {
            InitializeComponent();
            base.OnResize(new EventArgs() );
            run();
        }

        private void queryKeys_TextChanged(object sender, EventArgs e)
        {
            keys = queryKeys.Text;
            dataHolder.newDataReady = true;
        }

        private void queryTitle_TextChanged(object sender, EventArgs e)
        {
            title = queryTitle.Text;
            dataHolder.newDataReady = true;
        }

        private void queryLanguage_TextChanged(object sender, EventArgs e)
        {
            lang = queryLanguage.Text;
            dataHolder.newDataReady = true;
        }

        private void run()
        {
            TcpClient clientSocket = new TcpClient();
            clientSocket.Connect(IPAddress,port);
            NetworkStream stream = clientSocket.GetStream();
            UTF8Encoding encode = new UTF8Encoding();

            byte[] bytesFrom = new byte[BUFFER_SIZE];

            Thread updater = new Thread(new ThreadStart( () =>
                {
                    while(!dataHolder.bailout)
                    {
                        if(dataHolder.newDataReady)
                        {
                            dataHolder.newDataReady = false;
                            String outgoing = "3,";
                            outgoing += (title.Length > 0) ? title + "~" : "0~";
                            outgoing += (lang.Length > 0) ? lang + "~" : "0~";
                            outgoing += (keys.Length > 0) ? keys + "~" : "0~";
                            outgoing += "0";

                            byte[] outBytes = encode.GetBytes(outgoing);
                            stream.Write(outBytes,0,outBytes.Length);

                            bytesFrom = new byte[BUFFER_SIZE];
                            stream.Read(bytesFrom, 0, BUFFER_SIZE);
                            String[] RXVal = encode.GetString(bytesFrom).Split(new Char[] { DELIMITER });

                            items = new List<item>();
                            int i = 0;
                            int len = RXVal.Length - (RXVal.Length % 4);
                            while (true)
                            {
                                if (i >= len)
                                    break;
                                item itm = new item();
                                itm.language = RXVal[i++];
                                itm.title = RXVal[i++];
                                itm.keywords = RXVal[i++];
                                itm.code = RXVal[i++].Replace("\n","\r\n");
                                items.Add(itm);
                            }

                            Func<int> del2 = delegate()
                            {
                                
                                this.dataGridView1.Rows.Clear();
                                foreach(item itm in items)
                                {
                                    int n = dataGridView1.Rows.Add();
                                    dataGridView1.Rows[n].Cells[0].Value = itm.title;
                                    dataGridView1.Rows[n].Cells[1].Value = itm.language;
                                    dataGridView1.Rows[n].Cells[2].Value = itm.keywords;
                                }

                                return 0;
                            };
                            try { Invoke(del2); }
                            catch (Exception) {}
                        }

                        if (dataHolder.addDataReady)
                        {
                            dataHolder.addDataReady = false;
                            String outData = "0," + 
                                addTitle.Text + DELIMITER +
                                addLanguage.Text + DELIMITER +
                                addKeys.Text + DELIMITER +
                                addCode.Text;
                            byte[] outBytes = encode.GetBytes(outData);
                            stream.Write(outBytes, 0, outBytes.Length);
                            dataHolder.newDataReady = true;
                        }
                        Thread.Sleep(200);
                    }
                }));
            updater.IsBackground = true;
            updater.Start();
        }

        private void gridClickEvent(Object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;
            int index = e.RowIndex;

            Func<int> del = delegate()
            {
                queryCode.Text = items[index].code;

                this.queryCode.Focus();
                this.queryCode.SelectAll();
                return 0;
            };
            try {Invoke(del);}
            catch(Exception) {}
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            if (addTitle.Text.Length < 1)
            {
                MessageBox.Show("Enter a title");
                return;
            }
            if (addLanguage.Text.Length < 1)
            {
                MessageBox.Show("Enter a language");
                return;
            }
            if (addKeys.Text.Length < 1)
            {
                MessageBox.Show("Enter a list of keywords separated by commas");
                return;
            }
            if (addCode.Text.Length < 1)
            {
                MessageBox.Show("Enter your code");
                return;
            }
            dataHolder.addDataReady = true;
        }

        private void Form1_Resize(object sender, System.EventArgs e)
        {
            Control control = (Control)sender;
            rescale(control.Size.Width, control.Size.Height);
        }

        private void rescale(int Width, int Height)
        {
            this.dataGridView1.Width = Width / 3 - 15;
            this.dataGridView1.Height = Height - 60;
            this.dataGridView1.Location = new System.Drawing.Point(
                Width * 2 / 3 - 10, 10);

            this.groupBox2.Width = Width / 3 - 20;
            this.groupBox2.Height = Height - 60;
            this.groupBox2.Location = new System.Drawing.Point(
                Width * 1 / 3, 10);

            this.groupBox1.Width = Width / 3 - 20;
            this.groupBox1.Height = Height - 60;
            this.groupBox1.Location = new System.Drawing.Point(10, 10);

            this.addTitle.Width = this.groupBox1.Width * 4 / 5 - 12;
            this.addLanguage.Width = this.groupBox1.Width * 1 / 5 - 5;
            this.addLanguage.Location = new System.Drawing.Point(
                this.groupBox1.Location.X + this.groupBox1.Width * 4 / 5 - 11,
                this.addTitle.Location.Y);
            this.label2.Location = new System.Drawing.Point(this.addLanguage.Location.X, this.label1.Location.Y);
            this.addKeys.Width = this.groupBox1.Width - 12;
            this.addCode.Width = this.groupBox1.Width - 15;
            this.addCode.Height = this.groupBox1.Height - 225;
            this.addButton.Width = this.groupBox1.Width - 12;
            this.addButton.Location = new System.Drawing.Point(5, this.groupBox1.Height - 50);

            this.queryTitle.Width = this.groupBox2.Width * 4 / 5 - 12;
            this.queryLanguage.Width = this.groupBox2.Width * 1 / 5 - 5;
            this.queryLanguage.Location = new System.Drawing.Point(
                this.groupBox2.Width * 4 / 5 - 2,
                this.queryTitle.Location.Y);
            this.label4.Location = new System.Drawing.Point(this.queryLanguage.Location.X, this.label3.Location.Y);
            this.queryKeys.Width = this.groupBox2.Width - 13;
            this.queryCode.Width = this.groupBox2.Width - 15;
            this.queryCode.Height = this.groupBox2.Height - 175;
            
            
        }
    }

    public static class dataHolder
    {
        private static bool _bailout = false;
        private static object _bailoutLocker = new object();
        public static bool bailout
        {
            get { lock (_bailoutLocker) { return _bailout; } }
            set { lock (_bailoutLocker) { _bailout = value; } }
        }

        private static bool _newDataReady = false;
        private static object _newDataReadyLocker = new object();
        public static bool newDataReady
        {
            get { lock (_newDataReadyLocker) { return _newDataReady; } }
            set { lock (_newDataReadyLocker) { _newDataReady = value; } }
        }

        private static bool _addDataReady = false;
        private static object _addDataReadyLocker = new object();
        public static bool addDataReady
        {
            get { lock (_addDataReadyLocker) { return _addDataReady; } }
            set { lock (_addDataReadyLocker) { _addDataReady = value; } }
        }
    }
}
