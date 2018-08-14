using SMTPRouter.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter
{
    /// <summary>
    /// Represents an exception that is thrown when a message was rejected by the <see cref="Router"/>
    /// </summary>
    /// <remarks>The Acceptance or Rejection of a message can be configured on the <see cref="Router.AcceptedIPAddresses"/> and <see cref="Router.RejectedIPAddresses"/> collections</remarks>
    public sealed class UnauthorizedSenderException: RoutableMessageException
    {
        /// <summary>
        /// Initializes a new instance of the Unauthorized Sender Exception
        /// </summary>
        public UnauthorizedSenderException(): this(null) { }

        /// <summary>
        /// Initializes a new instance of the Unauthorized Sender Exception
        /// </summary>
        /// <param name="routableMessage">The Routable Message</param>
        public UnauthorizedSenderException(RoutableMessage routableMessage): this(routableMessage, RejectReasons.Unknown) { }

        /// <summary>
        /// Initializes a new instance of the Unauthorized Sender Exception
        /// </summary>
        /// <param name="routableMessage">The Routable Message</param>
        /// <param name="rejectReason">The reason why the message was rejected. Valid values are at <see cref="RejectReasons"/>.</param>
        public UnauthorizedSenderException(RoutableMessage routableMessage, RejectReasons rejectReason) : this(routableMessage, rejectReason, null) { }

        /// <summary>
        /// Initializes a new instance of the Unauthorized Sender Exception
        /// </summary>
        /// <param name="routableMessage">The Routable Message</param>
        /// <param name="rejectReason">The reason why the message was rejected. Valid values are at <see cref="RejectReasons"/>.</param>
        /// <param name="innerException">The inner exception</param>
        public UnauthorizedSenderException(RoutableMessage routableMessage, RejectReasons rejectReason, Exception innerException) : base($"The IP Address {routableMessage?.IPAddress} is not authorized to send messages thru the SMTP Router. Reason: {Enum.GetName(typeof(RejectReasons), rejectReason)}. You can add the IP Address to the AcceptedIPAddress Collection on the Router or tweak the message by adding the header 'SmtpRouter-Header-ForceRouting'.", routableMessage, innerException) { }
    }

    /// <summary>
    /// Valid Reasons to Reject a Sender
    /// </summary>
    public enum RejectReasons
    {
        /// <summary>
        /// Message was rejected for unknown reasons
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// The IP address of the sender is not on the <see cref="Router.AcceptedIPAddresses"/>.
        /// </summary>
        /// <remarks>If the <see cref="Router.AcceptedIPAddresses"/> is not empty, all valid senders must be on the list</remarks>
        NotInAcceptedAddressesList = 1,
        /// <summary>
        /// The IP address of the sender is on the <see cref="Router.RejectedIPAddresses"/>
        /// </summary>
        /// <remarks>If the <see cref="Router.RejectedIPAddresses"/> is empty, all senders are considered valid</remarks>
        ExistsInRejectedAddressesList = 2
    }
}
