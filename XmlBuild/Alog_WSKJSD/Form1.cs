using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using WindowsService1;

namespace Alog_WSKJSD
{
    public partial class Form1 : Form
    {
        static string BWReadType = ClsLog.GetAppSettings("BWReadType");
        static int HZThreadCount = Convert.ToInt32(ClsLog.GetAppSettings("HZThreadCount"));
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //
                int i = 0;
                DataRow[] drs = RepXml.dtRepXmlSet.Select("parentid= 0");
                Thread[] t = new Thread[drs.Length];  //根据报文类型数定义线程数
                foreach (DataRow dr in drs)  //处理每种报文类型
                {
                    try
                    {
                        //ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + dr["RepTitle"].ToString() + "_线程开始", dr["RepTitle"].ToString());

                        if (dr["RepTitle"].ToString() == "商品备案申请")
                        {
                            ClsThreadParam ClsParam = new ClsThreadParam();
                            ClsParam.RepTitle = dr["RepTitle"].ToString();
                            ClsParam.dr = dr;
                            ClsParam.tCount = i;
                            ClsParam.AutoID = Convert.ToInt16(dr["AutoID"].ToString());
                            ClsParam.DSID = Convert.ToInt16(dr["DSID"].ToString());

                            RepXml rx = new RepXml();
                            t[i] = new Thread(new ParameterizedThreadStart(rx.ThreadHandle));
                            t[i].Start(ClsParam);
                        }
                      

                    }
                    catch (Exception ex)
                    {
                        //当数据库服务器连接断开导致异常时，定时器状态需要开启
                        // timer1.Enabled = true;
                        ClsLog.AppendLog("Flag=" + ex.Message, "服务日志");
                        i++;
                        continue;
                    }
                    i++;
                }
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                ClsLog.AppendLog("Flag="  + ex.Message, "服务日志");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DataImport di = new DataImport();
            try
            {
                string HZPath = ClsLog.GetAppSettings("HZPath");
                string HZPathTempBak = ClsLog.GetAppSettings("HZPathTempBak");
                string HZPathBak = ClsLog.GetAppSettings("HZPathBak");
                string[] Files = System.IO.Directory.GetFiles(HZPath);


                foreach (string file in System.IO.Directory.GetFiles(HZPath))
                {
                    try
                    {
                        if (di.ReadFile(file) == 0)
                        {
                            ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                        @"" + HZPathTempBak + @"\");
                            ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                            @"" + HZPathBak + DateTime.Now.ToString("yyyyMM") + @"\");
                            //di.ReadFile(file);
                            ClsLog.DeleteFile(file);

                        }
                    }
                    catch (Exception ex)
                    {
                        ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");

                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
            }
        }
        #region  button3_Click
        private void button3_Click(object sender, EventArgs e)
        {
            string LabelCode="{SZHProduct不用}";
            string str = @"(" + LabelCode + @"(\((\w|,|.)*\)))|(" + LabelCode + ")";//表达
            string ParentTempLabelHtml = @"<?xml version='1.0' encoding='utf-8\'?>
<CBEC xmlns='urn:gacc:datamodel:GZ:CBEC101:1'>
  <Head>
    <MessageID>CBEC101_1p0_PTE51651407170000001_20160602144045_T0001</MessageID>
    <FunctionCode>2</FunctionCode>
    <MessageType>CBEC101</MessageType>
    <SenderID>PTE51651407170000001</SenderID>
    <ReceiverID>5141</ReceiverID>
    <SendTime>2016-06-06T16:21:13</SendTime> 
    <Version>1.0</Version>  
  </Head>
  <Declaration>
    <Declarant>
      <Company>
        <RegistrationNumber>PTE51651407170000001</RegistrationNumber>
      </Company>
    </Declarant>
    <DeclarationPort>5141</DeclarationPort>

    <Commodity>
      <ApplicationNo>TM0087531461741319</ApplicationNo>
      <RegistrationNumber></RegistrationNumber>
      <TypeCode>1</TypeCode>
      <Name>美国百德丝Biodermis精华液15ml</Name>
      <Model>15ml/瓶</Model>
      <UnitCode FirstConversionFactor='0.01500'>142</UnitCode>
      <IEFlag>I</IEFlag>
      <HSCode>3304990010</HSCode>
      <PostalArticlesTaxCode>09039900</PostalArticlesTaxCode>
       <Notes>BBC 1.品名:美国百德丝Biodermis精华液15ml;2.用途:增强皮肤弹力;3.成分:100%硅胶油;4.是否经过药物浸涂或制成零售包装;否;5.品牌:Biodermis;6.型号:清华液15ml;7.包装规格:塑料瓶+纸盒</Notes>
    </Commodity> 

    <Commodity>
      <ApplicationNo>TM0087531439538634</ApplicationNo>
      <RegistrationNumber></RegistrationNumber>
      <TypeCode>1</TypeCode>
      <Name>美国百德丝Biodermis精华液30ml</Name>
      <Model>30ml/瓶</Model>
      <UnitCode FirstConversionFactor='0.03000'>142</UnitCode>
      <IEFlag>I</IEFlag>
      <HSCode>3304990010</HSCode>
      <PostalArticlesTaxCode>09039900</PostalArticlesTaxCode>
       <Notes>BBC 1.品名:美国百德丝Biodermis精华液30ml;2.用途:增强皮肤弹力;3.成分:100%硅胶油;4.是否经过药物浸涂或制成零售包装;否;5.品牌:Biodermis;6.型号:清华液30ml;7.包装规格:塑料瓶+纸盒</Notes>
    </Commodity> 

    <Commodity>
      <ApplicationNo>TM0087531438430945</ApplicationNo>
      <RegistrationNumber></RegistrationNumber>
      <TypeCode>1</TypeCode>
      <Name>美国百德丝Biodermis硅胶疤痕贴3*3cm(6片装)</Name>
      <Model>6片/袋</Model>
      <UnitCode FirstConversionFactor='0.09300'>136</UnitCode>
      <IEFlag>I</IEFlag>
      <HSCode>3005109000</HSCode>
      <PostalArticlesTaxCode>27000000</PostalArticlesTaxCode>
       <Notes>BBC 1.品名:美国百德丝Biodermis硅胶疤痕贴3*3cm(6片装);2.用途:淡化色素;3.成分:硅胶聚合物硅氧烷;4.是否经过药物浸涂或制成零售包装;否;5.品牌:Biodermis;6.型号:EPTTNT-S;7.包装规格:塑料+纸袋装+盒子</Notes>
    </Commodity> 

    <Commodity>
      <ApplicationNo>TM0087531490080885</ApplicationNo>
      <RegistrationNumber></RegistrationNumber>
      <TypeCode>1</TypeCode>
      <Name>美国百德丝Biodermis小片硅胶贴5*6cm（2片装)</Name>
      <Model>2片/袋</Model>
      <UnitCode FirstConversionFactor='0.00800'>136</UnitCode>
      <IEFlag>I</IEFlag>
      <HSCode>3005109000</HSCode>
      <PostalArticlesTaxCode>27000000</PostalArticlesTaxCode>
       <Notes>BBC 1.品名:美国百德丝Biodermis小片硅胶贴5*6cm（2片装);2.用途:淡化色素;3.成分:硅胶聚合物硅氧烷;4.是否经过药物浸涂或制成零售包装;否;5.品牌:Biodermis;6.型号:EDGTNT-123;7.包装规格:塑料+纸袋装+盒子</Notes>
    </Commodity> 

