using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alog.Common.RabbitMQManager
{
    internal class QueueParameter
    {
        public static string ServerName
        {
            get
            {
                return ClsLog.GetAppSettings("Server");
            }
        }

        public static string HostUserName
        {
            get
            {
                return ClsLog.GetAppSettings("UserName");
            }  
        }

        public static string HostPassword
        {
            get
            {
                return ClsLog.GetAppSettings("Password");
            }  
        }

        public static string VirtualHost
        {
            get
            {
                return ClsLog.GetAppSettings("VirtualHost");
            }
        } 

        public static ushort Heartbeat
        {
            get
            {
                ushort value;
                if (ushort.TryParse(ClsLog.GetAppSettings("Heartbeat"), out value))
                    return value;
                return 60;
            }
        }

        public static int QueueCount
        {
            get
            {
                int value;
                if (int.TryParse(ClsLog.GetAppSettings("QueueCount"), out value))
                    return value;
                return 5;
            }
        }
    }
}
