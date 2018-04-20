using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter
{
    /// <summary>
    /// Event Arguments for the event when a general error has happened
    /// </summary>
    public class GeneralErrorEventArgs: EventArgs
    {
        /// <summary>
        /// The exception that was thrown
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Name of the Method where the error happened
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// Initializes a new GeneralErrorEventArgs
        /// </summary>
        public GeneralErrorEventArgs(Exception exception, string methodName)
        {
            Exception = exception;
            MethodName = methodName;
        }

    }
}
