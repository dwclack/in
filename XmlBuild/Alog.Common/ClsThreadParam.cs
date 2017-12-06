using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;

namespace Alog.Common
{
    public class CybThreadParam
    {
        public DataTable dt = null;
        public int Intdex = 0;
        public int Count = 0;
        public int thread = 0;
        public int tCount = 0;
        public Thread[] t = null;
    }
    public class ClsThreadParam
    {
        public DataRow dr = null;
        public int tCount = 0;
        public int DSID = 0;
        public int AutoID = 0;
        public string RepTitle = "";
        public DataSet ds = null;
        //public string MessageId = string.Empty;
        //public string MessageType = string.Empty;
        //public string SendTime = string.Empty;
        //public string PrimaryKey = string.Empty;
        //public string str="";
    }

}
