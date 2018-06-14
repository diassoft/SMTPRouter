using SMTPRouter.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter
{
    /// <summary>
    /// Represents an exception that is thrown when a message could not be routed to an Smtp Server by the <see cref="Router"/>
    /// </summary>
    public sealed class MessageNotRoutedException : RoutableMessageException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNotQueuedException"/>
        /// </summary>
        public MessageNotRoutedException() : this(null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNotQueuedException"/>
        /// </summary>
        /// <param name="routableMessage">The Message that could not be queued</param>
        public MessageNotRoutedException(RoutableMessage routableMessage) : this(null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNotQueuedException"/>
        /// </summary>
        /// <param name="routableMessage">The Message that could not be routed</param>
        /// <param name="innerException">The inner exception that caused the message to not be routed properly</param>
        public MessageNotRoutedException(RoutableMessage routableMessage, Exception innerException) : base("The Routable Message could not be queued", routableMessage, innerException) { }
    }
}
