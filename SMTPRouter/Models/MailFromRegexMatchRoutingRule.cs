﻿using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SMTPRouter.Models
{
    /// <summary>
    /// Represents a <see cref="RoutingRule"/> that will apply a <see cref="Regex">Regular Expression</see> against the Mail From of the message
    /// </summary>
    public sealed class MailFromRegexMatchRoutingRule: RoutingRule
    {
        private string _RegexExpression;

        /// <summary>
        /// The <see cref="Regex">Regular Expression</see> to run
        /// </summary>
        public string RegexExpression
        {
            get { return _RegexExpression; }
            set { SetProperty<string>(ref _RegexExpression, value); }
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="MailFromRegexMatchRoutingRule"/>
        /// </summary>
        public MailFromRegexMatchRoutingRule() : this(0, "", "") { }

        /// <summary>
        /// Initializes a new instance of a <see cref="MailFromRegexMatchRoutingRule"/>
        /// </summary>
        /// <param name="executionSequence">The Priority of the Rule</param>
        /// <param name="regexExpression">A <see cref="Regex">Regular Expression</see> to run</param>
        /// <param name="smtpConfigurationKey">The key of the Smtp Server to use when this rule matches</param>
        public MailFromRegexMatchRoutingRule(int executionSequence, string regexExpression, string smtpConfigurationKey) : base(executionSequence)
        {
            RegexExpression = regexExpression;
            base.SmtpConfigurationKey = smtpConfigurationKey;
        }

        /// <summary>
        /// Checks if the <see cref="Regex">Regular Expression</see> defined on <see cref="RegexExpression"/> is valid. The comparison happens against the mail from.
        /// </summary>
        /// <param name="routableMessage">The Message to Check</param>
        /// <returns>A <see cref="bool"/> to inform whether the rule matches or not</returns>
        public override bool Match(RoutableMessage routableMessage)
        {
            // Get the MailFrom from the Message
            if (routableMessage.MailFrom == null) return false;

            // Get the Domain of the Mail From
            return Regex.Match(routableMessage.MailFrom.Address, RegexExpression).Success;
        }

        /// <summary>
        /// Generates a string with the Routing Rule contents
        /// </summary>
        /// <returns>A <see cref="string"/> contaning the Routing Rule information</returns>
        public override string ToString()
        {
            return string.Format("{0}\n{1}",
                     base.ToString(),
                     base.FormatForToString("Regex Expression", RegexExpression));
        }

    }
}
