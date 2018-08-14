using SMTPRouter.Models;
using SmtpServer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SMTPRouter
{
    /// <summary>
    /// Represents the entire Server Implementation, with a <see cref="Listener"/> and a <see cref="Router"/> working together.
    /// </summary>
    /// <remarks>This class implements the entire communication between the <see cref="Listener"/> and the <see cref="Router"/></remarks>
    /// <example>
    /// Use the code below to initialize an instance of the <see cref="Server"/>:
    /// <code>
    /// // Creates the Server
    /// var server = new SMTPRouter.Server("localhost", 25, false, false, "SMTPRouter", "C:\\SMTPRouter\\Queues")
    /// {
    ///     MessageLifespan = new TimeSpan(0, 15, 0),
    ///     RoutingRules = new List<![CDATA[<Models.RoutingRule>]]>()
    ///     {
    ///         new Models.MailFromDomainRoutingRule(10, "gmail.com", "gmail"),
    ///         new Models.MailFromDomainRoutingRule(20, "hotmail.com", "hotmail")
    ///     },
    ///     DestinationSmtps = new Dictionary<![CDATA[<string, Models.SmtpConfiguration>]]>
    ///     {
    ///         { "gmail", new Models.SmtpConfiguration()
    ///             {
    ///                 Host = "smtp.gmail.com",
    ///                 Description = "Google Mail SMTP",
    ///                 Port = 587,
    ///                 RequiresAuthentication = true,
    ///                 User = "user@gmail.com",
    ///                 Password = "",
    ///             }
    ///         },
    ///         { "hotmail", new Models.SmtpConfiguration()
    ///             {
    ///                 Host = "smtp.live.com",
    ///                 Description = "Hotmail SMTP",
    ///                 Port = 587,
    ///                 RequiresAuthentication = true,
    ///                 User = "user@hotmail.com",
    ///                 Password = "",
    ///             }
    ///         }
    ///     },
    /// };
    /// 
    /// // Hook Listener Events
    /// server.Listener.SessionCreated += Server_SessionCreated;
    /// server.Listener.SessionCommandExecuting += Server_SessionCommandExecuting;
    /// server.Listener.SessionCompleted += Server_SessionCompleted;
    /// server.Listener.ListeningStarted += Server_ListeningStarted;
    /// server.Listener.MessageReceived += Server_MessageReceived;
    /// server.Listener.MessageReceivedWithErrors += Server_MessageReceivedWithErrors;
    /// 
    /// // Hook Router Events
    /// server.Router.MessageRoutedSuccessfully += Server_MessageRoutedSuccessfully;
    /// server.Router.MessageNotRouted += Server_MessageNotRouted;
    /// server.Router.MessagePurging += Server_MessagePurging;
    /// server.Router.MessagesPurged += Server_MessagesPurged;
    /// server.Router.MessageNotSent += Server_MessageNotSent;
    /// server.Router.MessageSentSuccessfully += Server_MessageSentSuccessfully;
    /// server.Router.SmtpConnectedSuccessfully += Server_SmtpConnectedSuccessfully;
    /// server.Router.SmtpNotConnected += Server_SmtpNotConnected;
    /// server.Router.SmtpConnectionEnded += Server_SmtpConnectionEnded;
    /// 
    /// // Initialize Services
    /// Task.WhenAll(server.StartAsync(CancellationToken.None)).ConfigureAwait(false);
    /// </code>
    /// </example>
    public sealed class Server
    {
        #region Listener Properties

        /// <summary>
        /// Reference to the Listener used by the Server
        /// </summary>
        public Listener Listener { get; private set; }
        /// <summary>
        /// Reference to the Router used by the Server
        /// </summary>
        public Router Router { get; private set; }

        /// <summary>
        /// Name of the Server where the services will run
        /// </summary>
        public string ServerName { get; set; }
        /// <summary>
        /// Ports where the SMTP Service will be available
        /// </summary>
        public int[] Ports { get; set; }
        /// <summary>
        /// Defines whether it's necessary to use SSL or not
        /// </summary>
        public bool UseSSL { get; set; }
        /// <summary>
        /// Defines whether the SMTP Requires authentication
        /// </summary>
        public bool RequiresAuthentication { get; set; }

        #endregion Listener Properties

        #region Router Properties

        /// <summary>
        /// The name of the queue
        /// </summary>
        public string QueueName { get; set; }
        /// <summary>
        /// The root directory where the queues will be located
        /// </summary>
        public string QueuePath { get; set; }

        /// <summary>
        /// A Dictionary containing the Smtp Configuration based on a Key name
        /// </summary>
        public Dictionary<string, SmtpConfiguration> DestinationSmtps { get; set; }

        /// <summary>
        /// List of Rules to be applied when routing messages
        /// </summary>
        public List<RoutingRule> RoutingRules { get; set; }

        /// <summary>
        /// A structure representing the queue folders
        /// </summary>
        public WorkingFolders Folders { get; set; }

        /// <summary>
        /// The <see cref="TimeSpan"/> a message is stil considered valid. By default, a message lasts 15 minutes after its creation time
        /// </summary>
        /// <remarks>The Lifespan is the maximum time the message is still considered active. After the Lifespan expires, the message is sent to the Error Queue.</remarks>
        public TimeSpan MessageLifespan { get; set; }
        
        /// <summary>
        /// The <see cref="TimeSpan"/> a message remains on queues. By default a message remains there for 90 days before being purged.
        /// </summary>
        public TimeSpan MessagePurgeLifespan { get; set; }

        #endregion Router Properties

        #region Server Events

        /// <summary>
        /// Event triggered when the Listener starts
        /// </summary>
        public event EventHandler<EventArgs> ListenerStarted;

        /// <summary>
        /// Event triggered when the Router starts
        /// </summary>
        public event EventHandler<EventArgs> RouterStarted;

        #endregion Server Events

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the Server
        /// </summary>
        public Server() : this("", null, false, false, "", "") { }

        /// <summary>
        /// Initializes a new Server Instance
        /// </summary>
        /// <param name="serverName">The Server Name (usually localhost)</param>
        /// <param name="port">Port where the service will be available</param>
        /// <param name="requiresAuthentication">A flag to define whether authentication is required for this smtp server</param>
        /// <param name="useSSL">A flag to define whether it is necessary to use SSL</param>
        /// <param name="queueName">Name of the Queue</param>
        /// <param name="queuePath">Root folder where the queue will be located. Ensure you have permissions on that folder.</param>
        public Server(string serverName, int port, bool requiresAuthentication, bool useSSL, string queueName, string queuePath): this(serverName, new int[] { port }, requiresAuthentication, useSSL, queueName, queuePath) { }

        /// <summary>
        /// Initializes a new Server Instance
        /// </summary>
        /// <param name="serverName">The Server Name (usually localhost)</param>
        /// <param name="ports">Ports where the service will be available</param>
        /// <param name="requiresAuthentication">A flag to define whether authentication is required for this smtp server</param>
        /// <param name="useSSL">A flag to define whether it is necessary to use SSL</param>
        /// <param name="queueName">Name of the Queue</param>
        /// <param name="queuePath">Root folder where the queue will be located. Ensure you have permissions on that folder.</param>
        public Server(string serverName, int[] ports, bool requiresAuthentication, bool useSSL, string queueName, string queuePath)
        {
            ServerName = serverName;
            Ports = ports;
            RequiresAuthentication = requiresAuthentication;
            UseSSL = useSSL;
            QueueName = queueName;
            QueuePath = queuePath;
            MessageLifespan = new TimeSpan(0, 15, 0);
            MessagePurgeLifespan = new TimeSpan(90, 0, 0, 0);
        }

        #endregion Constructors

        #region Initialization Methods

        /// <summary>
        /// Starts the Server <see cref="Listener"/> and <see cref="Router"/>
        /// </summary>
        /// <param name="cancellationToken">The Cancellation Task</param>
        /// <returns>An awaitable task with the Start</returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Validate Variables before starting the service

            // Initializes the Listener and hook events
            this.Listener = new Listener(this.ServerName, this.Ports, this.RequiresAuthentication, this.UseSSL);
            this.Listener.MessageReceived += Listener_MessageReceived;
            ListenerStarted?.Invoke(this, new EventArgs());

            // Initializes the Router and hook events
            this.Router = new Router(this.QueueName, this.QueuePath)
            {
                RoutingRules = this.RoutingRules,
                DestinationSmtps = this.DestinationSmtps,
                MessageLifespan = this.MessageLifespan,
                MessagePurgeLifespan = this.MessagePurgeLifespan
            };
            RouterStarted?.Invoke(this, new EventArgs());

            // Start the Service
            await Task.WhenAll(this.Listener.StartAsync(cancellationToken),
                               this.Router.StartAsync(cancellationToken)).ConfigureAwait(false);
        }

        #endregion Initialization Methods

        #region Internal Event Handlers

        /// <summary>
        /// Handle the MessageReceived event by enqueuing the message on the for the Routing Service
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Listener_MessageReceived(object sender, MessageEventArgs e)
        {
            // Sends the Message to the Router
            this.Router.Enqueue(e.RoutableMessage);
        }

        #endregion Internal Event Handlers
    }
}
