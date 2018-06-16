using SMTPRouter.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter
{
    /// <summary>
    /// Event Arguments for the event when a smtp connection failed to either connect or authenticate
    /// </summary>
    public class SmtpConnectionEventArgs: EventArgs
    {
        /// <summary>
        /// The exception that was thrown
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// The Smtp Configuration
        /// </summary>
        public SmtpConfiguration SmtpConfiguration { get; }

        /// <summary>
        /// The number of the connection
        /// </summary>
        public int ConnectionNumber { get; }

        /// <summary>
        /// Initializes a new SmtpConnectionEventArgs
        /// </summary>
        public SmtpConnectionEventArgs(SmtpConfiguration smtpConfiguration): this(smtpConfiguration, null) { }

        /// <summary>
        /// Initializes a new SmtpConnectionEventArgs
        /// </summary>
        public SmtpConnectionEventArgs(SmtpConfiguration smtpConfiguration, Exception exception): this(smtpConfiguration, 0, exception) { }

        /// <summary>
        /// Initializes a new SmtpConnectionEventArgs
        /// </summary>
        /// <param name="smtpConfiguration">The Smtp Configuration</param>
        /// <param name="connectionNumber">The number of the connection when using multiple threads</param>
        /// <param name="exception">An internal exception</param>
        public SmtpConnectionEventArgs(SmtpConfiguration smtpConfiguration, int connectionNumber, Exception exception)
        {
            SmtpConfiguration = smtpConfiguration;
            Exception = exception;
            ConnectionNumber = connectionNumber;
        }

    }
}
