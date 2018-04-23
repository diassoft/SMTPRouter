using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter
{
    /// <summary>
    /// A base class for Exceptions that contain a MimeMessage
    /// </summary>
    public abstract class MimeMessageException: Exception
    {
        /// <summary>
        /// The <see cref="MimeKit.MimeMessage"/> being processed when the exception occurred
        /// </summary>
        public MimeMessage MimeMessage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MimeMessageException"/>
        /// </summary>
        public MimeMessageException() : this("Mime Message Exception")
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MimeMessageException"/>
        /// </summary>
        /// <param name="message">The Exception Message</param>
        public MimeMessageException(string message) : this(message, null)
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MimeMessageException"/>
        /// </summary>
        /// <param name="message">The Exception Message</param>
        /// <param name="mimeMessage">The <see cref="MimeKit.MimeMessage"/> being processed when the exception occurred</param>
        public MimeMessageException(string message, MimeMessage mimeMessage) : this(message, mimeMessage, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNotSentException"/>
        /// </summary>
        /// <param name="message">The Exception Message</param>
        /// <param name="mimeMessage">The <see cref="MimeKit.MimeMessage"/> being processed when the exception occurred</param>
        /// <param name="innerException">The inner exception that caused this exception to be thrown</param>
        public MimeMessageException(string message, MimeMessage mimeMessage, Exception innerException) : base(message, innerException)
        {
            MimeMessage = mimeMessage;
        }
    }
}
