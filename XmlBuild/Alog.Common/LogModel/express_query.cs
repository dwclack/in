using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alog.Common.LogModel
{
    /// <summary>
    /// 查询日志
    /// </summary>
    public class express_query
    {
        /// <summary>
        /// 应用标识 = appKey
        /// </summary>
        public string appId { get; set; }
        /// <summary>
        /// appKey
        /// </summary>
        public string appKey { get; set; }
        /// <summary>
        /// 日志类型（标识） = "express_query"
        /// </summary>
        public string method { get; set; }
        /// <summary>
        /// 上传日志的时间
        /// </summary>
        public DateTime time { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime operation_time { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string company { get; set; }
        /// <summary>
        /// 站点id
        /// </summary>
        public string station_id { get; set; }
        /// <summary>
        /// 站点名称
        /// </summary>
        public string station { get; set; }
        /// <summary>
        /// 站点所在省份
        /// </summary>
        public string province { get; set; }
        /// <summary>
        /// 站点所在城市
        /// </summary>
        public string city { get; set; }
        /// <summary>
        /// 操作员账号ID
        /// </summary>
        public string user_id { get; set; }
        /// <summary>
        /// 操作员账号名
        /// </summary>
        public string user { get; set; }
        /// <summary>
        /// 发起请求的终端应用类型
        /// </summary>
        public string source_type { get; set; }
        /// <summary>
        /// 发起请求的应用
        /// </summary>
        public string source_application { get; set; }
        /// <summary>
        /// 查询操作源公网IP
        /// </summary>
        public string source_public_ip { get; set; }
        /// <summary>
        /// 查询操作源内网IP
        /// </summary>
        public string source_private_ip { get; set; }
        /// <summary>
        /// 查询操作源VPN IP
        /// </summary>
        public string source_vpn_ip { get; set; }
        /// <summary>
        /// 源MAC地址
        /// </summary>
        public string source_mac { get; set; }
        /// <summary>
        ///请求的目标应用
        /// </summary>
        public string target_application { get; set; }
        /// <summary>
        /// 查询目标端公网IP
        /// </summary>
        public string target_public_ip { get; set; }
        /// <summary>
        /// 查询目标端内网IP
        /// </summary>
        public string target_private_ip { get; set; }
        /// <summary>
        /// 操作类型（查询\导出）
        /// </summary>
        public string operation_type { get; set; }
        /// <summary>
        /// 操作结果
        /// </summary>
        public string result { get; set; }
        /// <summary>
        /// 结果原因
        /// </summary>
        public string reason { get; set; }
        /// <summary>
        /// 查询\导出的信息内容
        /// 包含信息内容的标识，根据实际查询内容选择下面列出的英文标识进行组合，用英文分隔符进行分割：
        /// 运单号－mail_no ,物流订单号－order_id ,仓库号－wms_id ,收件人手机号－receiver_phone, 收件人姓名－receiver_name
        /// 收件详细地址－receiver_address , 收件人支付宝账号－alipay_no , 收件人邮箱－receiver_email , 收件人身份证号－identification_no
        /// 收件人护照号－passport_no , 收件人身份证照片—identification_photo , 收件人护照照片－passport_photo , 底单照片—mail_photo
        /// 单号绑定省－mail_province , 单号绑定市－mail_city , 如： receiver_phone;receiver_name;
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// 运单号
        /// </summary>
        public string mail_no { get; set; }
        /// <summary>
        /// 物流订单号（LP号）
        /// </summary>
        public string order_id { get; set; }
        /// <summary>
        /// 仓库管理单号（如LBX号）
        /// </summary>
        public string wms_id { get; set; }
        /// <summary>
        /// 本次查询涉及单号数目
        /// </summary>
        public string association_number { get; set; }
        /// <summary>
        /// 如果涉及对外发送客户信息，需要记录email地址
        /// </summary>
        public string email { get; set; }
        /// <summary>
        /// 预留拓展字段
        /// </summary>
        public string feature { get; set; }

    }
}
