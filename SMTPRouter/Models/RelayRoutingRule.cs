using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter.Models
{
    /// <summary>
    /// A <see cref="RoutingRule"/> that always return true when matching. This is useful when implementing a SMTP Relay because there is no special rule needed, all messages should be accepted then.
    /// </summary>
    public class RelayRoutingRule: RoutingRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelayRoutingRule"/>
        /// </summary>
        /// <param name="executionSequence">The Priority of the Rule</param>
        /// <param name="smtpConfigurationKey">The key of the Smtp Server to use when this rule matches</param>
        public RelayRoutingRule(int executionSequence, string smtpConfigurationKey) : base(executionSequence)
        {
            base.SmtpConfigurationKey = smtpConfigurationKey;
        }

        /// <summary>
        /// Validates the rule
        /// </summary>
        /// <param name="mimeMessage">The Message to be accepted</param>
        /// <returns>This rule will always return true. Use it when implementing a SMTP Relay to accept all messages and just route them another SMTP.</returns>
        public override bool Match(MimeMessage mimeMessage)
        {
            return true;
        }

    }
}
