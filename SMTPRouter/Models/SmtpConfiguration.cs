using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter.Models
{
    /// <summary>
    /// Represents an SMTP Connection Configuration
    /// </summary>
    public class SmtpConfiguration: Diassoft.Mvvm.ObservableObjectBase
    {

        private string _Key;

        /// <summary>
        /// A unique name to identify the SMTP Configuration
        /// </summary>
        public string Key
        {
            get { return _Key; }
            set { SetProperty<string>(ref _Key, value); }
        }


        private string _Description;

        /// <summary>
        /// A Description for the SMTP Configuration
        /// </summary>
        public string Description
        {
            get { return _Description; }
            set { SetProperty<string>(ref _Description, value); }
        }


        private string _Host;
        /// <summary>
        /// The SMTP Host
        /// </summary>
        public string Host
        {
            get { return _Host; }
            set { SetProperty<string>(ref _Host, value); }
        }


        private int _Port;
        /// <summary>
        /// The Port Number
        /// </summary>
        public int Port
        {
            get { return _Port; }
            set { SetProperty<int>(ref _Port, value); }
        }


        private bool _RequiresAuthentication;
        /// <summary>
        /// A flag to define whether the SMTP Connection requires Authentication
        /// </summary>
        public bool RequiresAuthentication
        {
            get { return _RequiresAuthentication; }
            set { SetProperty<bool>(ref _RequiresAuthentication, value); }
        }

        private bool _UseSSL;

        /// <summary>
        /// Defines whether SSL is necessary to perform the connection.
        /// </summary>
        /// <remarks>Make sure you define the proper port on the <see cref="Port"/> Property. SSL usually uses port 465.</remarks>
        public bool UseSSL
        {
            get { return _UseSSL; }
            set { SetProperty<bool>(ref _UseSSL, value); }
        }

        private string _User;
        /// <summary>
        /// The User Name to connect to the SMTP
        /// </summary>
        public string User
        {
            get { return _User; }
            set { SetProperty<string>(ref _User, value); }
        }

        private string _Password;
        /// <summary>
        /// The Password to connect to the SMTP
        /// </summary>
        public string Password
        {
            get { return _Password; }
            set { SetProperty<string>(ref _Password, value); }
        }


        private int _SecureSocketOption;

        /// <summary>
        /// Represents the Secure Socket Options
        /// </summary>
        /// <remarks>
        /// Use the following values:
        /// <list type="bullet">
        ///     <item>None = 0 (No SSL or TLS encryption should be used)</item>
        ///     <item>Auto = 1 (The system will decide whether to use SSL or TLS)</item>
        ///     <item>SslOnConnect = 2 (The connection should use SSL or TLS encryption immediately)</item>
        ///     <item>StartTls = 3 (Elevates the connection to use TLS encryption immediately after reading the greeting and server capabilities)</item>
        ///     <item>StartTlsWhenAvailable = 3 (Elevates the connection to use TLS encryption immediately after reading the greeting and server capabilities, but only if the server supports that)</item>
        /// </list>
        /// </remarks>
        public int SecureSocketOption
        {
            get { return _SecureSocketOption; }
            set { SetProperty<int>(ref _SecureSocketOption, value); }
        }


        /// <summary>
        /// Initializes a new instance of the SMTP Configuration
        /// </summary>
        public SmtpConfiguration()
        {
            // Set the port to the default port
            Port = 25;

            UseSSL = false;
            RequiresAuthentication = false;

            SecureSocketOption = 0;
        }

    }
}
