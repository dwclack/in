using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Xml;
using System.Text.RegularExpressions;

namespace KJRStartServices
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
                string ExeList=ClsLog.GetAppSettings("ExeList");
                for (int i = 0; i < ExeList.Split(',').Length; i++)
                {
                    string serverName = ExeList.Split(',')[i].Trim();
                    ServiceController service = new ServiceController(serverName);

                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped);
                    }
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running);
                }
                MessageBox.Show("OK");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DateTime anomalyTime = DateTime.Now.AddDays(-1);
            int currentDay = anomalyTime.Day;
            DateTime startCalcDay;
            DateTime endCalcDay = anomalyTime.AddDays(-1);
            if (currentDay > 4)
            {
                startCalcDay = anomalyTime.AddDays(1 - anomalyTime.Day);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string xml = @"<request>
    <logisticsOrderCode>LP00068704652300</logisticsOrderCode>
    <sender>
        <imID>3423443</imID>
        <name>Jim</name>
        <phone>001-72626172</phone>
        <mobile>86-62761162</mobile>
        <email>abc@abc.com</email>
        <zipCode>766276</zipCode>
        <address>
            <country>美国</country>
            <province>俄勒冈州</province>
            <city>波特兰</city>
            <district></district>
            <street></street>
            <detailAddress>303 Perine Street</detailAddress>
        </address>
        <companyID>
            <type>tmall</type>
            <id>627261</id>
        </companyID>
        <companyName>CLIO</companyName>
        <storeUrl>https://koso.tmall.hk/shop/view_shop.htm?spm=a220o.1000855.1997427721.d4918089.lskRV5</storeUrl>
        <storeID>726272</storeID>
        <storeType>Direct</storeType>
        <identity>
            <type>营业执照</type>
            <id>24324327492</id>
        </identity>
    </sender>
    <buyer>
        <imID>726261</imID>
        <name>Jim</name>
        <phone>001-72626172</phone>
        <mobile>86-62761162</mobile>
        <email>abc@abc.com</email>
        <zipCode>000998</zipCode>
        <address>
            <country>中国</country>
            <province>浙江省</province>
            <city>杭州市</city>
            <district>上城区</district>
            <street></street>
            <detailAddress>海潮路100号</detailAddress>
        </address>
        <identity>
            <type>身份证</type>
            <id>510723199007195415</id>
        </identity>
    </buyer>
    <receiver>
        <imID>121</imID>
        <name>Jim</name>
        <phone>001-72626172</phone>
        <mobile>86-62761162</mobile>
        <email>abc@abc.com</email>
        <cpreceiverCode>Tran_Store_13164999</cpreceiverCode>
        <zipCode>888999</zipCode>
        <address>
            <country>美国</country>
            <province>俄勒冈</province>
            <city>波特兰</city>
            <district></district>
            <street></street>
            <detailAddress>500 Perine Street</detailAddress>
        </address>
        <identity>
            <type>营业执照</type>
            <id>23474693242</id>
        </identity>
    </receiver>
    <pickup>
        <name>Jim</name>
        <phone>6617172</phone>
        <mobile>62733373</mobile>
        <email>abc@abc.com</email>
        <zipCode>862132</zipCode>
        <address>
            <country>美国</country>
            <province>Oregan</province>
            <city>Portland</city>
            <district></district>
            <street></street>
            <detailAddress>pine street 76</detailAddress>
        </address>
    </pickup>
    <parcel>
        <parcelInspection>Y</parcelInspection>
        <weight>500</weight>
        <weightUnit>g</weightUnit>
        <price>252.5</price>
        <priceUnit>CNY</priceUnit>
        <length>60</length>
        <width>70</width>
        <height>80</height>
        <dimensionUnit>mm</dimensionUnit>
        <bigBagID>ZY8726384</bigBagID>
        <parcelQuantity>45</parcelQuantity>
        <bigBagWeight>5000</bigBagWeight>
        <bigBagWeightUnit>g</bigBagWeightUnit>
        <goodsList>
                <goods>
                    <productID>3132131</productID>
                    <skuID>3242432</skuID>
                    <name>泰迪熊</name>
                    <cnName>泰迪熊</cnName>
                    <localName>Tedd Bear</localName>
                    <categoryID>76</categoryID>
                    <categoryName>玩具</categoryName>
                    <categoryCNName>玩具</categoryCNName>
                    <categoryFeature></categoryFeature>
                    <price>200</price>
                    <itemPrice>40</itemPrice>
                    <priceUnit>CNY</priceUnit>
                    <priceCurrency>CNY</priceCurrency>
                    <declarePrice>200</declarePrice>
                    <quantity>5</quantity>
                    <extension>json格式</extension>
                    <url>http://*******</url>
                    <hsCode>62526677</hsCode>
                    <tax>20</tax>
                    <mailTaxNumber>678766</mailTaxNumber>
                    <productCategory>266526</productCategory>
                </goods>
        </goodsList>
    </parcel>
    <customs>
        <customsCode>4567</customsCode>
        <customsName>杭州海关</customsName>
        <customsContact>67897665</customsContact>
        <taxTotal>32</taxTotal>
        <declarePriceTotal>252.5</declarePriceTotal>
    </customs>
    <returnParcel>
        <imID>321312</imID>
        <name>Jim</name>
        <phone>213221312</phone>
        <mobile>312123131</mobile>
        <email>abc@abc.com</email>
        <undeliverableOption>N</undeliverableOption>
        <zipCode>21321321</zipCode>
        <address>
            <country>美国</country>
            <province>Oregan</province>
            <city>Portland</city>
            <district></district>
            <street></street>
            <detailAddress>New Street 123</detailAddress>
        </address>
    </returnParcel>
    <trade>
        <tradeID>2508103175893999</tradeID>
        <price>100.55</price>
        <priceUnit>CNY</priceUnit>
        <priceCurrency>CNY</priceCurrency>
        <purchaseTime>2017-01-23 18:23:56</purchaseTime>
    </trade>
    <shippingPaymentInfo>
        <paymentTime>2017-03-14 12:00:00</paymentTime>
        <actualPaymentPrice>2696</actualPaymentPrice>
        <price>2323</price>
        <priceUnit>CENT</priceUnit>
        <priceCurrency>USD</priceCurrency>
    </shippingPaymentInfo>
    <outboundTime>2017-03-14 12:00:00</outboundTime>
    <segmentCode>Trans_Code_234</segmentCode>
    <carrierCode>UPS</carrierCode>
    <isMorePackage>N</isMorePackage>
    <waybillUrl>http://******.pdf</waybillUrl>
    <trackingNumber>67234832</trackingNumber>
    <firstMileTrackingNumber>RU00212323</firstMileTrackingNumber>
    <remark></remark>
    <pickupTime>2017-03-10 11:20:00</pickupTime>
    <timeZone>8</timeZone>
    <preCPResCode></preCPResCode>
    <currentCPResCode>Tran_Store_12093300</currentCPResCode>
    <nextCPResCode>TRUNK_13168300</nextCPResCode>
    <laneCode>L_AE_DHLE</laneCode>
    <orderSource>AE</orderSource>
    <bizType>B2C</bizType>
    <parcelPickupType>123</parcelPickupType>
    <deliverType>123</deliverType>
    <cloudPrintData></cloudPrintData>
    <logisticsOrderCreateTime>2017-03-10 11:20:00</logisticsOrderCreateTime>
</request>";

            string sss = ItemListConvertHWC(xml, "CAINIAO_GLOBAL_SORTINGCENTER_ORDERINFO_NOTIFY");
        }

        /// <summary>
        /// 海外仓读取转换
        /// </summary>
        /// <param name="xmlStr"></param>
        /// <returns></returns>
        private string ItemListConvertHWC(string xmlStr, string msg_type)
        {
            try
            {
                if (!string.IsNullOrEmpty(xmlStr))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xmlStr);
                    if (msg_type == "CAINIAO_GLOBAL_LINEHAUL_ORDERINFO_NOTIFY")
                    {
                        XmlNode node = xmlDoc.SelectSingleNode("/request/sender");
                        if (node != null)
                        {
                            node.InnerXml = Regex.Replace(node.InnerXml, @"(?s)(?<=</?)address(?=>)", "senderaddress1");
                        }
                    }
                    else if (msg_type == "CAINIAO_GLOBAL_SORTINGCENTER_ORDERINFO_NOTIFY")
                    {
                        XmlNode node = xmlDoc.SelectSingleNode("/request/sender");
                        if (node != null)
                        {
                            node.InnerXml = Regex.Replace(node.InnerXml, @"(?s)(?<=</?)address(?=>)", "senderaddress1");
                        }
                        XmlNode node1 = xmlDoc.SelectSingleNode("/request/buyer");
                        if (node1 != null)
                        {
                            node1.InnerXml = Regex.Replace(node1.InnerXml, @"(?s)(?<=</?)address(?=>)", "buyeraddress1");
                        }
                        XmlNode node2 = xmlDoc.SelectSingleNode("/request/receiver");
                        if (node2 != null)
                        {
                            node2.InnerXml = Regex.Replace(node2.InnerXml, @"(?s)(?<=</?)address(?=>)", "receiveraddress1");
                        }
                        XmlNode node3 = xmlDoc.SelectSingleNode("/request/pickup");
                        if (node3 != null)
                        {
                            node3.InnerXml = Regex.Replace(node3.InnerXml, @"(?s)(?<=</?)address(?=>)", "pickupaddress1");
                        }
                        XmlNode node4 = xmlDoc.SelectSingleNode("/request/parcel/returnParcel");
                        if (node4 != null)
                        {
                            node4.InnerXml = Regex.Replace(node4.InnerXml, @"(?s)(?<=</?)address(?=>)", "returnParceladdress1");
                        }
                        XmlNode node5 = xmlDoc.SelectSingleNode("/request/sender");
                        if (node5 != null)
                        {
                            node5.InnerXml = Regex.Replace(node5.InnerXml, @"(?s)(?<=</?)identity(?=>)", "senderidentity1");
                        }
                        XmlNode node6 = xmlDoc.SelectSingleNode("/request/buyer");
                        if (node6 != null)
                        {
                            node6.InnerXml = Regex.Replace(node6.InnerXml, @"(?s)(?<=</?)identity(?=>)", "buyeridentity1");
                        }
                        XmlNode node7 = xmlDoc.SelectSingleNode("/request/receiver");
                        if (node7 != null)
                        {
                            node7.InnerXml = Regex.Replace(node7.InnerXml, @"(?s)(?<=</?)identity(?=>)", "receiveridentity1");
                        }
                    }

                    return xmlDoc.InnerXml.ToString();
                }
                else
                {
                    return xmlStr;
                }
            }
            catch
            {
                return xmlStr;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string tstr = "_SORTINGCENTER_OUTBONDCONFIRM_NOTIFY";
            if (tstr.Contains("CAINIAO_GLOBAL"))
            {
                MessageBox.Show("YES I AM");
            }
            else
            {
                MessageBox.Show("YES I gg"); 
            }
        }
    }
}
