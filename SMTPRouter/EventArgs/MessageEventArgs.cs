using MimeKit;
using SMTPRouter.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter
{
    /// <summary>
    /// Event Arguments for the event when a <see cref="RoutableMessage"/> is received
    /// </summary>
    public class MessageEventArgs: EventArgs
    {
        /// <summary>
        /// The message received
        /// </summary>
        public RoutableMessage RoutableMessage { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the Message Received Event Arguments
        /// </summary>
        public MessageEventArgs(): this(null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the Message Received Event Arguments
        /// </summary>
        /// <param name="routableMessage">The <see cref="RoutableMessage"/> received by the Smtp</param>
        public MessageEventArgs(RoutableMessage routableMessage) 
        {
            RoutableMessage = routableMessage;
        }

    }
}
