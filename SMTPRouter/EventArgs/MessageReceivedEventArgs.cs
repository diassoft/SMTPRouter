using MimeKit;
using SMTPRouter.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter
{
    /// <summary>
    /// Event Arguments for the event when a <see cref="MimeMessage"/> is received
    /// </summary>
    public class MessageReceivedEventArgs: EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the Message Received Event Arguments
        /// </summary>
        public MessageReceivedEventArgs(): this(null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the Message Received Event Arguments
        /// </summary>
        /// <param name="mimeMessage">The <see cref="MimeMessage"/> received by the Smtp</param>
        public MessageReceivedEventArgs(MimeMessage mimeMessage)
        {
            MimeMessage = mimeMessage;
        }

        /// <summary>
        /// The message received
        /// </summary>
        public MimeMessage MimeMessage { get; set; }



    }
}
