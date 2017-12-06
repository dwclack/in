using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;

namespace Alog.Common.MongodbManager
{
    public class MessageShotHistory
    {
        public ObjectId _id { get; set; }

        public string Guid { get; set; }

        public DateTime CreateTime { get; set; }

        public string RecordGuid { get; set; }

        public long ApiLogId { get; set; }

        public DateTime SendTime { get; set; }

        public int SendTimes { get; set; }

        public string LabelCode { get; set; }

        public string RepTitle { get; set; }

        public string Url { get; set; }

        public string PartnerCode { get; set; }

        public string PostData { get; set; }

        public string ReturnMsg { get; set; }
    }
}
