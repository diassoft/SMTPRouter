﻿using MimeKit;
using SMTPRouter.Models;
using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SMTPRouter
{
    /// <summary>
    /// The Message Store that handles the incoming SMTP messages
    /// </summary>
    internal class SmtpMessageStore: MessageStore
    {
        /// <summary>
        /// Event triggered when a message is received
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageReceived;

        /// <summary>
        /// Event triggered when a message is received but with errors
        /// </summary>
        public event EventHandler<MessageErrorEventArgs> MessageReceivedWithErrors;

        /// <summary>
        /// Initializes a new instance of the SmtpMessageStore
        /// </summary>
        public SmtpMessageStore()
        {

        }

        /// <summary>
        /// Saves the Message received by the SMTP
        /// </summary>
        /// <param name="context">The context information</param>
        /// <param name="transaction">The transaction</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns></returns>
        public override Task<SmtpServer.Protocol.SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, CancellationToken cancellationToken)
        {
            // The Routable Message
            RoutableMessage routableMessage = new RoutableMessage();
            routableMessage.CreationDateTime = DateTime.Now;

            try
            {

                // Retrieve the Mail From
                routableMessage.MailFrom = new MailboxAddress(string.Format("{0}@{1}",
                                                              transaction.From.User,
                                                              transaction.From.Host));

                // Retrieve the Mail To
                foreach (var mailTo in transaction.To)
                    routableMessage.Recipients.Add(new MailboxAddress($"{mailTo.User}@{mailTo.Host}"));

                // Gets the Message Contents and parse it to a MimeMessage
                var textMessage = (ITextMessage)transaction.Message;
                routableMessage.Message = MimeKit.MimeMessage.Load(textMessage.Content);

                // Retrieve information for the ReceivedBy header
                var sourceIpAddress = ((IPEndPoint)context.RemoteEndPoint).Address.ToString();
                var machineIpAddress = GetLocalIP();
                var machineHostName = System.Net.Dns.GetHostName();

                var receiveByString = string.Format("from [{0}] ({0}) by {1} ({2}) with Smtp Router Service; {3}",
                                                    sourceIpAddress,
                                                    machineHostName,
                                                    machineIpAddress,
                                                    DateTime.Now.ToString("ddd, dd MMM yyy HH:mm:ss %K"));

                // Append Received-By to the MimeMessage Header
                routableMessage.Message.Headers.Add("X-SM-Received", receiveByString);

                // Set the IP Address
                routableMessage.IPAddress = sourceIpAddress;

                // Trigger Event to inform a message was received
                MessageReceived?.Invoke(this, new MessageEventArgs(routableMessage));
            }
            catch (Exception e)
            {
                // Notify listener
                MessageReceivedWithErrors?.Invoke(this, new MessageErrorEventArgs(routableMessage, e));
                
                // Something failed
                return Task.FromResult(SmtpServer.Protocol.SmtpResponse.TransactionFailed);
            }

            // Transaction was processed successfully
            return Task.FromResult(SmtpServer.Protocol.SmtpResponse.Ok);
        }

        /// <summary>
        /// Retrieves current Local IP
        /// </summary>
        /// <returns>A <see cref="String"/> containing the current IP Address</returns>
        private string GetLocalIP()
        {
            // The easiest way to get an accurate local IP address is using this logic
            // When running in virtual machines, it's likely it will not retrieve the proper information, that is the reason why this method was used
            // Refer to Stackoverflow https://stackoverflow.com/questions/6803073/get-local-ip-address to have a better understanding
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP))
            {
                socket.Connect("8.8.8.8", 65530);
                var endPoint = socket.LocalEndPoint as IPEndPoint;
                
                return endPoint.Address.ToString();
            }
        }
    }
}
