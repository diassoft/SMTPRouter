using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter.Models
{
    /// <summary>
    /// A <see cref="RoutingRule"/> that always return true when matching. This is useful when implementing a SMTP Relay because there is no special rule needed, all messages should be accepted then.
    /// </summary>
    /// <remarks>Use the property <see cref="ReplaceMailFromAddress"/> if you need to replace the Mail From on the <see cref="MimeMessage"/>. If you are relaying emails, you may need to switch the mail from to one that is accepted by the SMTP.</remarks>
    public class RelayRoutingRule: RoutingRule
    {
        /// <summary>
        /// The email account to use on the Mail From.
        /// </summary>
        /// <remarks>If you leave it blank, the system will not replace the Mail From on the <see cref="MimeMessage"/></remarks>
        public string ReplaceMailFromAddress { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayRoutingRule"/>
        /// </summary>
        public RelayRoutingRule(): this(0, null) { }

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
            if (!String.IsNullOrEmpty(ReplaceMailFromAddress))
            {
                mimeMessage.From.Clear();
                mimeMessage.From.Add(new MailboxAddress(ReplaceMailFromAddress));
            }

            return true;
        }

        /// <summary>
        /// Generates a string with the Routing Rule contents
        /// </summary>
        /// <returns>A <see cref="string"/> contaning the Routing Rule information</returns>
        public override string ToString()
        {
            return string.Format("{0}\n{1}",
                     base.ToString(),
                     base.FormatForToString("Replace Mail From", ReplaceMailFromAddress));
        }
    }
}
