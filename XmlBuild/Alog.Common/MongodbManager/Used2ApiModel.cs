using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;

namespace Alog.Common.MongodbManager
{
    public class Used2ApiModel
    {
        public ObjectId _id { get; set; }

        public int UsedId { get; set; }

        public string PostGuid { get; set; }

        public string LogicGuid { get; set; }

        public int ReadStatus { get; set; }

        public DateTime ReadTime { get; set; }
    }
}
