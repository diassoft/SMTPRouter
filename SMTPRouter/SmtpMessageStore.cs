using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Storage;
using System;
using System.Collections.Generic;
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
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

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
            try
            {
                // Gets the Message Contents and parse it to a MimeMessage
                var textMessage = (ITextMessage)transaction.Message;
                var mimeMessage = MimeKit.MimeMessage.Load(textMessage.Content);

                // Trigger Event to inform a message was received
                MessageReceived?.Invoke(this, new MessageReceivedEventArgs(mimeMessage));
            }
            catch
            {
                // Something failed
                return Task.FromResult(SmtpServer.Protocol.SmtpResponse.TransactionFailed);
            }

            // Transaction was processed successfully
            return Task.FromResult(SmtpServer.Protocol.SmtpResponse.Ok);
        }

    }
}
