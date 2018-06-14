using MimeKit;
using SMTPRouter.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter
{
    /// <summary>
    /// A base class for Exceptions that contain a MimeMessage
    /// </summary>
    public abstract class RoutableMessageException: Exception
    {
        /// <summary>
        /// The <see cref="RoutableMessage"/> being processed when the exception occurred
        /// </summary>
        public RoutableMessage RoutableMessage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutableMessageException"/>
        /// </summary>
        public RoutableMessageException() : this("Mime Message Exception")
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutableMessageException"/>
        /// </summary>
        /// <param name="message">The Exception Message</param>
        public RoutableMessageException(string message) : this(message, null)
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RoutableMessageException"/>
        /// </summary>
        /// <param name="message">The Exception Message</param>
        /// <param name="routableMessage">The <see cref="MimeKit.MimeMessage"/> being processed when the exception occurred</param>
        public RoutableMessageException(string message, RoutableMessage routableMessage) : this(message, routableMessage, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNotSentException"/>
        /// </summary>
        /// <param name="message">The Exception Message</param>
        /// <param name="routableMessage">The <see cref="Models.RoutableMessage"/> being processed when the exception occurred</param>
        /// <param name="innerException">The inner exception that caused this exception to be thrown</param>
        public RoutableMessageException(string message, RoutableMessage routableMessage, Exception innerException) : base(message, innerException)
        {
            RoutableMessage = routableMessage;
        }
    }
}
