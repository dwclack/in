using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alog.Common.MongodbManager
{
    public class PostWarning
    {
        public string Guid { get; set; }

        public string TableName { get; set; }

        public long ApiLogId { get; set; }

        public int SendTimes { get; set; }

        public DateTime OrigeTime { get; set; }

        public string Method { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
