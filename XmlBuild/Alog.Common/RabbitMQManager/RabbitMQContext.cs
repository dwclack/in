using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;

namespace Alog.Common.RabbitMQManager
{
    public static class RabbitMQContext
    {
        private static QueueManager qm = new QueueManager();
        public static void PublishMessage(string exchangeName, string queueName, string keyName, string message)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(message))
                {
                    var model = qm.GetModel(exchangeName, queueName, keyName);

                    if (model != null)
                    {
                        using (model.Channel)
                        {
                            model.Channel.BasicPublish(exchangeName, keyName, model.Prop, Encoding.UTF8.GetBytes(message));
                        }
                    }
                        
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public static QueueDeclareOk Declare(string queueName)
        {
            try
            {
                return qm.CheckQueue(queueName);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string GetMessage(string queueName)
        {
            string result = string.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(queueName))
                {
                    var bytes = qm.GetQueueMessage(queueName);
                    if (bytes != null)
                    {
                        result = Encoding.UTF8.GetString(bytes);
                    }
                }
            }
            catch (Exception)
            {                
                throw;
            }

            return result;
        }
    }
}