    <Commodity>
      <ApplicationNo>TM0087531384699271</ApplicationNo>
      <RegistrationNumber></RegistrationNumber>
      <TypeCode>1</TypeCode>
      <Name>美国百德丝Biodermis乳房形状疤痕贴32*9.63cm（2片装）</Name>
      <Model>2片/袋</Model>
      <UnitCode FirstConversionFactor='0.19000'>136</UnitCode>
      <IEFlag>I</IEFlag>
      <HSCode>3005109000</HSCode>
      <PostalArticlesTaxCode>27000000</PostalArticlesTaxCode>
       <Notes>BBC 1.品名:美国百德丝Biodermis乳房形状疤痕贴32*9.63cm（2片装）;2.用途:淡化色素;3.成分:硅胶聚合物硅氧烷;4.是否经过药物浸涂或制成零售包装;否;5.品牌:Biodermis;6.型号:EMPTNT-1;7.包装规格:塑料+纸袋装+盒子</Notes>
    </Commodity> 

    <Commodity>
      <ApplicationNo>TM0087531439786144</ApplicationNo>
      <RegistrationNumber></RegistrationNumber>
      <TypeCode>1</TypeCode>
      <Name>美国百德丝Biodermis圆形网眼疤痕贴2.54*2.54cm（2片装）</Name>
      <Model>2片/袋</Model>
      <UnitCode FirstConversionFactor='0.08800'>136</UnitCode>
      <IEFlag>I</IEFlag>
      <HSCode>3005109000</HSCode>
      <PostalArticlesTaxCode>27000000</PostalArticlesTaxCode>
       <Notes>BBC 1.品名:美国百德丝Biodermis圆形网眼疤痕贴2.54*2.54cm（2片装）;2.用途:淡化色素;3.成分:硅胶聚合物硅氧烷;4.是否经过药物浸涂或制成零售包装;否;5.品牌:Biodermis;6.型号:EACTNT-1;7.包装规格:塑料+纸袋装+盒子</Notes>
    </Commodity> 

