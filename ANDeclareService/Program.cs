using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using Alog.Common;
using System.Security.Cryptography;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using System.CodeDom;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Web.Services.Description;
using ANDeclareService.oaService;
 
namespace ANDeclareService
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            string [] fdfarray={"5454"};
           string fsdf=        InvokeWebService("http://122.224.230.10:6060/manchesterDep/ws/manStockDeleteWs", "addStockDelete", fdfarray).ToString();



           List<C_OA_IF_WSDL_OTComplexTypeShape> list = new List<C_OA_IF_WSDL_OTComplexTypeShape>();

           //实例化实体类并赋值
           C_OA_IF_WSDL_OTComplexTypeShape t = new C_OA_IF_WSDL_OTComplexTypeShape
           {
               C_OA_SEQ = new C_OA_SEQTypeShape { Value = "4654161346464" },//流水号
               EMPLID = new EMPLIDTypeShape { Value = "10012383" },//工号
               EFFDT = new EFFDTTypeShape { Value = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")) },//生效日期
               ACTION = new ACTIONTypeShape { Value = "XFR" },//操作
               ACTION_REASON = new ACTION_REASONTypeShape { Value = "A75" },//操作原因
               POSITION_NBR = new POSITION_NBRTypeShape { Value = "10002556" },//岗位编码
               C_POSN_LVL = new C_POSN_LVLTypeShape { Value = "" }//职级
           };

           list.Add(t);

          CI_C_OA_WSDL_CI_PortTypeClient fdsfsd = new  CI_C_OA_WSDL_CI_PortTypeClient();
    string fsd556f=       fdsfsd.oaUpdate(list.ToArray()).ToString();
 

           string fs44df = InvokeWebService("http://10.10.111.253:8000/PSIGW/PeopleSoftServiceListeningConnector/PSFT_HR/CI_C_OA_WSDL_CI.1.wsdl", "oaUpdate", list.ToArray()).ToString();

            string ijj = "";


            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[] 
            //{ 
            //    new Service1() 
            //};
            //ServiceBase.Run(ServicesToRun);

        }





        #region InvokeWebService


        /// <summary>
        /// 动态调用web服务
        /// </summary>
        /// <param name="url">服务器地址</param>
        /// <param name="methodname">方法名</param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        public static object InvokeWebService(string url, string methodname, object[] args)
        {
            return InvokeWebService(url, null, methodname, args);
        }

        /// < summary>   
        /// 动态调用web服务   
        /// < /summary>   
        /// < param name="url">WSDL服务地址< /param>   
        /// < param name="classname">类名< /param>   
        /// < param name="methodname">方法名< /param>   
        /// < param name="args">参数< /param>
        /// < returns>< /returns>   
        public static object InvokeWebService(string url, string classname, string methodname, object[] args)
        {
            string @namespace = "EnterpriseServerBase.WebService.DynamicWebCalling";
            if ((classname == null) || (classname == ""))
            {
                classname = GetWsClassName(url);
            }

            try
            {
                //获取WSDL   
                WebClient wc = new WebClient();
                Stream stream = wc.OpenRead(url + "?WSDL");

                ServiceDescription sd = ServiceDescription.Read(stream);
                ServiceDescriptionImporter sdi = new ServiceDescriptionImporter();
                sdi.AddServiceDescription(sd, "", "");
                CodeNamespace cn = new CodeNamespace(@namespace);

                //生成客户端代理类代码   
                CodeCompileUnit ccu = new CodeCompileUnit();
                ccu.Namespaces.Add(cn);
                sdi.Import(cn, ccu);
                CSharpCodeProvider icc = new CSharpCodeProvider();

                //设定编译参数   
                CompilerParameters cplist = new CompilerParameters();
                cplist.GenerateExecutable = false;
                cplist.GenerateInMemory = true;
                cplist.ReferencedAssemblies.Add("System.dll");
                cplist.ReferencedAssemblies.Add("System.XML.dll");
                cplist.ReferencedAssemblies.Add("System.Web.Services.dll");
                cplist.ReferencedAssemblies.Add("System.Data.dll");

                //编译代理类   
                CompilerResults cr = icc.CompileAssemblyFromDom(cplist, ccu);
                if (true == cr.Errors.HasErrors)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    foreach (System.CodeDom.Compiler.CompilerError ce in cr.Errors)
                    {
                        sb.Append(ce.ToString());
                        sb.Append(System.Environment.NewLine);
                    }
                    throw new Exception(sb.ToString());
                }

                //生成代理实例，并调用方法   
                System.Reflection.Assembly assembly = cr.CompiledAssembly;
                Type t = assembly.GetType(@namespace + "." + classname, true, true);
                object obj = Activator.CreateInstance(t);
                System.Reflection.MethodInfo mi = t.GetMethod(methodname);
                if (mi == null)
                {
                    return "false";  //不存在该服务器方法
                }
                else
                {
                    return mi.Invoke(obj, args);


                }



                /*   
                PropertyInfo propertyInfo = type.GetProperty(propertyname);   
                return propertyInfo.GetValue(obj, null);   
                */
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException.Message, new Exception(ex.InnerException.StackTrace));
            }
        }

        private static string GetWsClassName(string wsUrl)
        {
            string[] parts = wsUrl.Split('/');
            string[] pps = parts[parts.Length - 1].Split('.');

            return pps[0];
        }
        #endregion

    }

}
