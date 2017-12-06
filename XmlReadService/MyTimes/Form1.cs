using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace MyTimes
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
          
            DateTime EndTime=Convert.ToDateTime(dateTimePicker1.Text);
            if (DateTime.Now.AddMinutes(1) == EndTime)
            {
                MessageBox.Show("系统预计一分钟后关机，请大佬知悉");
            }
            if (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") == EndTime.ToString("yyyy-MM-dd HH:mm:ss"))
            {
                MessageBox.Show("系统预计30s后关机，请大佬知悉");
               // OperateComputer("shutdown.exe -s -t 30");
                timer1.Enabled = false;
            }

            TimeSpan ts = EndTime.Subtract(DateTime.Now);
            textBox1.Text = ts.Hours.ToString() + "小时" + ts.Minutes.ToString() + "分" + ts.Seconds.ToString() + "秒";
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer1.Interval = 1000;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            textBox1.Text = "0";
        }

       /// <summary>
          /// 操作电脑命令
          /// </summary>
         /// <param name="command"></param>
        private void OperateComputer(string command)
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe");
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;
            var myProcess = new System.Diagnostics.Process();
            myProcess.StartInfo = startInfo;
            myProcess.Start();
            myProcess.StandardInput.WriteLine(command);
        }

    }
}