    <Commodity>
      <ApplicationNo>TM0087531443646535</ApplicationNo>
      <RegistrationNumber></RegistrationNumber>
      <TypeCode>1</TypeCode>
      <Name>美国百德丝Biodermis大尺寸硅胶贴28*40cm</Name>
      <Model>1片/袋</Model>
      <UnitCode FirstConversionFactor='0.26000'>020</UnitCode>
      <IEFlag>I</IEFlag>
      <HSCode>3005109000</HSCode>
      <PostalArticlesTaxCode>27000000</PostalArticlesTaxCode>
       <Notes>BBC 1.品名:美国百德丝Biodermis大尺寸硅胶贴28*40cm;2.用途:淡化色素;3.成分:硅胶聚合物硅氧烷;4.是否经过药物浸涂或制成零售包装;否;5.品牌:Biodermis;6.型号:EDGTNT-1116;7.包装规格:塑料+纸袋装+盒子</Notes>
    </Commodity> 

    <Commodity>
      <ApplicationNo>TM0087531490544738</ApplicationNo>
      <RegistrationNumber></RegistrationNumber>
      <TypeCode>1</TypeCode>
      <Name>美国百德丝Biodermis带状硅胶贴3.5*30cm（2片装）</Name>
      <Model>2片/袋</Model>
      <UnitCode FirstConversionFactor='0.18500'>136</UnitCode>
      <IEFlag>I</IEFlag>
      <HSCode>3005109000</HSCode>
      <PostalArticlesTaxCode>27000000</PostalArticlesTaxCode>
       <Notes>BBC 1.品名:美国百德丝Biodermis带状硅胶贴3.5*30cm（2片装）;2.用途:淡化色素;3.成分:硅胶聚合物硅氧烷;4.是否经过药物浸涂或制成零售包装;否;5.品牌:Biodermis;6.型号:EDGTNT-1112;7.包装规格:塑料+纸袋装+盒子</Notes>
    </Commodity> 

    <Commodity>
      <ApplicationNo>TM0087531490168847</ApplicationNo>
      <RegistrationNumber></RegistrationNumber>
      <TypeCode>1</TypeCode>
      <Name>美国百德丝Biodermis大片硅胶贴12*14.5cm</Name>
      <Model>1片/袋</Model>
      <UnitCode FirstConversionFactor='0.02600'>020</UnitCode>
      <IEFlag>I</IEFlag>
      <HSCode>3005109000</HSCode>
      <PostalArticlesTaxCode>27000000</PostalArticlesTaxCode>
       <Notes>BBC 1.品名:美国百德丝Biodermis大片硅胶贴12*14.5cm;2.用途:淡化色素;3.成分:硅胶聚合物硅氧烷;4.是否经过药物浸涂或制成零售包装;否;5.品牌:Biodermis;6.型号:EDGTNT-146;7.包装规格:塑料+纸袋装+盒子</Notes>
    </Commodity> 

