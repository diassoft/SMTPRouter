using SMTPRouter.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter
{
    /// <summary>
    /// Represents an exception during the Smtp Connection
    /// </summary>
    public class SmtpConnectionFailedException: Exception
    {
        /// <summary>
        /// Defines whether the connection has been successfull
        /// </summary>
        public bool HasConnected { get; }

        /// <summary>
        /// Defines whether the connection has been authenticated
        /// </summary>
        public bool HasAuthenticated { get; }

        /// <summary>
        /// Initializes a new instance of the SmtpConnectionFailedException
        /// </summary>
        public SmtpConnectionFailedException(): this(null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the SmtpConnectionFailedException
        /// </summary>
        /// <param name="innerException">The inner exception that caused the exception</param>
        public SmtpConnectionFailedException(Exception innerException): this(new SmtpConfiguration(), false, false, innerException)
        {

        }

        /// <summary>
        /// Initializes a new instance of the SmtpConnectionFailedException
        /// </summary>
        /// <param name="smtpConfiguration">The Smtp Connection that failed</param>
        /// <param name="hasConnected">Flag to define whether the connection has happened or not</param>
        /// <param name="hasAuthenticated">Flag to define whether the connection has been authenticated or not</param>
        /// <param name="innerException">The inner exception that caused the exception</param>
        public SmtpConnectionFailedException(SmtpConfiguration smtpConfiguration, bool hasConnected, bool hasAuthenticated, Exception innerException): base($"Error Connecting to Smtp: Host: '{smtpConfiguration.Host}', HasConnected: {hasConnected}, HasAuthenticated: {hasAuthenticated}", innerException)
        {
            HasConnected = hasConnected;
            HasAuthenticated = hasAuthenticated;
        }

    }
}
