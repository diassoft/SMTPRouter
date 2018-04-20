using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter.Models
{
    /// <summary>
    /// Represent the base class for Routing Rules
    /// </summary>
    public abstract class RoutingRule: Diassoft.Mvvm.ObservableObjectBase
    {
        private int _executionSequence;
        /// <summary>
        /// The Priority of the Rule. The lower the number is, the higher the priority is.
        /// </summary>
        public int ExecutionSequence
        {
            get { return _executionSequence; }
            set { SetProperty<int>(ref _executionSequence, value); }
        }

        private string m_SmtpConfigurationKey;
        /// <summary>
        /// The Smtp Configuration Key
        /// </summary>
        public string SmtpConfigurationKey
        {
            get { return m_SmtpConfigurationKey; }
            set { SetProperty<string>(ref m_SmtpConfigurationKey, value); }
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="RoutingRule"/>
        /// </summary>
        public RoutingRule() : this(0)
        {

        }

        /// <summary>
        /// Initializes a new instance of a <see cref="RoutingRule"/>
        /// </summary>
        /// <param name="executionSequence">The Priority of the Rule</param>
        public RoutingRule(int executionSequence)
        {
            ExecutionSequence = executionSequence;
        }

        /// <summary>
        /// Validates the Rule
        /// </summary>
        /// <param name="mimeMessage">Reference to the <see cref="RoutingMessage"/></param>
        /// <returns>A <see cref="bool"/> to define whether the rule matches</returns>
        public abstract bool Match(RoutingMessage mimeMessage);
    }
}