    <Commodity>
      <ApplicationNo>TM0087531383627103</ApplicationNo>
      <RegistrationNumber></RegistrationNumber>
      <TypeCode>1</TypeCode>
      <Name>美国百德丝Biodermis儿童粉色迷彩款硅胶贴7*3CM(3片装）</Name>
      <Model>3片/袋</Model>
      <UnitCode FirstConversionFactor='0.01200'>136</UnitCode>
      <IEFlag>I</IEFlag>
      <HSCode>3005109000</HSCode>
      <PostalArticlesTaxCode>27000000</PostalArticlesTaxCode>
       <Notes>BBC 1.品名:美国百德丝Biodermis儿童粉色迷彩款硅胶贴7*3CM(3片装）;2.用途:淡化色素;3.成分:硅凝胶垫(硅氧烷)，垫子保护层(氨纶、尼龙);4.是否经过药物浸涂或制成零售包装;否;5.品牌:Biodermis;6.型号:CDG-P3;7.包装规格:塑料+纸袋装+盒子</Notes>
    </Commodity> 

    <Commodity>
      <ApplicationNo>TM0087531460569680</ApplicationNo>
      <RegistrationNumber></RegistrationNumber>
      <TypeCode>1</TypeCode>
      <Name>美国百德丝Biodermis儿童绿色迷彩款硅胶贴7*3CM(3片装）</Name>
      <Model>3片/袋</Model>
      <UnitCode FirstConversionFactor='0.01200'>136</UnitCode>
      <IEFlag>I</IEFlag>
      <HSCode>3005109000</HSCode>
      <PostalArticlesTaxCode>27000000</PostalArticlesTaxCode>
       <Notes>BBC 1.品名:美国百德丝Biodermis儿童绿色迷彩款硅胶贴7*3CM(3片装）;2.用途:淡化色素;3.成分:硅凝胶垫(硅氧烷)，垫子保护层(氨纶、尼龙);4.是否经过药物浸涂或制成零售包装;否;5.品牌:Biodermis;6.型号:CDG-G3;7.包装规格:塑料+纸袋装+盒子</Notes>
    </Commodity> 

    <Commodity>
      <ApplicationNo>TM0087531383011489</ApplicationNo>
      <RegistrationNumber></RegistrationNumber>
      <TypeCode>1</TypeCode>
      <Name>美国百德丝Biodermis专业清洗剂28ml/瓶</Name>
      <Model>28ml/瓶</Model>
      <UnitCode FirstConversionFactor='0.02800'>142</UnitCode>
      <IEFlag>I</IEFlag>
      <HSCode>3005909000</HSCode>
      <PostalArticlesTaxCode>27000000</PostalArticlesTaxCode>
       <Notes>BBC 1.品名:美国百德丝Biodermis专业清洗剂28ml/瓶;2.用途:清洁硅胶贴;3.成分:水、2-磺基月桂酸钠、2-磺基月桂酸二钠、十六烷基甜菜碱、月桂基乳酰乳酸酯、月桂酰乳酸钠、甘油二硬脂酸酯、羟基丙基三甲基氯化铵、亚麻籽提取物、泛醇、苯氧乙醇、乙基己基甘油;4.是否经过药物浸涂或制成零售包装;否;5.品牌:Biodermis;6.型号:SLC-49;7.包装规格:塑料瓶</Notes>
    </Commodity> 

    <Commodity>
      <ApplicationNo>TM0087531381575737</ApplicationNo>
      <RegistrationNumber></RegistrationNumber>
      <TypeCode>1</TypeCode>
      <Name>美国百德丝Biodermis修复凝胶10g</Name>
      <Model>10g</Model>
      <UnitCode FirstConversionFactor='0.01000'>012</UnitCode>
      <IEFlag>I</IEFlag>
      <HSCode>3005909000</HSCode>
      <PostalArticlesTaxCode>27000000</PostalArticlesTaxCode>
       <Notes>BBC 1.品名:美国百德丝Biodermis修复凝胶10g;2.用途:淡化色素;3.成分:二甲基甲乙烯硅氧烷 100%;4.是否经过药物浸涂或制成零售包装;否;5.品牌:Biodermis;6.型号:XSO-79;7.包装规格:塑料管+纸盒</Notes>
    </Commodity> 

    <Commodity>
      <ApplicationNo>TM0087531489008394</ApplicationNo>
      <RegistrationNumber></RegistrationNumber>
      <TypeCode>1</TypeCode>
      <Name>美国百德丝Biodermis防晒硅胶膏4.25g</Name>
      <Model>4.25g/管</Model>
      <UnitCode FirstConversionFactor='0.00420'></UnitCode>
      <IEFlag>I</IEFlag>
      <HSCode>3005109000</HSCode>
      <PostalArticlesTaxCode>27000000</PostalArticlesTaxCode>
       <Notes>BBC 1.品名:美国百德丝Biodermis防晒硅胶膏4.25g;2.用途:淡化色素;3.成分:甲氧基肉桂酸辛酯、聚硅氧烷、蜂蜡;4.是否经过药物浸涂或制成零售包装;否;5.品牌:Biodermis;6.型号:SPF-PRO-111;7.包装规格:塑料管+纸盒</Notes>
    </Commodity> 

    <Commodity>
      <ApplicationNo>TM0087532713414881</ApplicationNo>
      <RegistrationNumber></RegistrationNumber>
      <TypeCode>1</TypeCode>
      <Name>美国百德丝Biodermis硅胶膏17g</Name>
      <Model>17g/管</Model>
      <UnitCode FirstConversionFactor='0.01700'></UnitCode>
      <IEFlag>I</IEFlag>
      <HSCode>3005909000</HSCode>
      <PostalArticlesTaxCode>27000000</PostalArticlesTaxCode>
       <Notes>BBC 1.品名:美国百德丝Biodermis硅胶膏17g;2.用途:淡化色素;3.成分:聚硅氧烷80%、蜂蜡20%;4.是否经过药物浸涂或制成零售包装;否;5.品牌:Biodermis;6.型号:PRO17-111;7.包装规格:塑料管+纸盒</Notes>
    </Commodity> 

    <Commodity>
      <ApplicationNo>TM0087531437782627</ApplicationNo>
      <RegistrationNumber></RegistrationNumber>
      <TypeCode>1</TypeCode>
      <Name>美国百德丝Biodermis硅胶膏4.25g</Name>
      <Model>4.25g/管</Model>
      <UnitCode FirstConversionFactor='0.00420'></UnitCode>
      <IEFlag>I</IEFlag>
      <HSCode>3005909000</HSCode>
      <PostalArticlesTaxCode>27000000</PostalArticlesTaxCode>
       <Notes>BBC 1.品名:美国百德丝Biodermis硅胶膏4.25g;2.用途:淡化色素;3.成分:聚硅氧烷80%、蜂蜡20%;4.是否经过药物浸涂或制成零售包装;否;5.品牌:Biodermis;6.型号:PRO-111;7.包装规格:塑料管+纸盒</Notes>
    </Commodity> 

    <Commodity>
      <ApplicationNo>TM0087531489388631</ApplicationNo>
      <RegistrationNumber></RegistrationNumber>
      <TypeCode>1</TypeCode>
      <Name>美国百德丝Biodermis儿童疤痕膏4.25g</Name>
      <Model>4.25g/管</Model>
      <UnitCode FirstConversionFactor='0.00420'></UnitCode>
      <IEFlag>I</IEFlag>
      <HSCode>3005909000</HSCode>
      <PostalArticlesTaxCode>27000000</PostalArticlesTaxCode>
       <Notes>BBC 1.品名:美国百德丝Biodermis儿童疤痕膏4.25g;2.用途:淡化色素;3.成分:聚硅氧烷80%、蜂蜡19%、二氧化钛1%;4.是否经过药物浸涂或制成零售包装;否;5.品牌:Biodermis;6.型号:PROK-111;7.包装规格:塑料管+纸盒</Notes>
    </Commodity> 

    <Commodity>
      <ApplicationNo>TM0087532854606615</ApplicationNo>
      <RegistrationNumber></RegistrationNumber>
      <TypeCode>1</TypeCode>
      <Name>美国百德丝Biodermis遮瑕硅胶贴5片装3.6*15cm</Name>
      <Model>5片/袋</Model>
      <UnitCode FirstConversionFactor='0.03500'>136</UnitCode>
      <IEFlag>I</IEFlag>
      <HSCode>3005109000</HSCode>
      <PostalArticlesTaxCode>27000000</PostalArticlesTaxCode>
       <Notes>BBC 1.品名:美国百德丝Biodermis遮瑕硅胶贴5片装3.6*15cm;2.用途:淡化色素;3.成分:硅胶聚合物硅氧烷;4.是否经过药物浸涂或制成零售包装;否;5.品牌:Biodermis;6.型号:FDG-5106;7.包装规格:塑料+纸袋装+盒子</Notes>
    </Commodity> 

    <Commodity>
      <ApplicationNo>TM0087531460353307</ApplicationNo>
      <RegistrationNumber></RegistrationNumber>
      <TypeCode>1</TypeCode>
      <Name>美国百德丝Biodermis遮瑕硅胶贴1片装3.6*15cm</Name>
      <Model>7g/片</Model>
      <UnitCode FirstConversionFactor='0.00700'>020</UnitCode>
      <IEFlag>I</IEFlag>
      <HSCode>3005109000</HSCode>
      <PostalArticlesTaxCode>27000000</PostalArticlesTaxCode>
       <Notes>BBC 1.品名:美国百德丝Biodermis遮瑕硅胶贴1片装3.6*15cm;2.用途:淡化色素;3.成分:硅胶聚合物硅氧烷;4.是否经过药物浸涂或制成零售包装;否;5.品牌:Biodermis;6.型号:FDG-1106;7.包装规格:塑料+纸袋装+盒子</Notes>
    </Commodity> 

    <Commodity>
      <ApplicationNo>TM0087532854010700</ApplicationNo>
      <RegistrationNumber></RegistrationNumber>
      <TypeCode>1</TypeCode>
      <Name>美国百德丝Biodermis硅胶贴5片装3.6*15cm</Name>
      <Model>5片/袋</Model>
      <UnitCode FirstConversionFactor='0.03500'>136</UnitCode>
      <IEFlag>I</IEFlag>
      <HSCode>3005109000</HSCode>
      <PostalArticlesTaxCode>27000000</PostalArticlesTaxCode>
       <Notes>BBC 1.品名:美国百德丝Biodermis硅胶贴5片装3.6*15cm;2.用途:淡化色素;3.成分:硅胶聚合物硅氧烷;4.是否经过药物浸涂或制成零售包装;否;5.品牌:Biodermis;6.型号:EDGTNT-5106;7.包装规格:塑料+纸袋装+盒子</Notes>
    </Commodity> 

    <Commodity>
      <ApplicationNo>TM0087531486560783</ApplicationNo>
      <RegistrationNumber></RegistrationNumber>
      <TypeCode>1</TypeCode>
      <Name>美国百德丝Biodermis硅胶贴1片装3.6*15cm</Name>
      <Model>7g/片</Model>
      <UnitCode FirstConversionFactor='0.00700'>020</UnitCode>
      <IEFlag>I</IEFlag>
      <HSCode>3005109000</HSCode>
      <PostalArticlesTaxCode>27000000</PostalArticlesTaxCode>
       <Notes>BBC 1.品名:美国百德丝Biodermis硅胶贴1片装3.6*15cm;2.用途:淡化色素;3.成分:硅胶聚合物硅氧烷;4.是否经过药物浸涂或制成零售包装;否;5.品牌:Biodermis;6.型号:EDGTNT-1106;7.包装规格:塑料+纸袋装+盒子</Notes>
    </Commodity> 

  </Declaration>
</CBEC>
";
            MatchCollection macths = Regex.Matches(ParentTempLabelHtml, str, RegexOptions.RightToLeft);
        }
        #endregion

        private void button4_Click(object sender, EventArgs e)
        {
            NLogger.WriteLog("报文生成开始" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "报文生成日志");
            int i = 0;
            DataRow[] drs = RepXml.dtRepXmlSet.Select("parentid= 0");
            Thread[] t = new Thread[drs.Length];  //根据报文类型数定义线程数
            foreach (DataRow dr in drs)  //处理每种报文类型
            {
                try
                {
                    //NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + dr["RepTitle"].ToString() + "_线程开始", dr["RepTitle"].ToString());
                    if (RepXml.AllOK[dr["RepTitle"].ToString()] == false)
                    {
                        NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + dr["RepTitle"].ToString() + "_线程阻塞", dr["RepTitle"].ToString());
                        continue;  //当报文是回执等类型或上次线程未结束，则跳至下个循环
                    }
                    if (!string.IsNullOrEmpty(BWReadType))
                    {
                        int isBw = 1;
                        for (int k = 0; k < BWReadType.Split(',').Length; k++)
                        {
                            if (BWReadType.Split(',')[k].Trim() == dr["RepTitle"].ToString().Trim())
                            {
                                isBw = 0;
                            }
                        }
                        ///如果不是指定的报文类型就不执行
                        if (isBw == 1)
                        {
                            continue;
                        }
                    }

                    ClsThreadParam ClsParam = new ClsThreadParam();
                    ClsParam.RepTitle = dr["RepTitle"].ToString();
                    ClsParam.dr = dr;
                    ClsParam.tCount = i;
                    ClsParam.AutoID = Convert.ToInt16(dr["AutoID"].ToString());
                    ClsParam.DSID = Convert.ToInt16(dr["DSID"].ToString());

                    RepXml rx = new RepXml();
                    t[i] = new Thread(new ParameterizedThreadStart(rx.ThreadHandle));
                    t[i].Start(ClsParam);

                }
                catch (Exception ex)
                {
                    //当数据库服务器连接断开导致异常时，定时器状态需要开启
                    i++;
                    continue;
                }
                i++;
            }
            NLogger.WriteLog("报文生成结束" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "报文生成日志");
        }
    }
}
