using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

namespace Alog.Common.MongodbManager
{
    public abstract partial class GatewayModel
    {
        #region 抽象方法

        public abstract string JudgeRequest();

        /// <summary>
        /// 报文判重，并同步到数据
        /// </summary>
        /// <returns></returns>
        public abstract string JudgePostData();

        /// <summary>
        /// 验证签名
        /// </summary>
        /// <returns></returns>
        public abstract string JudgeSign();

        /// <summary>
        /// 预验证报文
        /// </summary>
        /// <returns></returns>
        public abstract string VerifyPostData();

        /// <summary>
        /// 特殊接口处理
        /// </summary>
        /// <returns></returns>
        public abstract string SpecialHandle();

        #endregion

        #region 虚方法
        public virtual bool NeedSpecialHandle()
        {
            return false;
        }

        public virtual bool NeedPublish()
        {
            return true;
        }

        public virtual void UpdateModelStatus(OrderStatus status, string returnMsg)
        {

        }

        public virtual string Invalid()
        {
            return "";
        }

        public virtual string GetSuccessText()
        {
            return successText;
        }

        public virtual void UpdateStatus4QueryForm(int? readTimes, DateTime? readTime, OrderStatus? status, object handler)
        {
            
        }
        #endregion


        #region 共用方法
        /// <summary>
        /// 获取密钥
        /// </summary>
        /// <param name="appKey"></param>
        /// <param name="dataType"></param>
        /// <param name="appSecret"></param>
        /// <returns></returns>
        protected string GetSecretKey(string appKey, string dataType)
        {
            if (!string.IsNullOrWhiteSpace(AppSecret))
            {
                return "";
            }

            try
            {

                //从缓存中获取应用信息
                DataRow[] dr = di.OAuth_APP.Select("AppKey='" + appKey + "'");
                if (dr.Length > 0)
                {

                    string UserName = dr[0]["UserName"].ToString();
                    if (HttpContext.Current != null)
                    {
                        HttpContext.Current.Items["UserName"] = UserName;
                    }
                    AppSecret = dr[0]["AppSecret"].ToString();
                }
                else
                {

                    //   ClsLog.AppendDbLog(AppKey + "，" + Method + "，" + ErrorCode.ERROR_CODE.S0005, "wmsgatewayaspx");
                    //resultStr = ErrorCode.GetResultForWms(dataType, false, ErrorCode.ERROR_CODE.S0005);
                    //return resultStr;
                    return
                        "<response><success>false</success><errorCode></errorCode><errorMsg>无效appKey</errorMsg></response>";

                }
            }
            catch (Exception ex)
            {

                //ClsLog.AppendDbLog("系统错误" + AppKey + "，" + Method + "，" + ex.Message, "wmsgatewayaspx");
                //resultStr = ErrorCode.GetResultForWms(dataType, false, ErrorCode.ERROR_CODE.S9999, ex.Message);
                //return resultStr;
                return
                        "<response><success>false</success><errorCode></errorCode><errorMsg>无效appKey</errorMsg></response>";
            }

            return "";
        }

        #endregion

    }
}
