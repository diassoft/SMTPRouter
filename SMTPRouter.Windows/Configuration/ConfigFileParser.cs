using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using SMTPRouter.Models;
using System.Collections.Specialized;
using System.Reflection;

namespace SMTPRouter.Windows.Configuration
{
    /// <summary>
    /// A helper class to read a <see cref="System.Configuration.Configuration">Configuration File</see> and parse its sections into the proper collections
    /// </summary>
    /// <remarks>The configuration file must respect a schema in order to be parsed properly by this class.</remarks>
    /// <seealso cref="System.Configuration.Configuration"/>
    /// <example>
    /// The example below demonstrates a valid App.Config file:
    /// <code>
    /// <![CDATA[
    ///  <configSections>
    ///    <section name="SmtpRouterConfiguration"
    ///              type="SMTPRouter.Windows.Configuration.NameValueSection, SMTPRouter.Windows"/>
    ///    <section name="SmtpConfiguration"
    ///              type="SMTPRouter.Windows.Configuration.SmtpConnectionsSection, SMTPRouter.Windows"/>
    ///    <section name="RoutingRulesConfiguration"
    ///              type="SMTPRouter.Windows.Configuration.RoutingRulesSection, SMTPRouter.Windows"/>
    ///  </configSections>
    ///
    ///  <!-- Configuration for the Current SMTP Server -->
    ///  <SmtpRouterConfiguration>
    ///    <add name="Host" value="localhost"/>
    ///    <add name="Port" value="25"/>
    ///    <add name="MessageLifespanMinutes" value="15"/>
    ///    <add name="MessagePurgeLifespanDays" value="90"/>
    ///  </SmtpRouterConfiguration>
    ///
    ///  <!-- Destination SMTPs -->
    ///  <SmtpConfiguration>
    ///    <SmtpConnections>
    ///      <add key="gmail"
    ///            description="Gmail SMTP Server"
    ///            host="smtp.gmail.com"
    ///            port="587"
    ///            requiresAuthentication="true"
    ///            user="user"
    ///            password="pwd"/>
    ///      <add key="hotmail"
    ///            description="Hotmail SMTP Server"
    ///            host="smtp.live.com"
    ///            port="587"
    ///            requiresAuthentication="true"
    ///            user="user"
    ///            password="pwd"/>
    ///    </SmtpConnections>
    ///  </SmtpConfiguration>
    ///
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
    ///  </RoutingRulesConfiguration>    
    ///  ]]>
    /// </code>
    /// </example>
    public sealed class ConfigFileParser
    {
        /// <summary>
        /// The <see cref="System.Configuration.Configuration">Configuration</see> element
        /// </summary>
        public System.Configuration.Configuration Config { get; private set; }

        /// <summary>
        /// A <see cref="SmtpConfiguration"/> representing the Server SMTP Configuration
        /// </summary>
        public SmtpConfiguration SmtpHost { get; private set; }

        /// <summary>
        /// A <see cref="Dictionary{TKey, TValue}"/> containing the Smtp Connections keyed by the Smtp Configuration Key defined on the <see cref="SmtpConfiguration.Key"/> property
        /// </summary>
        public Dictionary<string, SmtpConfiguration> SmtpConnections { get; private set; }

