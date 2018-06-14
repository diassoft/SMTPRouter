using MimeKit;
using SMTPRouter.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter
{
    /// <summary>
    /// Event Arguments for the event when a <see cref="MimeMessage"/> has had errors during its processing
    /// </summary>
    public sealed class MessageErrorEventArgs: MessageEventArgs
    {
        /// <summary>
        /// The exception that caused the error
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Initializes a new instance of the Message Error Event Arguments
        /// </summary>
        public MessageErrorEventArgs(): this(null) { }

        /// <summary>
        /// Initializes a new instance of the Message Error Event Arguments
        /// </summary>
        /// <param name="routableMessage">The <see cref="MimeMessage"/> received by the Smtp</param>
        public MessageErrorEventArgs(RoutableMessage routableMessage): this(routableMessage, null) { }

        /// <summary>
        /// Initializes a new instance of the Message Error Event Arguments
        /// </summary>
        /// <param name="routableMessage">The <see cref="MimeMessage"/> received by the Smtp</param>
        /// <param name="exception">The exception that caused the error</param>
        public MessageErrorEventArgs(RoutableMessage routableMessage, Exception exception): base(routableMessage)
        {
            RoutableMessage = routableMessage;
            Exception = exception;
        }


    }
}
