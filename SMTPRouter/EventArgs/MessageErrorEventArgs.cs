using MimeKit;
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
        /// <param name="mimeMessage">The <see cref="MimeMessage"/> received by the Smtp</param>
        public MessageErrorEventArgs(MimeMessage mimeMessage): this(mimeMessage, null) { }

        /// <summary>
        /// Initializes a new instance of the Message Error Event Arguments
        /// </summary>
        /// <param name="mimeMessage">The <see cref="MimeMessage"/> received by the Smtp</param>
        /// <param name="exception">The exception that caused the error</param>
        public MessageErrorEventArgs(MimeMessage mimeMessage, Exception exception): base(mimeMessage)
        {
            MimeMessage = mimeMessage;
            Exception = exception;
        }


    }
}
