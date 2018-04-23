using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter
{
    /// <summary>
    /// Represents an exception that is thrown when a message could not be added to the queue by the <see cref="Router"/>
    /// </summary>
    public sealed class MessageNotQueuedException: MimeMessageException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNotQueuedException"/>
        /// </summary>
        public MessageNotQueuedException() : this(null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNotQueuedException"/>
        /// </summary>
        /// <param name="message">The Message that could not be queued</param>
        public MessageNotQueuedException(MimeMessage message) : this(null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNotQueuedException"/>
        /// </summary>
        /// <param name="mimeMessage">The Message that could not be queued</param>
        /// <param name="innerException">The inner exception that caused the message to not be routed properly</param>
        public MessageNotQueuedException(MimeMessage mimeMessage, Exception innerException) : base("The MimeMessage could not be queued", mimeMessage, innerException) { }
    }
}