        /// <summary>
        /// A <see cref="List{T}"/> of <see cref="RoutingRule"/> containing all the rules used to route messages
        /// </summary>
        public List<RoutingRule> RoutingRules { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigFileParser"/>
        /// </summary>
        /// <remarks>This constructor passes the default App.Config as the <see cref="System.Configuration.Configuration"/></remarks>
        /// <exception cref="Exception">If any configuration parameter is invalid, a <see cref="Exception"/> is thrown</exception>
        public ConfigFileParser(): this(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None))
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigFileParser"/>
        /// </summary>
        /// <param name="config">Reference to the configuration file</param>
        /// <remarks>This constructor accepts any valid <see cref="System.Configuration.Configuration"/></remarks>
        /// <exception cref="Exception">If any configuration parameter is invalid, a <see cref="Exception"/> is thrown</exception>
        public ConfigFileParser(System.Configuration.Configuration config)
        {
            Config = config;

            // Validate Sections
            string section_SmtpRouter = "SmtpRouterConfiguration";
            string section_SmtpConfiguration = "SmtpConfiguration";
            string section_RoutingRulesConfiguration = "RoutingRulesConfiguration";

            // Validate the SmtpRouter Configuration
            var SmtpRouterConfiguration = config.GetSection(section_SmtpRouter) as NameValueSection;
            
            SmtpHost = new SmtpConfiguration()
            {
                Key = "CurrentHost",
            };

            if (TryFetchSetting(ref SmtpRouterConfiguration, "Host", out string tempHost))
                SmtpHost.Host = tempHost;
            else
                throw new Exception("SmtpHost.Host is a required field");

            if (TryFetchSetting(ref SmtpRouterConfiguration, "Port", out string tempPort))
            {
                if (int.TryParse(tempPort, out int port))
                    SmtpHost.Port = port;
                else
                    throw new Exception($"Invalid SmtpHost.Port Number. '{tempPort}' is not a valid number.");
            }
            else
                throw new Exception("SmtpHost.Port is a required field");

            SmtpHost.RequiresAuthentication = false;
            if (TryFetchSetting(ref SmtpRouterConfiguration, "RequiresAuthentication", out string tempRequireAuthentication))
            {
                if (bool.TryParse(tempRequireAuthentication, out bool requiresAuthentication))
                    SmtpHost.RequiresAuthentication = requiresAuthentication;
            }

            SmtpHost.UseSSL = false;
            if (TryFetchSetting(ref SmtpRouterConfiguration, "UseSSL", out string tempuseSSL))
            {
                if (bool.TryParse(tempuseSSL, out bool useSSL))
                    SmtpHost.UseSSL = useSSL;
            }

            // Validate the Destination Smtp Configuration
            var SmtpConfiguration = config.GetSection(section_SmtpConfiguration) as SmtpConnectionsSection;
            this.SmtpConnections = new Dictionary<string, SmtpConfiguration>();

            foreach (var s in SmtpConfiguration.SmtpConnections)
            {
                if (s is SmtpConnectionElement e)
                {
                    var tempSmtpConnection = new SmtpConfiguration()
                    {
                        Key = e.Key,
                        Host = e.Host,
                        Description = e.Description,
                        Port = e.Port,
                        RequiresAuthentication = e.RequiresAuthentication,
                        UseSSL = e.UseSSL,
                        User = e.User,
                        Password = e.Password
                    };

                    this.SmtpConnections.Add(tempSmtpConnection.Key, tempSmtpConnection);
                }
            }

            // Validate Routing Rules
            var RoutingRulesConfiguration = config.GetSection(section_RoutingRulesConfiguration) as RoutingRulesSection;
            this.RoutingRules = new List<RoutingRule>();

            foreach (var rule in RoutingRulesConfiguration.RoutingRules)
            {
                if (rule is RoutingRuleElement ruleElement)
                {
                    // Validate the Object Type
                    Type routingRuleType = Type.GetType(ruleElement.Type);
                    if (routingRuleType == null)
                        throw new Exception($"The system could not identify '{ruleElement.Type}' as a valid Type for a Routing Rule. Make sure you add it with the assembly name. Example: 'SMTPRouter.Models.MailFromDomainRoutingRule, SMTPRouter'");

                    // Validate if the Type is in fact a RoutingRule
                    if (!(typeof(RoutingRule).IsAssignableFrom(routingRuleType)))
                        throw new Exception($"The type '{ruleElement.Type}' does not inherit from the 'SMTPRouter.Models.RoutingRule' abstract class, therefore it is not considered a Routing Rule");

                    // Creates the RoutingRule Object
                    RoutingRule routingRule = (RoutingRule)Activator.CreateInstance(routingRuleType);

                    // Sets data to the RoutingRule Object
                    routingRule.ExecutionSequence = ruleElement.ExecutionSequence;
                    routingRule.SmtpConfigurationKey = ruleElement.SmtpConfigurationKey;

                    // Now Parse Parameters
                    //    Example: Parameter1=value1;Parameter2=value2;Parameter3=value3
                    string[] parameters = ruleElement.Parameters.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var p in parameters)
                    {
                        // Now Split the contents
                        string[] parameterContent = p.Split('=');
                        if (parameterContent.Length == 2)
                        {
                            PropertyInfo prop = routingRule.GetType().GetProperty(parameterContent[0], BindingFlags.Public | BindingFlags.Instance);
                            if (prop == null)
                                throw new Exception($"Unable to find a property named '{parameterContent[0]}' inside the type '{ruleElement.Type}'");

                            if (!prop.CanWrite)
                                throw new Exception($"The Property '{parameterContent[0]}' is read-only on type '{ruleElement.Type}'");

                            // Property is valid, set it
                            try
                            {
                                prop.SetValue(routingRule, Convert.ChangeType(parameterContent[1], prop.PropertyType));
                            }
                            catch (Exception e)
                            {
                                throw new Exception($"Unable to set Property '{parameterContent[0]}' to '{parameterContent[1]}' on type '{ruleElement.Type}'. Mismatch types.", e);
                            }
                        }
                    }

                    // Add to the Collection of RoutingRules
                    RoutingRules.Add(routingRule);
                }
            }
        }

        /// <summary>
        /// Fetches the configuration from the Settings
        /// </summary>
        /// <param name="section">Reference to the <see cref="NameValueSection"/></param>
        /// <param name="settingName">The Setting Name (case sensitive)</param>
        /// <param name="value">Variable to store the results</param>
        /// <returns>A <see cref="bool"/> to inform whether the settings was found successfully or not</returns>
        private bool TryFetchSetting(ref NameValueSection section, string settingName, out string value)
        {
            try
            {
                value = section.Settings[settingName].Value;
                return true;
            }
            catch
            {
                value = "";
                return false;
            }
        }

    }
}
