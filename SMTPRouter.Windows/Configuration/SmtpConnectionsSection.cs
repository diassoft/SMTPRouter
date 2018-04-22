using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Windows.Configuration
{
    /// <summary>
    /// Represents a <see cref="ConfigurationSection"/> where multiple SmtpConnections can be setup
    /// </summary>
    /// <remarks>The syntax must be followed precisely otherwise the system will not accept the configuration</remarks>
    /// <example>
    /// This is how the App.Config must be setup in order to be processed by this class:
    /// <code>
    /// <![CDATA[
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
    /// ]]>
    /// </code>
    /// </example>
    public sealed class SmtpConnectionsSection: ConfigurationSection
    {
        /// <summary>
        /// A collection of SmtpConfigurations
        /// </summary>
        [ConfigurationProperty(nameof(SmtpConnections), IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(SmtpConnectionCollection),
                                 AddItemName = "add",
                                 ClearItemsName = "clear",
                                 RemoveItemName = "remove")]
        public SmtpConnectionCollection SmtpConnections
        {
            get
            {
                SmtpConnectionCollection smtpConnectionsCollection = (SmtpConnectionCollection)base[nameof(SmtpConnections)];

                return smtpConnectionsCollection;
            }

            set
            {
                SmtpConnectionCollection smtpConnectionsCollection = value;
            }

        }

        /// <summary>
        /// Initializes a new instance of the Smtp <see cref="ConfigurationSection"/>
        /// </summary>
        /// <remarks>The system automatically adds one empty element to the collection</remarks>
        public SmtpConnectionsSection()
        {
            SmtpConnections.Add(new SmtpConnectionElement());
        }
    }
}
