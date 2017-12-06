using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;

namespace Alog.Common.MongodbManager
{
    public class RepostLog
    {
        public ObjectId _id { get; set; }

        public DateTime DateTime { get; set; }

        public string OldGuid { get; set; }

        public string NewGuid { get; set; }

        public string MethodType { get; set; }

        public string OldReturnMsg { get; set; }
    }
}
