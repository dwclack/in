using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ASPNetPortal;

namespace Alog_WSKJSD
{
    public partial class frmTest : Form
    {
        public frmTest()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DataTable dt = DBAccess.GetDataTable("SELECT * FROM RepXmlSet", 7);
            dataGridView1.DataSource = dt;
            //dataGridView1.

        }

    }
}
