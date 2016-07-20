using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.NotificationEngine.Common.Constants;
using TradeHub.NotificationEngine.Common.Utility;
using TradeHub.NotificationEngine.Common.ValueObject;

namespace TradeHub.NotificationEngine.NotificationCenter.Manager
{
    /// <summary>
    /// Handles all email related functionality
    /// </summary>
    internal class EmailManager
    {
        private Type _type = typeof (EmailManager);

        private string _smtpAddress = String.Empty;
        private int _portNumber = 587;
        private bool _enableSsl = true;

        private Dictionary<string, string> _senderInformation;
        private Dictionary<string, string> _receiverInformation; 

        /// <summary>
        /// Default Constructor
        /// </summary>
        public EmailManager()
        {
            // Initialize objects
            _senderInformation = new Dictionary<string, string>();
            _receiverInformation = new Dictionary<string, string>();
        }

        /// <summary>
        /// Sends given notificaiton message to required destination
        /// </summary>
        /// <param name="notification"></param>
        public void SendNotification(OrderNotification notification)
        {
            try
            {
                // Read account information
                ReadSenderAccountInformation();
                ReadReceiverAccountInformation();

                string accountType;
                if (_senderInformation.TryGetValue("username", out accountType))
                {
                    // Get sender account type
                    accountType = (accountType.Split('@')[1]).Split('.')[0];
                    if ((_smtpAddress = GetSmtpAddress(accountType)) != String.Empty)
                    {
                        string subject = CreateSubject(notification.OrderNotificationType);
                        string body = CreateBody(notification);

                        // Send email using the specified credentials
                        SendEmail(subject, body);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendNotification");
            }
        }

        /// <summary>
        /// Sends email message
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        private void SendEmail(string subject, string body)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Sending email notificaiton", _type.FullName, "SendEmail");
                    }

                    string senderAccount;
                    if (!_senderInformation.TryGetValue("username", out senderAccount))
                        return;

                    string senderPassword;
                    if (!_senderInformation.TryGetValue("password", out senderPassword))
                        return;

                    string receiverAccount;
                    if (!_receiverInformation.TryGetValue("username", out receiverAccount))
                        return;

                    mail.From = new MailAddress(senderAccount);
                    mail.To.Add(receiverAccount);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = false;

                    using (SmtpClient smtp = new SmtpClient(_smtpAddress, _portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(senderAccount, senderPassword);
                        smtp.EnableSsl = _enableSsl;
                        smtp.Send(mail);

                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info("Email notificaiton sent", _type.FullName, "SendEmail");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendEmail");
            }
        }

        /// <summary>
        /// Reads infomration required by the email sender address
        /// </summary>
        private void ReadSenderAccountInformation()
        {
            _senderInformation = NotificationConfigurationReader.ReadEmailConfiguration("Sender",
                AppDomain.CurrentDomain.BaseDirectory + @"\Config\EmailSenderInformation.xml");
        }

        /// <summary>
        /// Reads information required by the email receiver address
        /// </summary>
        private void ReadReceiverAccountInformation()
        {
            _receiverInformation = NotificationConfigurationReader.ReadEmailConfiguration("Receiver",
                AppDomain.CurrentDomain.BaseDirectory + @"\Config\EmailReceiverInformation.xml");
        }

        /// <summary>
        /// Gets appropriate SMPT address depending on the type of email account being used
        /// </summary>
        /// <param name="accountType"></param>
        private string GetSmtpAddress(string accountType)
        {
            switch (accountType)
            {
                case "gmail":
                    return "smtp.gmail.com";
                case "yahoo":
                    return "smtp.mail.yahoo.com";
                case "ymail":
                    return "smtp.mail.yahoo.com";
                case "hotmail":
                    return "smtp.live.com";
                case "live":
                    return "smtp.live.com";
                default:
                    return String.Empty;
            }
        }

        /// <summary>
        /// Creates email subject text depending on the notification message
        /// </summary>
        /// <returns></returns>
        private string CreateSubject(OrderNotificationType orderNotificationType)
        {
            string subject = "TradeSharp " + orderNotificationType.ToString() + " Order";

            return subject;
        }

        /// <summary>
        /// Creates email body test depending on the notification message
        /// </summary>
        private string CreateBody(OrderNotification notification)
        {
            string body = String.Empty;

            if (notification.OrderNotificationType.Equals(OrderNotificationType.New) || notification.OrderNotificationType.Equals(OrderNotificationType.Accepted))
            {
                // Get basic order details
                body = GetOrderInformation(notification);
            }
            else if (notification.OrderNotificationType.Equals(OrderNotificationType.Executed))
            {
                // Get basic Order details
                body = GetOrderInformation(notification);

                // Get Fill details
                body += GetFillInformation(notification);
            }
            else if (notification.OrderNotificationType.Equals(OrderNotificationType.Rejected))
            {
                // Get Rejection details
                body = GetRejectionInformation(notification);
            }

            return body;
        }

        /// <summary>
        /// Returns string containg necessary order details
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        private string GetOrderInformation(OrderNotification notification)
        {
            StringBuilder orderDetails = new StringBuilder();

            orderDetails.AppendLine("ORDER DETAILS:");
            orderDetails.AppendLine("Connector: " + notification.Order.OrderExecutionProvider);
            orderDetails.AppendLine("Symbol: " + notification.Order.Security.Symbol);
            orderDetails.AppendLine("Side: " + notification.Order.OrderSide);
            orderDetails.AppendLine("Size: " + notification.Order.OrderSize);

            if (notification.LimitPrice != default(decimal))
                orderDetails.AppendLine("Price: " + notification.LimitPrice);

            orderDetails.AppendLine("Status: " + notification.Order.OrderStatus);
            orderDetails.AppendLine("ID: " + notification.Order.OrderID);
            orderDetails.AppendLine("Broker ID: " + notification.Order.BrokerOrderID);

            return orderDetails.ToString();
        }

        /// <summary>
        /// Returns string containg necessary Fill details
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        private string GetFillInformation(OrderNotification notification)
        {
            StringBuilder fillDetails = new StringBuilder();

            fillDetails.AppendLine("FILL:");
            fillDetails.AppendLine("Status: " + notification.Fill.ExecutionType);
            fillDetails.AppendLine("Size: " + notification.Fill.ExecutionSize);
            fillDetails.AppendLine("Price: " + notification.Fill.ExecutionPrice);
            fillDetails.AppendLine("Execution ID: " + notification.Fill.ExecutionId);

            return fillDetails.ToString();
        }

        /// <summary>
        /// Returns string containg necessary Rejection details
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        private string GetRejectionInformation(OrderNotification notification)
        {
            StringBuilder rejectionDetails = new StringBuilder();

            rejectionDetails.AppendLine("REJECTION:");
            rejectionDetails.AppendLine("Connector: " + notification.Rejection.OrderExecutionProvider);
            rejectionDetails.AppendLine("ID: " + notification.Rejection.OrderId);
            rejectionDetails.AppendLine("Symbol: " + notification.Rejection.Security.Symbol);
            rejectionDetails.AppendLine("Reason: " + notification.Rejection.RejectioReason);

            return rejectionDetails.ToString();
        }
    }
}