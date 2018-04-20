using MimeKit;
using SMTPRouter.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter
{
    /// <summary>
    /// Represents an exception that is thrown when a message could not be routed by the <see cref="Router"/>
    /// </summary>
    public sealed class MessageNotSentException: Exception
    {
        /// <summary>
        /// The default description of the error message
        /// </summary>
        private static string DEFAULTROUTINGMESSAGE = "Routing Message was not processed successfully";

        /// <summary>
        /// The Message that could not be routed
        /// </summary>
        public MimeMessage MimeMessage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNotSentException"/>
        /// </summary>
        public MessageNotSentException(): this(null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNotSentException"/>
        /// </summary>
        /// <param name="message">The Message that could not be routed</param>
        public MessageNotSentException(MimeMessage message): this(null, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNotSentException"/>
        /// </summary>
        /// <param name="message">The Message that could not be routed</param>
        /// <param name="innerException">The inner exception that caused the message to not be routed properly</param>
        public MessageNotSentException(MimeMessage message, Exception innerException): base(DEFAULTROUTINGMESSAGE, innerException)
        {
            MimeMessage = message;
        }
    }
}
