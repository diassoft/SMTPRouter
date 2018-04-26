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
    /// // Hook Events
    /// server.SessionCreated += Server_SessionCreated;
    /// server.SessionCommandExecuting += Server_SessionCommandExecuting;
    /// server.SessionCompleted += Server_SessionCompleted;
    /// server.ListeningStarted += Server_ListeningStarted;
    /// server.MessageReceived += Server_MessageReceived;
    /// server.MessageRoutedSuccessfully += Server_MessageRoutedSuccessfully;
    /// server.MessageNotRouted += Server_MessageNotRouted;
    /// 
    /// // Initialize Services
    /// Task.WhenAll(server.StartAsync(CancellationToken.None)).ConfigureAwait(false);
    /// </code>
    /// </example>
    public sealed class Server
    {
        #region Server Properties

        private bool _isPaused;
        /// <summary>
        /// Defines whether the routing process is Paused or Running
        /// </summary>
        /// <remarks>This property triggers the property <see cref="Router.IsPaused"/> if the <see cref="Router"/> is initialized</remarks>
        public bool IsPaused
        {
            get { return _isPaused; }
            set
            {
                _isPaused = value;

                if (this.Router != null)
                    this.Router.IsPaused = _isPaused;
            }
        }

        #endregion Server Properties

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

        #region Listener Events

        /// <summary>
        /// Event triggered when the Listener started to listen to smtp messages
        /// </summary>
        public event EventHandler<EventArgs> ListeningStarted;

        /// <summary>
        /// Event triggered when a message is received
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageReceived;

        /// <summary>
        /// Event trigered when a SMTP Command is being executed
        /// </summary>
        public event EventHandler<SmtpCommandExecutingEventArgs> SessionCommandExecuting;
        /// <summary>
        /// Event triggered when a SMTP Session is created
        /// </summary>
        public event EventHandler<SessionEventArgs> SessionCreated;
        /// <summary>
        /// Event triggered when a SMTP Session is closed
        /// </summary>
        public event EventHandler<SessionEventArgs> SessionCompleted;

        #endregion Listener Events

        #region Router Events

        /// <summary>
        /// Event triggered when a message is routed successfully
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageRoutedSuccessfully;

        /// <summary>
        /// Event triggered when a message could not be routed successfully
        /// </summary>
        public event EventHandler<MessageErrorEventArgs> MessageNotRouted;

        /// <summary>
        /// Event triggered when a message is about to be purged by the system.
        /// </summary>
        /// <remarks>You can prevent the purge by changing the <see cref="PurgeFileEventArgs.Cancel"/> property to true</remarks>
        public event EventHandler<PurgeFileEventArgs> MessagePurging;

        /// <summary>
        /// Event triggered after messages are purged
        /// </summary>
        public event EventHandler<PurgeFilesEventArgs> MessagesPurged;

        /// <summary>
        /// Event triggered when a general error happens on the processing
        /// </summary>
        /// <remarks>Usually general errors stop the processing so it's important to handle this event</remarks>
        public event EventHandler<GeneralErrorEventArgs> GeneralError;

        #endregion Router Events

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
            IsPaused = false;
            MessageLifespan = new TimeSpan(0, 15, 0);
            MessagePurgeLifespan = new TimeSpan(90, 0, 0, 0);
        }

        #endregion Constructors

        #region Initialization Methods

        /// <summary>
        /// Starts the Server <see cref="Listener"/> and <see cref="Router"/>
        /// </summary>
        /// <param name="cancellationToken">The Cancellation Task</param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Validate Variables before starting the service

            // Initializes the Listener and hook events
            this.Listener = new Listener(this.ServerName, this.Ports, this.RequiresAuthentication, this.UseSSL);
            this.Listener.SessionCreated += Listener_SessionCreated;
            this.Listener.SessionCompleted += Listener_SessionCompleted;
            this.Listener.SessionCommandExecuting += Listener_SessionCommandExecuting;
            this.Listener.ListeningStarted += Listener_ListeningStarted;
            this.Listener.MessageReceived += Listener_MessageReceived;

            // Initializes the Router and hook events
            this.Router = new Router(this.QueueName, this.QueuePath)
            {
                RoutingRules = this.RoutingRules,
                DestinationSmtps = this.DestinationSmtps,
                MessageLifespan = this.MessageLifespan,
                MessagePurgeLifespan = this.MessagePurgeLifespan
            };
            this.Router.GeneralError += Router_GeneralError;
            this.Router.MessageNotRouted += Router_MessageNotRouted;
            this.Router.MessageRoutedSuccessfully += Router_MessageRoutedSuccessfully;
            this.Router.MessagePurging += Router_MessagePurging;
            this.Router.MessagesPurged += Router_MessagesPurged;
            this.Router.IsPaused = this.IsPaused;

            // Start the Service
            await Task.WhenAll(this.Listener.StartAsync(cancellationToken),
                               this.Router.StartAsync(cancellationToken)).ConfigureAwait(false);
        }

        #endregion Initialization Methods

        #region Internal Event Handlers

        private void Listener_SessionCommandExecuting(object sender, SmtpServer.SmtpCommandExecutingEventArgs e)
        {
            SessionCommandExecuting?.Invoke(sender, e);
        }

        private void Listener_SessionCompleted(object sender, SmtpServer.SessionEventArgs e)
        {
            SessionCompleted?.Invoke(sender, e);
        }

        private void Listener_SessionCreated(object sender, SmtpServer.SessionEventArgs e)
        {
            SessionCreated?.Invoke(sender, e);
        }

        private void Router_MessagesPurged(object sender, PurgeFilesEventArgs e)
        {
            MessagesPurged?.Invoke(sender, e);
        }

        private void Router_MessagePurging(object sender, PurgeFileEventArgs e)
        {
            MessagePurging?.Invoke(sender, e);
        }

        /// <summary>
        /// Event triggered when the Router successfully routed a message
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Arguments containing the message that was routed</param>
        private void Router_MessageRoutedSuccessfully(object sender, MessageEventArgs e)
        {
            // Raises the MessageRouterSuccesfully Event
            MessageRoutedSuccessfully?.Invoke(sender, e);
        }

        /// <summary>
        /// Event triggered when the Router could not route a message
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Arguments containing the message that could not be routed and the exception that caused it</param>
        private void Router_MessageNotRouted(object sender, MessageErrorEventArgs e)
        {
            // Raises the MessageNotRoutedSuccesfully Event
            MessageNotRouted?.Invoke(sender, e);
        }

        /// <summary>
        /// Event triggered when a General Error happened on the router
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Arguments containing the exception</param>
        private void Router_GeneralError(object sender, GeneralErrorEventArgs e)
        {
            // Raises the General Error Event
            GeneralError?.Invoke(sender, e);
        }

        /// <summary>
        /// Event triggered when the SMTP Messages started to be listened
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Arguments</param>
        private void Listener_ListeningStarted(object sender, EventArgs e)
        {
            // Raises the Listening Started Event for the Server Component
            ListeningStarted?.Invoke(sender, e);
        }

        /// <summary>
        /// Event triggered everytime a message arrives on the SMTP
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Event Arguments containing the Message that was received</param>
        private void Listener_MessageReceived(object sender, MessageEventArgs e)
        {
            // Sends the Message to the Router
            this.Router.Enqueue(e.MimeMessage);

            // Raises the Message Received Event for the Server Component
            MessageReceived?.Invoke(sender, e);
        }

        #endregion Internal Event Handlers
    }
}
