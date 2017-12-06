using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ASPNetPortal.DB;
using RabbitMQ.Client;

namespace Alog.Common.RabbitMQManager
{
    internal class QueueManager
    {
        private static ConnectionFactory Factory;

        public readonly IConnection SendConnection;

        private readonly IConnection ReceiveConnection;

        public IModel SendChannel { get; set; }

        public QueueManager()
        {
            string server = QueueParameter.ServerName;//  ConfigurationManager.AppSettings["Server"];

            if (!string.IsNullOrWhiteSpace(server))
            {
                if (server.ToLower() == "localhost")
                {
                    Factory = new ConnectionFactory
                    {
                        HostName = "localhost",
                        AutomaticRecoveryEnabled = true,
                        RequestedHeartbeat = QueueParameter.Heartbeat
                    };
                }
                else
                {
                    string userName = QueueParameter.HostUserName;
                    string password = DBAccess.DesDecrypt(QueueParameter.HostPassword);
                    string virtualHost = QueueParameter.VirtualHost;

                    if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password) &&
                        !string.IsNullOrWhiteSpace(virtualHost))
                    {
                        Factory = new ConnectionFactory
                        {
                            HostName = server,
                            UserName = userName,
                            Password = password,
                            Port = 5672,
                            VirtualHost = virtualHost,
                            AutomaticRecoveryEnabled = true,
                            RequestedHeartbeat = QueueParameter.Heartbeat
                        };
                    }
                }

                SendConnection = Factory.CreateConnection();
                ReceiveConnection = Factory.CreateConnection();
                SendChannel = SendConnection.CreateModel();
            }
        }

        public ChannelModel GetModel(string exchangeName, string queueName, string keyName)
        {
            ChannelModel model = null;
            if (SendConnection != null)
            {
                IModel channel = SendConnection.CreateModel();
                channel.ExchangeDeclare(exchangeName, "direct");
                channel.QueueDeclare(queueName, true, false, false, null);
                channel.QueueBind(queueName, exchangeName, keyName);

                IBasicProperties prop = channel.CreateBasicProperties();
                prop.DeliveryMode = 2;

                model = new ChannelModel
                {
                    Channel = channel,
                    keyName = keyName,
                    Prop = prop
                };
            }

            return model;
        }

        public byte[] GetQueueMessage(string queueName)
        {
            if (ReceiveConnection != null && !string.IsNullOrWhiteSpace(queueName))
            {
                using (IModel channel = ReceiveConnection.CreateModel())
                {
                    

                    channel.BasicQos(0, 1, false);

                    //noAck = true，不需要回复，接收到消息后，queue上的消息就会清除
                    //noAck = false，需要回复，接收到消息后，queue上的消息不会被清除，直到调用channel.basicAck(deliveryTag, false); queue上的消息才会被清除 而且，在当前连接断开以前，其它客户端将不能收到此queue上的消息
                    var res = channel.BasicGet(queueName, true);

                    if (res != null)
                    {
                        return res.Body;
                    }
                }
            }

            return null;
        }

        public QueueDeclareOk CheckQueue(string queueName)
        {
            QueueDeclareOk ok = null;
            if (ReceiveConnection != null && !string.IsNullOrWhiteSpace(queueName))
            {
                using (IModel channel = ReceiveConnection.CreateModel())
                {
                    
                    ok = channel.QueueDeclarePassive(queueName);
                    //ok = channel.QueueDeclare(queueName, false,false, false, null);
                    
                }
            }

            return ok;
        }

        public delegate bool QueueHandlDel(string text);
        public bool GetQueueMessage(string queueName, QueueHandlDel handlDelegate)
        {
            if (ReceiveConnection != null && !string.IsNullOrWhiteSpace(queueName) && handlDelegate != null)
            {
                using (IModel channel = ReceiveConnection.CreateModel())
                {

                    channel.BasicQos(0, 1, false);

                    //noAck = true，不需要回复，接收到消息后，queue上的消息就会清除
                    //noAck = false，需要回复，接收到消息后，queue上的消息不会被清除，直到调用channel.basicAck(deliveryTag, false); queue上的消息才会被清除 而且，在当前连接断开以前，其它客户端将不能收到此queue上的消息
                    var res = channel.BasicGet(queueName, false);

                    if (res != null)
                    {
                        byte[] body = res.Body;
                        string data = Encoding.UTF8.GetString(body);

                        if (!handlDelegate(data))
                        {
                            // 处理失败，把消息放回队头
                            channel.BasicReject(res.DeliveryTag, true);
                            return false;
                        }
                        else
                        {
                            // 处理成功，把消息移除
                            channel.BasicAck(res.DeliveryTag, false);
                            return true;
                        }

                    }
                }
            }

            return false;
        }

        public IConnection CreateConnection()
        {

            return Factory.CreateConnection();
        }

        public IModel CreateChannel(IConnection conn)
        {
            if (conn == null)
                return null;
            return conn.CreateModel();
        }
    }

    internal class ChannelModel
    {
        public string keyName { get; set; }

        public IModel Channel { get; set; }

        public IBasicProperties Prop { get; set; }

    }
}
