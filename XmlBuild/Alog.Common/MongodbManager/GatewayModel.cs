using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Builders;

namespace Alog.Common.MongodbManager
{
    public abstract partial class GatewayModel
    {

        #region 公共属性，对应数据库字段
        public ObjectId _id { get; set; }
        /// <summary>
        /// 客户主键
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// appkey+客户主键
        /// </summary>
        public string PrimaryKey { get; set; }


        public string MsgId { get; set; }

        /// <summary>
        /// 报文MD5加密
        /// </summary>
        public string Md5Str { get; set; }

        public long Md5Value { get; set; }

        /// <summary>
        /// 数据格式
        /// </summary>
        public string DataType { get; set; }


        /// <summary>
        /// 密文
        /// </summary>
        public string DataDigest { get; set; }

        /// <summary>
        /// 接口
        /// </summary>
        public string MethodType { get; set; }

        /// <summary>
        /// 报文
        /// </summary>
        public string PostData { get; set; }


        public string Guid { get; set; }

        /// <summary>
        /// 公钥
        /// </summary>
        public string AppKey { get; set; }

        public OrderStatus Status { get; set; }

        /// <summary>
        /// 写入时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateTime { get; set; }

        public string ReturnMsg { get; set; }

        /// <summary>
        /// 处理状态：0：未处理；1：处理成功；2：处理失败
        /// </summary>
        public int ReadStatus { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime ReadTime { get; set; }

        /// <summary>
        /// 处理失败原因
        /// </summary>
        public string ReadReturnMessage { get; set; }

        public int ReadTimes { get; set; }

        public string Creater { get; set; } 

        /// <summary>
        /// 如有解析失败，发送给客户的状态
        /// </summary>
        public int SendStatus { get; set; }


        /// <summary>
        /// 如有解析失败，发送给客户的时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime SendTime { get; set; }


        #endregion


        /////////////////////////////////////////////////////////////////////////////////////////


        #region 子类共用属性
        protected static DataImport di = new DataImport();

        protected string AppSecret = "";

        protected const string successText = "<response><success>true</success></response>";

        #endregion





    }

    public enum OrderStatus
    {
        /// <summary>
        /// 写入mongodb
        /// </summary>
        EnterMongodb = 0,

        /// <summary>
        /// 签名验证通过
        /// </summary>
        SignPass = 1,

        /// <summary>
        /// 预验证通过
        /// </summary>
        VerifiedPass = 2,


        /// <summary>
        /// 预验证通过，进入MQ
        /// </summary>
        MQReceived = 3,

        /// <summary>
        /// 特殊接口处理通过
        /// </summary>
        SpecialPass = 4,

        /// <summary>
        /// 写入SQLServer
        /// </summary>
        EnterSqlServer = 20
    }
}
