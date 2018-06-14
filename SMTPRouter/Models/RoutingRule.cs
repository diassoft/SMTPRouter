using MimeKit;
using System;
using System.Collections.Generic;
using System.Reflection;
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
        /// The Key of the <see cref="SmtpConfiguration"/> to be used to route the message
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
        /// <param name="routableMessage">Reference to the <see cref="MimeMessage"/></param>
        /// <returns>A <see cref="bool"/> to define whether the rule matches</returns>
        public abstract bool Match(RoutableMessage routableMessage);

        /// <summary>
        /// Creates a Routing Rule based on the input parameters. This function is very useful when reading configuration files.
        /// </summary>
        /// <param name="executionSequence">The Priority of the Rule. The lower the number is, the higher the priority is.</param>
        /// <param name="type">A string containing the <see cref="Type"/> of the object to generate. A valid example would be <code>SMTPRouter.Models.RelayRoutingRule, SMTPRouter</code></param>
        /// <param name="smtpKey">The Key of the <see cref="SmtpConfiguration"/> to be used to route the message</param>
        /// <returns>An instance of the object defined on the <paramref name="type"/> parameter, as long as it inherits from the <see cref="RoutingRule"/> type</returns>
        /// <remarks>
        /// The <see cref="RoutingRule"/> is an abstract class, therefore this function will always return an instance of an object that inherits from the <see cref="RoutingRule"/> class.
        /// </remarks>
        /// <exception cref="TypeLoadException">Triggered when the system cannot load the type defined on <paramref name="type"/> or when the type does not inherit from <see cref="RoutingRule"/></exception>
        public static RoutingRule CreateRule(int executionSequence, string type, string smtpKey)
        {
            return CreateRule(executionSequence, type, smtpKey, "");
        }

        /// <summary>
        /// Creates a Routing Rule based on the input parameters. This function is very useful when reading configuration files.
        /// </summary>
        /// <param name="executionSequence">The Priority of the Rule. The lower the number is, the higher the priority is.</param>
        /// <param name="type">A string containing the <see cref="Type"/> of the object to generate. A valid example would be <code>SMTPRouter.Models.RelayRoutingRule, SMTPRouter</code></param>
        /// <param name="smtpKey">The Key of the <see cref="SmtpConfiguration"/> to be used to route the message</param>
        /// <param name="additionalParameters">Additional Properties that belong to the specific type.</param>
        /// <returns>An instance of the object defined on the <paramref name="type"/> parameter, as long as it inherits from the <see cref="RoutingRule"/> type</returns>
        /// <remarks>
        /// The <see cref="RoutingRule"/> is an abstract class, therefore this function will always return an instance of an object that inherits from the <see cref="RoutingRule"/> class.
        /// The <paramref name="additionalParameters"/> must be setup using <code>Parameter=Value</code>. Multiple parameters can be separated by ";".
        /// </remarks>
        /// <exception cref="TypeLoadException">Triggered when the system cannot load the type defined on <paramref name="type"/> or when the type does not inherit from <see cref="RoutingRule"/></exception>
        /// <exception cref="MissingFieldException">Triggered when the property defined on the <paramref name="additionalParameters"/> does not exists on the type defined at <paramref name="type"/></exception>
        /// <exception cref="MemberAccessException">Triggered when the property defined on the <paramref name="additionalParameters"/> is not accessible (usually there is only a Getter, but not a Setter)</exception>
        /// <exception cref="ArgumentException">Triggered when the property defined on <paramref name="additionalParameters"/> could not be set due to mismatch types</exception>
        public static RoutingRule CreateRule(int executionSequence, string type, string smtpKey, string additionalParameters)
        {
            // Validate the Object Type
            Type routingRuleType = Type.GetType(type);
            if (routingRuleType == null)
                throw new TypeLoadException($"The system could not identify '{type}' as a valid Type for a Routing Rule. Make sure you add it with the assembly name. Example: 'SMTPRouter.Models.MailFromDomainRoutingRule, SMTPRouter'");

            // Validate if the Type is in fact a RoutingRule
            if (!(typeof(RoutingRule).IsAssignableFrom(routingRuleType)))
                throw new TypeLoadException($"The type '{type}' does not inherit from the 'SMTPRouter.Models.RoutingRule' abstract class, therefore it is not considered a Routing Rule");

            // Creates the RoutingRule Object
            RoutingRule routingRule = (RoutingRule)Activator.CreateInstance(routingRuleType);

            // Sets data to the RoutingRule Object
            routingRule.ExecutionSequence = executionSequence;
            routingRule.SmtpConfigurationKey = smtpKey;

            // Now Parse Parameters
            //    Example: Parameter1=value1;Parameter2=value2;Parameter3=value3
            string[] parameters = additionalParameters.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var p in parameters)
            {
                // Now Split the contents
                string[] parameterContent = p.Split('=');
                if (parameterContent.Length == 2)
                {
                    PropertyInfo prop = routingRule.GetType().GetProperty(parameterContent[0], BindingFlags.Public | BindingFlags.Instance);
                    if (prop == null)
                        throw new MissingFieldException($"Unable to find a property named '{parameterContent[0]}' inside the type '{type}'");

                    if (!prop.CanWrite)
                        throw new MemberAccessException($"The Property '{parameterContent[0]}' is read-only on type '{type}'");

                    // Property is valid, set it
                    try
                    {
                        prop.SetValue(routingRule, Convert.ChangeType(parameterContent[1], prop.PropertyType));
                    }
                    catch (Exception e)
                    {
                        throw new ArgumentException($"Unable to set Property '{parameterContent[0]}' to '{parameterContent[1]}' on type '{type}'. Mismatch types.", e);
                    }
                }
            }

            // Add Routing Rule
            return routingRule;
        }

        /// <summary>
        /// Formats the Description to use on the <see cref="RoutingRule.ToString"/> method
        /// </summary>
        /// <param name="displayName">The name to display</param>
        /// <param name="value">The value to display</param>
        /// <returns>A <see cref="string"/> containing the formatted display name and value</returns>
        protected string FormatForToString(string displayName, object value)
        {
            return string.Format("{0}: {1}",
                                 displayName.PadRight(21, '.').Substring(0, 21),
                                 value?.ToString());
        }

        /// <summary>
        /// Generates a string with the Routing Rule contents
        /// </summary>
        /// <returns>A <see cref="string"/> contaning the Routing Rule information</returns>
        public override string ToString()
        {
            return string.Format("{0}\n{1}\n{2}",
                                 FormatForToString("Type", this.GetType().ToString()),
                                 FormatForToString("Sequence", ExecutionSequence.ToString()),
                                 FormatForToString("Smtp Key", SmtpConfigurationKey));
        }
    }
}
