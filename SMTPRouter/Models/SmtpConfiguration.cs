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
        /// <summary>
        /// The path for the Smtp Key working folders
        /// </summary>
        public WorkingFolders Folders { get; private set; }

        /// <summary>
        /// Sets the working directory of the folders
        /// </summary>
        /// <param name="rootDirectory">The root directory for the Folders</param>
        public void SetWorkingDirectory(string rootDirectory)
        {
            // Create Folders structure
            if (!String.IsNullOrEmpty(_Key))
                Folders = new WorkingFolders(System.IO.Path.Combine(rootDirectory, Key), GroupingOption);
            else
                Folders = new WorkingFolders(System.IO.Path.Combine(rootDirectory, "UndefinedSmtp"), GroupingOption);
        }

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


        private int _ActiveConnections;

        /// <summary>
        /// The number of active connections for the SMTP
        /// </summary>
        /// <remarks>
        /// The Default value is Zero. That means each time this SMTP Connection is to be used, the system will create a connection, send the message and disconnect.
        /// When you have more than one active connection, the system will use the next available connection to send the message. If there are too many messages to be sent, messages will be on hold until a SMTP Connection is avalable.
        /// Try to limit your active connections to 10 (ten).
        /// </remarks>
        public int ActiveConnections
        {
            get { return _ActiveConnections; }
            set { SetProperty<int>(ref _ActiveConnections, value); }
        }

        private int _QueueNumber;

        /// <summary>
        /// The number of the Queue. This is used by the Router to help find the queue on the array.
        /// </summary>
        internal int QueueNumber
        {
            get { return _QueueNumber; }
            set { SetProperty<int>(ref _QueueNumber, value); }
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
        ///     <item>StartTlsWhenAvailable = 4 (Elevates the connection to use TLS encryption immediately after reading the greeting and server capabilities, but only if the server supports that)</item>
        /// </list>
        /// </remarks>
        public int SecureSocketOption
        {
            get { return _SecureSocketOption; }
            set { SetProperty<int>(ref _SecureSocketOption, value); }
        }


        private FileGroupingOptions _GroupingOption;

        /// <summary>
        /// The <see cref="FileGroupingOptions"/> to define how to group the messages in the Sent folder
        /// </summary>
        public FileGroupingOptions GroupingOption
        {
            get { return _GroupingOption; }
            set { SetProperty<FileGroupingOptions>(ref _GroupingOption, value); }
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

            GroupingOption = 0;

            // Set Queue Number
            QueueNumber = 0;
        }

    }

    /// <summary>
    /// Grouping options for the files on the Sent folder
    /// </summary>
    public enum FileGroupingOptions: int
    {
        /// <summary>
        /// All files will be saved on the root folder
        /// </summary>
        NoGrouping = 0,
        /// <summary>
        /// All files will be saved on a folder per day
        /// </summary>
        GroupByDate = 1,
        /// <summary>
        /// All files will be saved on a folder per day and hour
        /// </summary>
        GroupByDateAndHour = 2
    }
}
