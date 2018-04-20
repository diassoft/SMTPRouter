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
        public Listener()
        {
            IsListening = false;
        }

        #endregion Constructors

        /// <summary>
        /// Initializes a new instance of the Listener
        /// </summary>
        /// <param name="serverName">The Hostname (usually Localhost or 127.0.0.1)</param>
        /// <param name="ports">The ports where the service should be initialized</param>
        /// <returns>An awaitable <see cref="Task"/> with the listener to Smtp Messages</returns>
        public async Task StartAsync(string serverName, params int[] ports)
        {
            await StartAsync(CancellationToken.None, serverName, ports, false, false);
        }

        /// <summary>
        /// Initializes a new instance of the Listener
        /// </summary>
        /// <param name="cancellationToken">The Cancellation Token to stop a transaction</param>
        /// <param name="serverName">The Hostname (usually Localhost or 127.0.0.1)</param>
        /// <param name="ports">The ports where the service should be initialized</param>
        /// <returns>An awaitable <see cref="Task"/> with the listener to Smtp Messages</returns>
        public async Task StartAsync(CancellationToken cancellationToken, string serverName, params int[] ports)
        {
            await StartAsync(cancellationToken, serverName, ports, false, false);
        }

        /// <summary>
        /// Initializes a new instance of the Listener
        /// </summary>
        /// <param name="serverName">The Hostname (usually Localhost or 127.0.0.1)</param>
        /// <param name="port">The port where the service should be initialized</param>
        /// <param name="requiresAuthentication">A flag to define whether the Smtp Requires authentication or not</param>
        /// <returns>An awaitable <see cref="Task"/> with the listener to Smtp Messages</returns>
        public async Task StartAsync(string serverName, int port, bool requiresAuthentication)
        {
            await StartAsync(CancellationToken.None, serverName, new int[] { port });
        }

        /// <summary>
        /// Initializes a new instance of the Listener
        /// </summary>
        /// <param name="serverName">The Hostname (usually Localhost or 127.0.0.1)</param>
        /// <param name="ports">The ports where the service should be initialized</param>
        /// <param name="requiresAuthentication">A flag to define whether the Smtp Requires authentication or not</param>
        /// <returns>An awaitable <see cref="Task"/> with the listener to Smtp Messages</returns>
        public async Task StartAsync(string serverName, int[] ports, bool requiresAuthentication)
        {
            await StartAsync(CancellationToken.None, serverName, ports, requiresAuthentication, false);
        }

        /// <summary>
        /// Initializes a new instance of the Listener
        /// </summary>
        /// <param name="cancellationToken">The Cancellation Token to stop a transaction</param>
        /// <param name="serverName">The Hostname (usually Localhost or 127.0.0.1)</param>
        /// <param name="port">The port where the service should be initialized</param>
        /// <param name="requiresAuthentication">A flag to define whether the Smtp Requires authentication or not</param>
        /// <returns>An awaitable <see cref="Task"/> with the listener to Smtp Messages</returns>
        public async Task StartAsync(CancellationToken cancellationToken, string serverName, int port, bool requiresAuthentication)
        {
            await StartAsync(cancellationToken, serverName, new int[] { port }, requiresAuthentication, false);
        }

        /// <summary>
        /// Initializes a new instance of the Listener
        /// </summary>
        /// <param name="cancellationToken">The Cancellation Token to stop a transaction</param>
        /// <param name="serverName">The Hostname (usually Localhost or 127.0.0.1)</param>
        /// <param name="ports">The ports where the service should be initialized</param>
        /// <param name="requiresAuthentication">A flag to define whether the Smtp Requires authentication or not</param>
        /// <returns>An awaitable <see cref="Task"/> with the listener to Smtp Messages</returns>
        public async Task StartAsync(CancellationToken cancellationToken, string serverName, int[] ports, bool requiresAuthentication)
        {
            await StartAsync(cancellationToken, serverName, ports, requiresAuthentication, false);
        }

        /// <summary>
        /// Initializes a new instance of the Listener
        /// </summary>
        /// <param name="serverName">The Hostname (usually Localhost or 127.0.0.1)</param>
        /// <param name="port">The port where the service should be initialized</param>
        /// <param name="requiresAuthentication">A flag to define whether the Smtp Requires authentication or not</param>
        /// <param name="useSSL">A flag to define whether the connection will use SSL or not. Usually when SSL is activated the port should be 465.</param>
        /// <returns>An awaitable <see cref="Task"/> with the listener to Smtp Messages</returns>
        public async Task StartAsync(string serverName, int port, bool requiresAuthentication, bool useSSL)
        {
            await StartAsync(CancellationToken.None, serverName, new int[] { port }, requiresAuthentication, useSSL);
        }

        /// <summary>
        /// Initializes a new instance of the Listener
        /// </summary>
        /// <param name="serverName">The Hostname (usually Localhost or 127.0.0.1)</param>
        /// <param name="ports">The ports where the service should be initialized</param>
        /// <param name="requiresAuthentication">A flag to define whether the Smtp Requires authentication or not</param>
        /// <param name="useSSL">A flag to define whether the connection will use SSL or not. Usually when SSL is activated the port should be 465.</param>
        /// <returns>An awaitable <see cref="Task"/> with the listener to Smtp Messages</returns>
        public async Task StartAsync(string serverName, int[] ports, bool requiresAuthentication, bool useSSL)
        {
            await StartAsync(CancellationToken.None, serverName, ports, requiresAuthentication, useSSL);
        }

        /// <summary>
        /// Initializes a new instance of the Listener
        /// </summary>
        /// <param name="cancellationToken">The Cancellation Token to stop a transaction</param>
        /// <param name="serverName">The Hostname (usually Localhost or 127.0.0.1)</param>
        /// <param name="ports">The ports where the service should be initialized</param>
        /// <param name="requiresAuthentication">A flag to define whether the Smtp Requires authentication or not</param>
        /// <param name="useSSL">A flag to define whether the connection will use SSL or not. Usually when SSL is activated the port should be 465.</param>
        /// <returns>An awaitable <see cref="Task"/> with the listener to Smtp Messages</returns>
        public async Task StartAsync(CancellationToken cancellationToken, string serverName, int[] ports, bool requiresAuthentication, bool useSSL)
        {
            // Parameters for the SMTP Server
            ISmtpServerOptions options;

            // Setup the MessageStore
            SmtpMessageStore smtpMessageStore = new SmtpMessageStore();
            smtpMessageStore.MessageReceived += SmtpMessageStore_MessageReceived;

            // Configure the SMTP Server Parameters
            if (requiresAuthentication)
            {
                // Setup the UserAuthenticator
                SmtpAuthenticator smtpAuthenticator = new SmtpAuthenticator();

                options = new SmtpServerOptionsBuilder().ServerName(serverName)
                                                        .Port(ports)
                                                        .AllowUnsecureAuthentication(!useSSL)
                                                        .AuthenticationRequired(true)
                                                        .MessageStore(smtpMessageStore)
                                                        .UserAuthenticator(smtpAuthenticator)
                                                        .Build();
            }
            else
            {
                options = new SmtpServerOptionsBuilder().ServerName(serverName)
                                                        .Port(ports)
                                                        .AllowUnsecureAuthentication(!useSSL)
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
