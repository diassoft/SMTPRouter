using Diassoft.Mvvm;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter.Models
{
    /// <summary>
    /// Represents a <see cref="RoutingRule"/> that will verify if the Mail From belongs to a specific domain
    /// </summary>
    public sealed class MailFromDomainRoutingRule: RoutingRule
    {
        private string _Domain;

        /// <summary>
        /// The domain to compare against
        /// </summary>
        public string Domain
        {
            get { return _Domain; }
            set { SetProperty<string>(ref _Domain, value); }
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="MailFromDomainRoutingRule"/>
        /// </summary>
        public MailFromDomainRoutingRule(): this(0, "","") { }

        /// <summary>
        /// Initializes a new instance of a <see cref="MailFromDomainRoutingRule"/>
        /// </summary>
        /// <param name="executionSequence">The Priority of the Rule</param>
        /// <param name="domain">The domain to compare with</param>
        /// <param name="smtpConfigurationKey">The key of the Smtp Server to use when this rule matches</param>
        public MailFromDomainRoutingRule(int executionSequence, string domain, string smtpConfigurationKey): base(executionSequence)
        {
            Domain = domain;
            base.SmtpConfigurationKey = smtpConfigurationKey;
        }

        /// <summary>
        /// Checks if the email sender belongs to the <see cref="Domain"/>
        /// </summary>
        /// <param name="routableMessage">The Message to Check</param>
        /// <returns>A <see cref="bool"/> to inform whether the rule matches or not</returns>
        public override bool Match(RoutableMessage routableMessage)
        {
            // Get the MailFrom from the Message
            if (routableMessage.MailFrom == null) return false;

            // Get the Domain of the Mail From
            var mailFromContents = routableMessage.MailFrom.Address.Split('@');
            if (mailFromContents.Length < 2) return false;

            // Try to match rule
            return (mailFromContents[1].Trim().ToUpper() == Domain.Trim().ToUpper());
        }

        /// <summary>
        /// Generates a string with the Routing Rule contents
        /// </summary>
        /// <returns>A <see cref="string"/> contaning the Routing Rule information</returns>
        public override string ToString()
        {
            return string.Format("{0}\n{1}",
                                 base.ToString(),
                                 base.FormatForToString("Domain", Domain));
        }
    }
}
