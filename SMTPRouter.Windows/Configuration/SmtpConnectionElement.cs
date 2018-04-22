using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Windows.Configuration
{
    /// <summary>
    /// Represents the Configuration Element for SMTP Connections, to be used on App.Config / Web.Config Files
    /// </summary>
    public sealed class SmtpConnectionElement: ConfigurationElement
    {
        /// <summary>
        /// A unique name to identify the SMTP Configuration
        /// </summary>
        [ConfigurationProperty("key", IsRequired = true)]
        public string Key
        {
            get { return (string)this["key"]; }
        }

        /// <summary>
        /// A Description for the SMTP Configuration
        /// </summary>
        [ConfigurationProperty("description", IsRequired = true)]
        public string Description
        {
            get { return (string)this["description"]; }
        }

        /// <summary>
        /// The SMTP Host
        /// </summary>
        [ConfigurationProperty("host", IsRequired = true)]
        public string Host
        {
            get { return (string)this["host"]; }
        }

        /// <summary>
        /// The Port Number
        /// </summary>
        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get { return (int)this["port"]; }
        }

        /// <summary>
        /// A flag to define whether the SMTP Connection requires Authentication
        /// </summary>
        [ConfigurationProperty("requiresAuthentication", DefaultValue = false)]
        public bool RequiresAuthentication
        {
            get { return (bool)this["requiresAuthentication"]; }
        }

        /// <summary>
        /// Defines whether SSL is necessary to perform the connection.
        /// </summary>
        /// <remarks>Make sure you define the proper port on the <see cref="Port"/> Property. SSL usually uses port 465.</remarks>
        [ConfigurationProperty("useSSL", DefaultValue = false)]
        public bool UseSSL
        {
            get { return (bool)this["useSSL"]; }
        }

        /// <summary>
        /// The User Name to connect to the SMTP
        /// </summary>
        [ConfigurationProperty("user", DefaultValue = "")]
        public string User
        {
            get { return (string)this["user"]; }
        }

        /// <summary>
        /// The Password to connect to the SMTP
        /// </summary>
        [ConfigurationProperty("password", DefaultValue = "")]
        public string Password
        {
            get { return (string)this["password"]; }
        }

        /// <summary>
        /// Returns a string with the information regarding the SMTP Connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Key............: {this.Key}\n" +
                   $"Description....: {this.Description}\n" +
                   $"Host...........: {this.Host}\n" +
                   $"Port...........: {this.Port.ToString()}\n" +
                   $"Use SSL........: {this.UseSSL}\n" +
                   $"Auth Required..: {this.RequiresAuthentication}\n" +
                   $"User...........: {this.User}\n" +
                   $"Password.......: {this.Password}";
        }
    }
}
