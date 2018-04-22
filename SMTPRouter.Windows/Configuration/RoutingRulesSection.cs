using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Windows.Configuration
{
    /// <summary>
    /// Represents a <see cref="ConfigurationSection"/> where multiple RoutingRules can be setup
    /// </summary>
    /// <remarks>The syntax must be followed precisely otherwise the system will not accept the routing rule</remarks>
    /// <example>
    /// This is how the App.Config must be setup in order to be processed by this class:
    /// <code>
    /// <![CDATA[
    ///  <RoutingRulesConfiguration>
    ///    <RoutingRules>
    ///      <add executionSequence="10"
    ///           type="SMTPRouter.Models.MailFromDomainRoutingRule, SMTPRouter"
    ///           params="Domain=gmail.com"
    ///           smtpkey="gmail" />
    ///      <add executionSequence="20"
    ///           type="SMTPRouter.Models.MailFromDomainRoutingRule, SMTPRouter"
    ///           params="Domain=hotmail.com;"
    ///           smtpkey="hotmail" />
    ///      <add executionSequence="30"
    ///           type="SMTPRouter.Models.MailFromRegexMatchRoutingRule, SMTPRouter"
    ///           params="RegexExpression=\A[Uu](\d{5})\z"
    ///           smtpkey="hotmail" />
    ///    </RoutingRules>
    /// ]]>
    /// </code>
    /// </example>
    public sealed class RoutingRulesSection: ConfigurationSection
    {
        /// <summary>
        /// A collection of routing rules to be set in the configuration file
        /// </summary>
        [ConfigurationProperty(nameof(RoutingRules), IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(RoutingRuleCollection),
                                 AddItemName = "add",
                                 ClearItemsName = "clear",
                                 RemoveItemName = "remove")]
        public RoutingRuleCollection RoutingRules
        {
            get
            {
                RoutingRuleCollection routingRulesCollection = (RoutingRuleCollection)base[nameof(RoutingRules)];

                return routingRulesCollection;
            }

            set
            {
                RoutingRuleCollection routingRulesCollection = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationSection"/>
        /// </summary>
        /// <remarks>The system automatically adds one empty element to the collection</remarks>
        public RoutingRulesSection()
        {
            RoutingRules.Add(new RoutingRuleElement());
        }
    }
}
