using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter
{
    /// <summary>
    /// Represents an exception thrown when the <see cref="Router"/> could not create the queues
    /// </summary>
    public sealed class QueuesNotInitializedException: Exception
    {
        private static string DEFAULTEMESSAGE = "Unable to initialize queues";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueuesNotInitializedException"/>
        /// </summary>
        public QueuesNotInitializedException(): this(null)
        {



        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueuesNotInitializedException"/>
        /// </summary>
        /// <param name="innerException">The exception that caused the queue to not be initialized</param>
        public QueuesNotInitializedException(Exception innerException): base(QueuesNotInitializedException.DEFAULTEMESSAGE, innerException)
        {

        }

        
    }
}
