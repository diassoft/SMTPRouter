using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="MailKit.Net.Smtp.SmtpClient"/>
    /// </summary>
    public static class SmtpClientExtension
    {
        /// <summary>
        /// Tries to send a message thru the Smtp Connection
        /// </summary>
        /// <param name="smtpClient">Reference to the Smtp Client calling the function</param>
        /// <param name="message">The message to be sent</param>
        /// <param name="sender">The sender of the message</param>
        /// <param name="recipients">The recipients of the message</param>
        /// <param name="exception">The exception that has been thrown in case it fails</param>
        /// <returns>A <see cref="bool"/> to define whether the message was sent or not</returns>
        public static bool TrySend(this MailKit.Net.Smtp.SmtpClient smtpClient, MimeMessage message, MailboxAddress sender, IEnumerable<MailboxAddress> recipients, out Exception exception)
        {
            try
            {
                smtpClient.Send(message, sender, recipients);
                exception = null;
                return true;
            }
            catch (Exception e)
            {
                exception = e;
                return false;
            }
        }
    }
}
