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
    public sealed class MessageNotSentException: RoutableMessageException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNotSentException"/>
        /// </summary>
        public MessageNotSentException(): this(null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNotSentException"/>
        /// </summary>
        /// <param name="routableMessage">The Message that could not be routed</param>
        public MessageNotSentException(RoutableMessage routableMessage): this(null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNotSentException"/>
        /// </summary>
        /// <param name="routableMessage">The Message that could not be routed</param>
        /// <param name="innerException">The inner exception that caused the message to not be routed properly</param>
        public MessageNotSentException(RoutableMessage routableMessage, Exception innerException): base("The MimeMessage could not be sent", routableMessage, innerException) { }
    }
}
