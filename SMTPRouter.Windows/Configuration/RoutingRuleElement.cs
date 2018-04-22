using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Windows.Configuration
{
    /// <summary>
    /// Represents <see cref="SMTPRouter.Models.RoutingRule"/> configuration to be used inside App.Config files
    /// </summary>
    /// <remarks>A routing rule must be setup using a precise XML syntax, otherwise the system cannot load the configuration</remarks>
    /// <example>
    /// This is an example of a valid RoutingRuleElement:
    /// <code>
    /// <![CDATA[
    ///   <add executionSequence="10"
    ///        type="SMTPRouter.Models.MailFromDomainRoutingRule, SMTPRouter"
    ///        params="Domain=gmail.com"
    ///        smtpkey="gmail" />
    /// ]]>
    /// </code>
    /// </example>
    public sealed class RoutingRuleElement: ConfigurationElement
    {
        /// <summary>
        /// A unique name to identify the SMTP Configuration
        /// </summary>
        [ConfigurationProperty("executionSequence", IsRequired = true)]
        public int ExecutionSequence
        {
            get { return (int)this["executionSequence"]; }
        }

        /// <summary>
        /// The CLR Type
        /// </summary>
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return (string)this["type"]; }
        }

        /// <summary>
        /// A Description for the SMTP Configuration
        /// </summary>
        [ConfigurationProperty("smtpkey", IsRequired = true)]
        public string SmtpConfigurationKey
        {
            get { return (string)this["smtpkey"]; }
        }

        /// <summary>
        /// Additional Properties that belong to the specific type
        /// </summary>
        [ConfigurationProperty("params", DefaultValue = "")]
        public string Parameters
        {
            get { return (string)this["params"]; }
        }

        /// <summary>
        /// Returns a string with the information regarding the SMTP Connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Sequence.......: {this.ExecutionSequence.ToString()}\n" +
                   $"Type...........: {this.Type}\n" +
                   $"SmtpKey........: {this.SmtpConfigurationKey}\n" +
                   $"Parameters.....: {this.Parameters}";
        }
    }
}
