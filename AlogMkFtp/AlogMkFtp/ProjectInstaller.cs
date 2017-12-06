using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;


namespace AlogMkFtp
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            //系统用于标志此服务名称(唯一性)
            //serviceInstaller1.ServiceName = Utility.Fun.ReadNodeOfXML("ServiceName", "value");
            ////向用户标志服务的显示名称(可以重复)
            //serviceInstaller1.DisplayName = Utility.Fun.ReadNodeOfXML("ServiceDisplayName", "value");
            ////向用户标志服务的说明(描述)
            //serviceInstaller1.Description = Utility.Fun.ReadNodeOfXML("ServiceDescription", "value");
        }
    }
}
