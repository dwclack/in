using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;

namespace Alog.Common.MongodbManager
{
    /// <summary>
    /// 发送报文基类
    /// 发送的参数分为系统级参数和接口级参数，
    ///     系统级参数为绝大多数接口都共有（不同接口的名称可能不同）
    ///     接口级参数为不同接口各自定义
    ///     定义的系统级参数有：接口名称，报文，伙伴编码，签名
    /// 发送数据时，默认系统级参数允许有的为空，也允许值为空，但是不允许同时为空，如需修改，定义子类并覆盖该方法
    /// </summary>
    public class BaseMessage
    {
        public BaseMessage(string guid)
        {
            Guid = guid;
            CreateTime = DateTime.Now;
            SendStatus = 0;
            SendTimes = 0;
            NextSendTime = DateTime.Now;
        }

        #region 对象属性

        public ObjectId _id { get; set; }

        public string Guid { get; set; }

        public DateTime CreateTime { get; set; }

        public long ApiLogId { get; set; }

        public string CompleteSql { get; set; }
        
        ///////////////////////// 以上属性为对象产生时设置，后续不再修改。
        ///////////////////////// 以下属性为记录发送过程所使用的，其值会被修改且可能多次修改

        public int SendStatus { get; set; }

        public DateTime SendTime { get; set; }

        public int SendTimes { get; set; }

        public string SendReturnMessage { get; set; }

        public DateTime NextSendTime { get; set; }

        public string ErrorMessage { get; set; }

        public int SendIntervalIndex { get; set; }

        #endregion

        #region 系统级参数
        public string Url { get; set; }

        public string RepTitle { get; set; }

        // ------ 系统级参数：接口名称
        /// <summary>
        /// 参数为接口名称的参数名称
        /// </summary>
        public string MethodName { get; set; }
        /// <summary>
        /// 接口名称
        /// </summary>
        public string Method { get; set; }

        // ------ 系统级参数：伙伴编码
        /// <summary>
        /// 参数为伙伴编码的参数名称
        /// </summary>
        public string PartnerCodeName { get; set; }
        /// <summary>
        /// 伙伴编码
        /// </summary>
        public string PartnerCode { get; set; }

        // ------ 系统级参数：签名
        /// <summary>
        /// 参数为签名的参数名称
        /// </summary>
        public string SignName { get; set; }
        /// <summary>
        /// 签名
        /// </summary>
        public string Sign { get; set; }

        // ------ 系统级参数：报文
        /// <summary>
        /// 参数为报文的参数名称
        /// </summary>
        public string ContentName { get; set; }
        /// <summary>
        /// 报文
        /// </summary>
        public string Content { get; set; }
        #endregion

        /// <summary>
        /// 接口级参数，以键值对的方式放在字典里，具体的值由
        /// </summary>
        public Dictionary<string, string> MessageDict { get; set; }

        public virtual string SendMessage()
        {
            ErrorMessage = "";

            if (Url.IsNullOrWhiteSpace())
            {
                return
                    @"<response><success>false</success><errorCode></errorCode><errorMsg>发送地址不能为空</errorMsg></response>";
            }

            int praFlag = 0;
            StringBuilder postBuilder = new StringBuilder();

            if (!MethodName.IsNullOrWhiteSpace())
            {
                praFlag = 1;
                postBuilder.Append(MethodName)
                    .Append("=")
                    .Append(Method);
            }
            if (!SignName.IsNullOrWhiteSpace())
            {
                if (praFlag == 1)
                    postBuilder.Append("&");

                postBuilder.Append(SignName)
                    .Append("=")
                    .Append(Sign);
                praFlag = 1;
            }
            if (!PartnerCodeName.IsNullOrWhiteSpace())
            {
                if (praFlag == 1)
                    postBuilder.Append("&");
                postBuilder.Append(PartnerCodeName)
                    .Append("=")
                    .Append(PartnerCode);
                praFlag = 1;
            }
            if (!ContentName.IsNullOrWhiteSpace())
            {
                if (praFlag == 1)
                    postBuilder.Append("&");
                postBuilder.Append(ContentName)
                    .Append("=")
                    .Append(Content);
                praFlag = 1;
            }

            //默认系统级参数允许有的为空，也允许值为空，但是不允许同时为空，如需修改，定义子类并覆盖该方法
            if (praFlag == 0)
            {
                return
                    @"<response><success>false</success><errorCode></errorCode><errorMsg>参数出错:Post参数为空</errorMsg></response>";
            }

            string postData = postBuilder.ToString();

            StringBuilder urlBuilder = new StringBuilder();

            if (MessageDict != null)
            {
                foreach (KeyValuePair<string, string> pair in MessageDict)
                {
                    urlBuilder.Append(pair.Key)
                        .Append("=")
                        .Append(pair.Value)
                        .Append("&");
                }
            }
            string prmText = urlBuilder.ToString();
            if (prmText.Length > 0)
                prmText = prmText.Substring(0, prmText.Length - 1);

            
            string urlStr = //Url + "?" + prmText;
                (Url.EndsWith("&") ? Url : Url + "?") + prmText;

            string err = "";
            string postResult = CommonUtil.GetPage(urlStr, postData, "UTF-8", out err);
            ErrorMessage = err;

            return postResult;
        }
    }
}
