using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;

namespace SMTPRouter
{
    /// <summary>
    /// A class representing an SMTP Listener
    /// </summary>
    /// <remarks>The Listener will listen to Smtp Commands and fire events when messages are received</remarks>
    /// <example>
    /// A <see cref="Listener"/> can be created using the code below:
    /// <code>
    /// // Create the Listener
    /// var listener = new SMTPRouter.Listener()
    /// {
    ///     ServerName = "localhost",
    ///     Ports = new int[] { 25, 587 },
    ///     RequiresAuthentication = false,
    ///     UseSSL = false
    /// };
    /// 
    /// // Hook into events
    /// listener.ListeningStarted += Server_ListeningStarted;
    /// listener.SessionCreated += Server_SessionCreated;
    /// listener.SessionCommandExecuting += Server_SessionCommandExecuting;
    /// listener.SessionCompleted += Server_SessionCompleted;
    /// listener.MessageReceived += Server_MessageReceived;
    /// </code>
    /// </example>
    public sealed class Listener
    {
        #region Properties

        /// <summary>
        /// Reference to the SmtpServer
        /// </summary>
        public SmtpServer.SmtpServer Server { get; private set; }
        /// <summary>
        /// Defines whether the Listener is active or not
        /// </summary>
        public bool IsListening { get; private set; }
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

        #endregion Properties

        #region Events

        /// <summary>
        /// Event triggered when the Listener started to listen to smtp messages
        /// </summary>
        public event EventHandler<EventArgs> ListeningStarted;

        /// <summary>
        /// Event triggered when a message is received
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageReceived;

        /// <summary>
        /// Event triggered when an SMTP Session is created
        /// </summary>
        public event EventHandler<SessionEventArgs> SessionCreated;

        /// <summary>
        /// Event triggered when an SMTP Session is completed
        /// </summary>
        public event EventHandler<SessionEventArgs> SessionCompleted;

        /// <summary>
        /// Event triggered when an SMTP Command is executing
        /// </summary>
        public event EventHandler<SmtpCommandExecutingEventArgs> SessionCommandExecuting;

        #endregion Events

        #region Constructors

        /// <summary>
        /// Initializes a new instance of a Smtp Listener
        /// </summary>
        public Listener(): this("", null) { }

        /// <summary>
        /// Initializes a new instance of a Smtp Listener
        /// </summary>
        /// <param name="serverName">The Server Name (usually localhost)</param>
        /// <param name="ports">Ports where the service will be available</param>
        public Listener(string serverName, int[] ports): this(serverName, ports, false) { }

        /// <summary>
        /// Initializes a new instance of a Smtp Listener
        /// </summary>
        /// <param name="serverName">The Server Name (usually localhost)</param>
        /// <param name="ports">Ports where the service will be available</param>
        /// <param name="requiresAuthentication">A flag to define whether authentication is required for this smtp server</param>
        public Listener(string serverName, int[] ports, bool requiresAuthentication): this(serverName, ports, requiresAuthentication, false) { }

        /// <summary>
        /// Initializes a new instance of a Smtp Listener
        /// </summary>
        /// <param name="serverName">The Server Name (usually localhost)</param>
        /// <param name="ports">Ports where the service will be available</param>
        /// <param name="requiresAuthentication">A flag to define whether authentication is required for this smtp server</param>
        /// <param name="useSSL">A flag to define whether it is necessary to use SSL</param>
        public Listener(string serverName, int[] ports, bool requiresAuthentication, bool useSSL)
        {
            this.ServerName = serverName;
            this.Ports = ports;
            this.RequiresAuthentication = requiresAuthentication;
            this.UseSSL = useSSL;

            IsListening = false;
        }

        #endregion Constructors

        #region Initialization Methods

        /// <summary>
        /// Initializes a new instance of the Listener
        /// </summary>
        /// <param name="cancellationToken">The Cancellation Token to stop a transaction</param>
        /// <returns>An awaitable <see cref="Task"/> with the listener to Smtp Messages</returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Parameters for the SMTP Server
            ISmtpServerOptions options;

            // Setup the MessageStore
            SmtpMessageStore smtpMessageStore = new SmtpMessageStore();
            smtpMessageStore.MessageReceived += SmtpMessageStore_MessageReceived;

            // Configure the SMTP Server Parameters
            if (this.RequiresAuthentication)
            {
                // Setup the UserAuthenticator
                SmtpAuthenticator smtpAuthenticator = new SmtpAuthenticator();

                options = new SmtpServerOptionsBuilder().ServerName(this.ServerName)
                                                        .Port(this.Ports)
                                                        .AllowUnsecureAuthentication(!this.UseSSL)
                                                        .AuthenticationRequired(true)
                                                        .MessageStore(smtpMessageStore)
                                                        .UserAuthenticator(smtpAuthenticator)
                                                        .Build();
            }
            else
            {
                options = new SmtpServerOptionsBuilder().ServerName(this.ServerName)
                                                        .Port(this.Ports)
                                                        .AllowUnsecureAuthentication(!this.UseSSL)
                                                        .AuthenticationRequired(false)
                                                        .MessageStore(smtpMessageStore)
                                                        .Build();
            }

            // Initialize the SMTP Server
            Server = new SmtpServer.SmtpServer(options);

            // Hook the events
            Server.SessionCreated += Server_OnSessionCreated;
            Server.SessionCompleted += Server_OnSessionCompleted;

            // Sets the Listening to on and kick event
            IsListening = true;
            ListeningStarted?.Invoke(this, null);

            // Starts the SMTP Server
            await Server.StartAsync(cancellationToken);
        }

        #endregion Initialization Methods

        #region Internal Event Handlers

        private void SmtpMessageStore_MessageReceived(object sender, MessageEventArgs e)
        {
            // Trigger the MessageReceived event for the Listener
            MessageReceived?.Invoke(sender, e);
        }

        private void ServerSession_OnCommandExecuting(object sender, SmtpCommandExecutingEventArgs e)
        {
            // Trigger the Session Command Executing event for the Listener
            SessionCommandExecuting?.Invoke(sender, e);
        }

        private void Server_OnSessionCreated(object sender, SessionEventArgs e)
        {
            // Hook event to the Session
            e.Context.CommandExecuting += ServerSession_OnCommandExecuting;

            // Trigger the Session Created event for the Listener
            SessionCreated?.Invoke(sender, e);
        }

        private void Server_OnSessionCompleted(object sender, SessionEventArgs e)
        {
            // Trigger the Session Created event for the Listener
            SessionCompleted?.Invoke(sender, e);
        }

        #endregion Internal Event Handlers
    }
}
