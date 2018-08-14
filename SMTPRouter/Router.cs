using MailKit.Net.Smtp;
using MimeKit;
using SMTPRouter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Xml.Serialization;
using System.Collections.Concurrent;
using SMTPRouter.Extensions;

namespace SMTPRouter
{
    /// <summary>
    /// A class representing a Message Router.
    /// </summary>
    /// <remarks>The router receives the message and then routes it</remarks>
    /// <example>
    /// A <see cref="Router"/> can be created using the code below:
    /// <code>
    /// // Create the Router
    /// var router = new SMTPRouter.Router("SMTPRouter", "C:\\SMTPRouter\\Queues")
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
    ///                 SecureSocketOption = 1,
    ///                 ActiveConnections = 1,
    ///                 GroupingOption = FileGroupingOptions.GroupByDateAndHour
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
    ///                 SecureSocketOption = 1,
    ///                 ActiveConnections = 1,
    ///                 GroupingOption = FileGroupingOptions.GroupByDateAndHour
    ///             }
    ///         }
    ///     },
    /// };
    /// router.MessageRoutedSuccessfully += Server_MessageRoutedSuccessfully;
    /// router.MessageNotRouted += Server_MessageNotRouted;
    /// router.MessageNotSent += Server_MessageNotSent;
    /// </code>
    /// </example>
    public sealed class Router
    {

        #region Properties

        /// <summary>
        /// Returns a flag to inform if the router is initialized
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Defines whether the routing process is Paused or Running
        /// </summary>
        public bool IsPaused { get; set; }

        /// <summary>
        /// Represents the Queue Name
        /// </summary>
        public string QueueName { get; private set; }

        /// <summary>
        /// Represents the Queue Path
        /// </summary>
        public string QueuePath { get; private set; }

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
        public WorkingFolders Folders { get; internal set; }

        /// <summary>
        /// The <see cref="TimeSpan"/> a message is stil considered valid to retry. By default, a message lasts 15 minutes after its creation time
        /// </summary>
        /// <remarks>The Lifespan is the maximum time the message is still considered active. After the Lifespan expires, the message is sent to the Error Queue.</remarks>
        public TimeSpan MessageLifespan { get; set; }

        /// <summary>
        /// The <see cref="TimeSpan"/> a message remains on the queues. By default a message remains there for 90 days before being purged.
        /// </summary>
        public TimeSpan MessagePurgeLifespan { get; set; }

        /// <summary>
        /// List of Administrators of the system, separated by semicolon. Leave it blank if you do not want messages to be sent to administrators.
        /// </summary>
        /// <remarks>
        /// Administrators can be notified in case something goes wrong with the service
        /// </remarks>
        public string AdministratorsEmail { get; set; }

        /// <summary>
        /// The system email account, to be used as the mail sender when messages are sent from the system itself.
        /// </summary>
        public string SystemEmail { get; set; }

        /// <summary>
        /// A list of IP addresses to accept messages from
        /// </summary>
        /// <remarks>An Empty List means all IP addresses will be accepted</remarks>
        public List<string> AcceptedIPAddresses { get; set; }

        /// <summary>
        /// A list of IP addresses to reject messages from
        /// </summary>
        /// <remarks>An Empty List means no rejections will be made</remarks>
        public List<string> RejectedIPAddresses { get; set; }

        #endregion Properties

        #region Events

        /// <summary>
        /// Event triggered when a message is routed successfully
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageRoutedSuccessfully;

        /// <summary>
        /// Event triggered when a message could not be routed successfully
        /// </summary>
        public event EventHandler<MessageErrorEventArgs> MessageNotRouted;

        /// <summary>
        /// Event triggered when a message is queued successfully
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageQueuedSuccessfully;

        /// <summary>
        /// Event triggered when a message could not be queued successfully
        /// </summary>
        public event EventHandler<MessageErrorEventArgs> MessageNotQueued;

        /// <summary>
        /// Event triggered when a message is sent successfully
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageSentSuccessfully;

        /// <summary>
        /// Event triggered when a message could not be sent successfully
        /// </summary>
        public event EventHandler<MessageErrorEventArgs> MessageNotSent;

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
        /// Event triggered after the Smtp Connection is successfull
        /// </summary>
        public event EventHandler<SmtpConnectionEventArgs> SmtpConnectedSuccessfully;

        /// <summary>
        /// Event triggered after the Smtp Connection failed
        /// </summary>
        public event EventHandler<SmtpConnectionEventArgs> SmtpNotConnected;

        /// <summary>
        /// Event triggered when a thread processing an specific Smtp Connection has ended
        /// </summary>
        public event EventHandler<SmtpConnectionEventArgs> SmtpConnectionEnded;

        /// <summary>
        /// Event triggered when a general error happens on the processing
        /// </summary>
        /// <remarks>Usually general errors stop the processing so it's important to handle this event</remarks>
        public event EventHandler<GeneralErrorEventArgs> GeneralError;

        #endregion Events

        #region Concurrent Queues

        /// <summary>
        /// Array of Smtp Message Queues
        /// </summary>
        internal ConcurrentQueue<RoutableMessage>[] SmtpQueues;

        /// <summary>
        /// The Queue containing the messages to be routed
        /// </summary>
        internal ConcurrentQueue<RoutableMessage> MessagesToRouteQueue;

        #endregion Concurrent Queues

        #region Constructors

        /// <summary>
        /// Initializes a new instance of a <see cref="Router"/>
        /// </summary>
        public Router(): this("SMTPRouter") { }

        /// <summary>
        /// Initializes a new instance of a <see cref="Router"/> and sets the Queue Path to be the Current Directory
        /// </summary>
        /// <param name="queueName">Name of the Queue</param>
        /// <remarks>Inner blanks will be removed from <paramref name="queueName"/>. If the value is an empty string, a <see cref="ArgumentNullException"/> will be thrown.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="queueName"/> parameter is null or an empty string</exception>
        public Router(string queueName): this(queueName, System.IO.Directory.GetCurrentDirectory()) { }

        /// <summary>
        /// Initializes a new instance of a <see cref="Router"/>
        /// </summary>
        /// <param name="queueName">Name of the Queue</param>
        /// <param name="queuePath">Root folder where the queue will be located. Ensure you have permissions on that folder.</param>
        /// <remarks>Inner blanks will be removed from <paramref name="queueName"/> and <paramref name="queuePath"/>. If the value is an empty string, a <see cref="ArgumentNullException"/> will be thrown.</remarks>
        /// <exception cref="ArgumentNullException">Either <paramref name="queueName"/> or <paramref name="queuePath"/> parameters are null or an empty string</exception>
        public Router(string queueName, string queuePath)
        {
            // Remove Blank Spaces
            QueueName = queueName.Replace(" ", "");
            QueuePath = queuePath.Replace(" ", "");

            if (string.IsNullOrWhiteSpace(QueueName))
                throw new ArgumentNullException(nameof(queueName), $"'{nameof(queueName)}' cannot be empty or blank");

            if (string.IsNullOrWhiteSpace(QueuePath))
                throw new ArgumentNullException(nameof(queuePath), $"'{nameof(queuePath)}' cannot be empty or blank");

            // Set Default Lifespan
            MessageLifespan = new TimeSpan(0, 15, 0);
            MessagePurgeLifespan = new TimeSpan(90, 0, 0, 0, 0);

            // Set the folder structure
            Folders = new WorkingFolders(queuePath);

            // Initialize Collections
            DestinationSmtps = new Dictionary<string, SmtpConfiguration>();
            RoutingRules = new List<RoutingRule>();

            // Initialize IP Addresses Collections
            AcceptedIPAddresses = new List<string>();
            RejectedIPAddresses = new List<string>();

            // Initialize Queues
            try
            {
                // Create Folders if they do not exist already
                if (!Directory.Exists(Folders.OutgoingFolder)) Directory.CreateDirectory(Folders.OutgoingFolder);
                if (!Directory.Exists(Folders.InQueueFolder)) Directory.CreateDirectory(Folders.InQueueFolder);
                if (!Directory.Exists(Folders.RejectedFolder)) Directory.CreateDirectory(Folders.RejectedFolder);
                if (!Directory.Exists(Folders.ErrorFolder)) Directory.CreateDirectory(Folders.ErrorFolder);
            }
            catch (Exception e)
            {
                // The Router could not initialize the queues, throw the exception and keep the details in the Inner Exception
                throw new QueuesNotInitializedException(e);
            }
        }

        #endregion Constructors

        #region Initialization Methods

        /// <summary>
        /// Starts the Message Routing
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Verify the number of Smtp Connections
            if (DestinationSmtps.Count == 0)
                throw new ArgumentException($"'{nameof(DestinationSmtps)}' cannot be empty. You need to have at least one Smtp Connection in order to start routing messages.");

            // Create SmtpQUeues Array
            SmtpQueues = new ConcurrentQueue<RoutableMessage>[DestinationSmtps.Count];

            // List of Tasks to be running in parallel
            List<Task> TaskList = new List<Task>();

            // Add Background Tasks
            TaskList.Add(new Task(() => EnqueueOutgoingMessages(cancellationToken)));
            TaskList.Add(new Task(() => RouteMessages(cancellationToken)));
            TaskList.Add(new Task(() => PurgeOldMessages(cancellationToken)));

            // Create the MessagesToRouteQueue
            MessagesToRouteQueue = new ConcurrentQueue<RoutableMessage>();

            // Initialize SMTP Connections

            // Set Queue Numbers
            int QueueNumber = 0;

            foreach (KeyValuePair<string, SmtpConfiguration> kvp in DestinationSmtps)
            {
                // Initialize folder structure for each Smtp Key
                kvp.Value.SetWorkingDirectory(Path.Combine(this.QueuePath, "Routed"));
                if (!Directory.Exists(kvp.Value.Folders.InQueueFolder)) Directory.CreateDirectory(kvp.Value.Folders.InQueueFolder);
                if (!Directory.Exists(kvp.Value.Folders.SentFolder)) Directory.CreateDirectory(kvp.Value.Folders.SentFolder);
                if (!Directory.Exists(kvp.Value.Folders.ErrorFolder)) Directory.CreateDirectory(kvp.Value.Folders.ErrorFolder);

                // Set Index for the Smtp Connection
                DestinationSmtps[kvp.Key].QueueNumber = QueueNumber;

                // Create the Queue
                SmtpQueues[QueueNumber] = new ConcurrentQueue<RoutableMessage>();

                // Number of Connections
                int tempActiveConnections = System.Math.Max(kvp.Value.ActiveConnections, 1);

                // Create the Tasks
                for (int connectionNumber = 1; connectionNumber <= tempActiveConnections; connectionNumber++)
                {
                    int tempConnectionNumber = connectionNumber;
                    TaskList.Add(new Task(() => SendRoutedMessages(tempConnectionNumber, kvp.Value, cancellationToken)));
                }
                    

                QueueNumber++;
            }

            // Enqueue all existing messages back to the Message Queues
            EnqueueExistingInQueueMessages();

            // Start All Async Tasks
            for (int t = 0; t < TaskList.Count; t++)
                TaskList[t].Start();

            // Start the Tasks
            await Task.WhenAll(TaskList.ToArray()).ConfigureAwait(false);
         }

        #endregion Initialization Methods

        #region Message Processing Methods

        /// <summary>
        /// Adds a <see cref="RoutableMessage"/> to the Messages To Route queue
        /// </summary>
        /// <param name="routableMessage">A <see cref="RoutableMessage"/> to be routed</param>
        /// <remarks>If the <see cref="RoutableMessage.FileName"/> is empty, the system understand this is a brand new message, therefore it will create a file and send it to the 'InQueue' folder</remarks>
        public bool Enqueue(RoutableMessage routableMessage)
        {
            // Current File Location
            string FileCurrentLocation = routableMessage.FileName;

            try
            {
                // When there is no file name, it's a brand new message. Dump the file to the folder
                if (string.IsNullOrEmpty(routableMessage.FileName))
                {
                    routableMessage.FileName = Path.Combine(Folders.InQueueFolder,
                                                            string.Format("{0}-{1}.EML", routableMessage.CreationDateTime.ToString("yyyyMMddHHmmss"), Guid.NewGuid().ToString()));
                    routableMessage.SaveToFile();
                    FileCurrentLocation = routableMessage.FileName;
                }

                // Move file to the InQueue folder, unless it's already there
                if (routableMessage.FileName != Path.Combine(Folders.InQueueFolder, Path.GetFileName(routableMessage.FileName)))
                {
                    File.Move(routableMessage.FileName, Path.Combine(Folders.InQueueFolder, Path.GetFileName(routableMessage.FileName)));
                    FileCurrentLocation = Path.Combine(Folders.InQueueFolder, Path.GetFileName(routableMessage.FileName));
                    routableMessage.FileName = FileCurrentLocation;
                }

                // Add to the concurrent queue
                lock (MessagesToRouteQueue)
                {
                    MessagesToRouteQueue.Enqueue(routableMessage);
                }

                // Notify the message has been queued
                MessageQueuedSuccessfully?.Invoke(this, new MessageEventArgs(routableMessage));

                return true;
            }
            catch (Exception e)
            {
                // Move file to the Error Queue (there is no Retry Queue for first level messages)
                File.Move(FileCurrentLocation, Path.Combine(Folders.ErrorFolder, Path.GetFileName(routableMessage.FileName)));

                // Notify that the message was not queued
                MessageNotQueued?.Invoke(this, new MessageErrorEventArgs(routableMessage, e));

                return false;
            }
        }

        /// <summary>
        /// Adds a <see cref="RoutableMessage"/> to the Messages To Route queue
        /// </summary>
        /// <param name="fileName">The path where the message is located</param>
        public bool Enqueue(string fileName)
        {
            return Enqueue(RoutableMessage.LoadFromFile(fileName));
        }

        /// <summary>
        /// Enqueue all messages already existing on the InQueue. It will do it for the InQueue folder on the Root level and also for the InQueue folder inside each Smtp Connection.
        /// </summary>
        /// <remarks>This should only be called during the Initialization of the Router</remarks>
        private void EnqueueExistingInQueueMessages()
        {
            // Get all files to process (since the number of files is usually small, it is ok to use GetFiles method)
            DirectoryInfo dirInfo = new DirectoryInfo(Folders.InQueueFolder);
            IEnumerable<FileInfo> files = dirInfo.EnumerateFiles("*.EML");

            foreach (FileInfo fi in files)
                Enqueue(fi.FullName);

            // Now enqueue messages already routed
            foreach (var kvp in DestinationSmtps)
            {
                dirInfo = new DirectoryInfo(kvp.Value.Folders.InQueueFolder);
                files = dirInfo.EnumerateFiles("*.EML");

                lock (SmtpQueues[kvp.Value.QueueNumber])
                {
                    foreach (FileInfo fi in files)
                    {
                        SmtpQueues[kvp.Value.QueueNumber].Enqueue(RoutableMessage.LoadFromFile(fi.FullName));
                    }
                }
            }
        }

        /// <summary>
        /// Enqueue messages on the Outgoing Queue by adding them to the <see cref="MessagesToRouteQueue"/> and move the file to the "InQueue" folder
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        private void EnqueueOutgoingMessages(CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                if (!IsPaused)
                {
                    // Get all files to process (since the number of files is usually small, it is ok to use GetFiles method)
                    DirectoryInfo dirInfo = new DirectoryInfo(Folders.OutgoingFolder);
                    IEnumerable<FileInfo> files = dirInfo.EnumerateFiles("*.EML");

                    foreach (FileInfo fi in files)
                        Enqueue(fi.FullName);

                    // Wait before attempting again
                    Task.Delay(2000).Wait(cancellationToken);
                }
                else
                {
                    // Nothing to do other than wait a few seconds before trying again
                    Task.Delay(5000).Wait(cancellationToken);
                }
            }
        }

        /// <summary>
        /// Get messages form the <see cref="MessagesToRouteQueue"/> and routes them to the proper Smtp Queue
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <remarks>This method is expected to be run in different tasks at the same time, hence it uses a <see cref="ConcurrentQueue{T}"/> to avoid deadlocks</remarks>
        private void RouteMessages(CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                if (!IsPaused)
                {
                    // Pull next message from the queue
                    bool Dequeued = false;
                    RoutableMessage routableMessage = null;
                    
                    // Lock the Queue to Ensure Thread-Safety
                    lock (MessagesToRouteQueue)
                    {
                        Dequeued = MessagesToRouteQueue.TryDequeue(out routableMessage);
                    }

                    if ((Dequeued) && (routableMessage != null))
                    {
                        // The Message Current Location
                        string FileCurrentLocation = routableMessage.FileName;

                        // Route the message that just came from the queue
                        try
                        {
                            // Validate Incoming IP Address (unless the Force Routing flag is set)
                            if (!routableMessage.ForceRouting)
                            {
                                if (AcceptedIPAddresses?.Count > 0)
                                {
                                    // Check if Incoming IP Address is on the list
                                    if (!AcceptedIPAddresses.Contains(routableMessage.IPAddress))
                                        throw new UnauthorizedSenderException(routableMessage, RejectReasons.NotInAcceptedAddressesList);
                                }

                                if (RejectedIPAddresses?.Count > 0)
                                {
                                    // Check if Incoming IP Address is on the rejection list
                                    if (RejectedIPAddresses.Contains(routableMessage.IPAddress))
                                        throw new UnauthorizedSenderException(routableMessage, RejectReasons.ExistsInRejectedAddressesList);
                                }
                            }

                            // Validate Rules
                            if (RoutingRules == null)
                                throw new Exception("No rules configured to process routing. The message will be sent to the Error Queue after exceeding the Maximum Number of Attempts.");

                            if (RoutingRules?.Count == 0)
                                throw new Exception("No rules configured to process routing. The message will be sent to the Error Queue after exceeding the Maximum Number of Attempts.");

                            // Validate SMTP Configurations
                            if (DestinationSmtps == null)
                                throw new Exception("There is no SMTP configuration to route the emails to. The message will be sent to the Error Queue after exceeding the Maximum Number of Attempts");

                            if (DestinationSmtps?.Count == 0)
                                throw new Exception("There is no SMTP configuration to route the emails to. The message will be sent to the Error Queue after exceeding the Maximum Number of Attempts");

                            // Ensure there is a valid Routable Message
                            if (routableMessage == null)
                                throw new Exception("The Routable Message is not valid");

                            // Ensure there is a valid MimeMessage
                            if (routableMessage.Message == null)
                                throw new Exception("The MimeMessage is nov valid");

                            // The rule selected for use
                            RoutingRule selectedRule = null;

                            // Fetch Rules sorted by the Execution Sequence
                            foreach (RoutingRule rule in (from r in RoutingRules orderby r.ExecutionSequence select r))
                            {
                                // Try the rule
                                if (rule.Match(routableMessage))
                                {
                                    selectedRule = rule;

                                    // No need to continue loop
                                    break;
                                }
                            }

                            // Ensure a rule was found
                            if (selectedRule == null)
                                throw new Exception($"Unable to find a {nameof(RoutingRule)} for the given message");

                            // Verify SMTP Connection
                            if (string.IsNullOrWhiteSpace(selectedRule.SmtpConfigurationKey))
                                throw new Exception($"The {nameof(selectedRule.SmtpConfigurationKey)} is empty");

                            if (!DestinationSmtps.ContainsKey(selectedRule.SmtpConfigurationKey))
                                throw new Exception($"There is no Smtp configured for the key '{selectedRule.SmtpConfigurationKey}'");

                            SmtpConfiguration smtpConfiguration = DestinationSmtps[selectedRule.SmtpConfigurationKey];

                            if (smtpConfiguration == null)
                                throw new ArgumentNullException($"The Smtp by the key '{selectedRule.SmtpConfigurationKey}' is misconfigured");

                            // All checks passed, Send it to the proper queue

                            // Move files and enqueue
                            File.Move(routableMessage.FileName, Path.Combine(smtpConfiguration.Folders.InQueueFolder, Path.GetFileName(routableMessage.FileName)));
                            FileCurrentLocation = Path.Combine(smtpConfiguration.Folders.InQueueFolder, Path.GetFileName(routableMessage.FileName));
                            routableMessage.FileName = FileCurrentLocation;
                            SmtpQueues[smtpConfiguration.QueueNumber].Enqueue(routableMessage);

                            // Notify the message was routed
                            MessageRoutedSuccessfully?.Invoke(this, new MessageEventArgs(routableMessage));
                        }
                        catch (UnauthorizedSenderException e)
                        {
                            // Move the message to the Rejected folder
                            try
                            {
                                File.Move(FileCurrentLocation, Path.Combine(Folders.RejectedFolder, Path.GetFileName(FileCurrentLocation)));
                            }
                            catch (Exception e2)
                            {
                                // That is a very unlikely situation 
                                GeneralError?.Invoke(this, new GeneralErrorEventArgs(new Exception("Unable to move file to Rejected Folder", e2), nameof(RouteMessages)));
                            }

                            // Notify the message was not routed
                            MessageNotRouted?.Invoke(this, new MessageErrorEventArgs(routableMessage, e));
                        }
                        catch (Exception e)
                        {
                            // Move the message to the retry or error folder
                            try
                            {
                                File.Move(FileCurrentLocation, Path.Combine(Folders.ErrorFolder, Path.GetFileName(FileCurrentLocation)));
                            }
                            catch (Exception e2)
                            {
                                // That is a very unlikely situation 
                                GeneralError?.Invoke(this, new GeneralErrorEventArgs(new Exception("Unable to move file to Error Folder", e2), nameof(RouteMessages)));
                            }

                            // Notify the message was not routed
                            MessageNotRouted?.Invoke(this, new MessageErrorEventArgs(routableMessage, new MessageNotRoutedException(routableMessage, e)));
                        }
                    }
                    else
                    {
                        // Wait two seconds before trying again
                        Task.Delay(2000).Wait(cancellationToken);
                    }
                }
                else
                {
                    // Nothing to do other than wait a few seconds before trying again
                    Task.Delay(5000).Wait(cancellationToken);
                }
            }
        }

        /// <summary>
        /// Connects to the Smtp using the Smtp Configuration
        /// </summary>
        /// <param name="smtpClient">Reference to the Smtp Client that holds the connection. It can be a null variable.</param>
        /// <param name="smtpConfiguration">The Smtp Configuration</param>
        /// <returns>A <see cref="bool"/> to define whether the connection was successful or not</returns>
        private bool TryConnectToSmtp(ref SmtpClient smtpClient, SmtpConfiguration smtpConfiguration)
        {
            // Temporary SmtpClient
            SmtpClient tempClient = new SmtpClient();
            bool hasConnected = false;

            try
            {
                // Set Timeout (5 minutes)
                tempClient.Timeout = 5 * 60 * 1000;

                // Tries to connect
                tempClient.Connect(smtpConfiguration.Host,
                                   smtpConfiguration.Port,
                                   (MailKit.Security.SecureSocketOptions)smtpConfiguration.SecureSocketOption);

                hasConnected = true;

                if (smtpConfiguration.RequiresAuthentication)
                {
                    tempClient.Authenticate(smtpConfiguration.User,
                                             smtpConfiguration.Password);
                }

                // Notify the connection happened successfully
                SmtpConnectedSuccessfully?.Invoke(this, new SmtpConnectionEventArgs(smtpConfiguration));

                // Set the return variable to the tempClient
                smtpClient = tempClient;

                // Notify the connection worked
                return true;
            }
            catch (Exception e)
            {
                var newException = new SmtpConnectionFailedException(smtpConfiguration, hasConnected, false, e);
                SmtpNotConnected?.Invoke(this, new SmtpConnectionEventArgs(smtpConfiguration, newException));

                return false;
            }
        }

        /// <summary>
        /// Process the Smtp Queue for messages already routed
        /// </summary>
        /// <param name="connectionNumber">The number of the connection on the same queue. This information is inserted on the header of the MimeMessage</param>
        /// <param name="smtpConfiguration">The configuration information for the queue</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <remarks>This method is expected to run into multiple tasks, hence it uses a <see cref="ConcurrentQueue{T}"/></remarks>
        private void SendRoutedMessages(int connectionNumber, SmtpConfiguration smtpConfiguration, CancellationToken cancellationToken)
        {
            // Connects to the Smtp
            SmtpClient client = new SmtpClient();

            // Control the Number of Attempts
            int attempts = 0;
            int maxAttempts = 20;

            while (cancellationToken.IsCancellationRequested == false)
            {
                try
                {
                    // Pull a message from the queue
                    RoutableMessage routableMessage = null;
                    bool Dequeued = false;

                    lock (SmtpQueues[smtpConfiguration.QueueNumber])
                    {
                        Dequeued = SmtpQueues[smtpConfiguration.QueueNumber].TryDequeue(out routableMessage);
                    }

                    if ((Dequeued) && (routableMessage != null))
                    {
                        try
                        {
                            // Append details to the header
                            var receiveByString = string.Format("Queue [{0}] at Connection [{1}] of Smtp Key [{2}] with Smtp Router Service; {3}",
                                                                (smtpConfiguration.QueueNumber+1),
                                                                connectionNumber,
                                                                smtpConfiguration.Key,
                                                                DateTime.Now.ToString("ddd, dd MMM yyy HH:mm:ss %K"));

                            routableMessage.Message.Headers.Add("X-SM-SentBy", receiveByString);

                            // Append File Name to the Header
                            routableMessage.Message.Headers.Add("X-SM-FileName", Path.GetFileName(routableMessage.FileName));

                            // Create folder to store sent files
                            string SentFolder = smtpConfiguration.Folders.SentFolderWithGroupingOptions(routableMessage.CreationDateTime);
                            if (!Directory.Exists(SentFolder)) Directory.CreateDirectory(SentFolder);

                            // Sends the message with the active connection
                            bool messageSent = false;

                            // Reset attempts
                            attempts = 0;

                            while ((!messageSent) && (attempts <= maxAttempts))
                            {
                                // Increment number of attempts
                                attempts++;

                                try
                                {
                                    // Ensure to create a connection
                                    if (TryConnectToSmtp(ref client, smtpConfiguration))
                                    {
                                        // Tries to send the message
                                        if (client.TrySend(routableMessage.Message, routableMessage.MailFrom, routableMessage.Recipients, out Exception eSendException ))
                                        {
                                            messageSent = true;

                                            // Move file to the sent folder
                                            File.Move(routableMessage.FileName, Path.Combine(SentFolder, Path.GetFileName(routableMessage.FileName)));

                                            // Notify that a message has been sent
                                            MessageSentSuccessfully?.Invoke(this, new MessageEventArgs(routableMessage));
                                        }
                                        else
                                        {
                                            // Notify the message has not been sent
                                            MessageNotSent?.Invoke(this, new MessageErrorEventArgs(routableMessage, eSendException));

                                            // Something happened, give it a few seconds before trying again
                                            Task.Delay(2000).Wait(cancellationToken);
                                        }

                                        // Disconnect the client
                                        if (client.IsConnected)
                                            client.Disconnect(true);
                                    }
                                    else
                                    {
                                        // Something happened, give it a few seconds before trying again
                                        Task.Delay(2000).Wait(cancellationToken);
                                    }
                                }
                                catch
                                {
                                    // Give it some time before trying again to connect on this loop
                                    Task.Delay(2000).Wait(cancellationToken);
                                }
                            }

                            // Check if message is still not sent
                            if (!messageSent)
                            {
                                if (attempts == maxAttempts)
                                    throw new MessageNotSentException(routableMessage, new Exception($"Reached the limit of attempts to send the message ({maxAttempts} times) without any success"));
                                else
                                    throw new MessageNotSentException(routableMessage, new Exception($"Unable to send the message after {attempts} attempts"));
                            }
                                
                        }
                        catch (Exception e)
                        {
                            // Move message to the error queue
                            File.Move(routableMessage.FileName, Path.Combine(smtpConfiguration.Folders.ErrorFolder, Path.GetFileName(routableMessage.FileName)));

                            // Notify that a message was not Sent
                            MessageNotSent?.Invoke(this, new MessageErrorEventArgs(routableMessage, e));
                        }
                    }
                    else
                    {
                        // Wait for a few seconds before trying again
                        Task.Delay(3000).Wait(cancellationToken);
                    }
                }
                catch (Exception e)
                {
                    // Nothing else to try, notifies system about the issue
                    GeneralError?.Invoke(this, new GeneralErrorEventArgs(new Exception($"General Error on '{nameof(SendRoutedMessages)}' method", e), nameof(SendRoutedMessages)));
                }
                finally
                {
                    // Disconnects client if needed
                    if (client != null)
                        if (client.IsConnected)
                            client.Disconnect(true);
                }
            }

            // Notify that the Smtp Connection has been closed
            SmtpConnectionEnded?.Invoke(this, new SmtpConnectionEventArgs(smtpConfiguration, connectionNumber, null));
        }

        #endregion Message Processing Methods

        #region Queue Methods

        /// <summary>
        /// Purges queues for messages older then the <see cref="MessagePurgeLifespan"/>
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        public void PurgeOldMessages(CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                // Ensure the service is not paused
                if (!IsPaused)
                {
                    try
                    {
                        // Get all files inside the Queue
                        DirectoryInfo dirInfo = new DirectoryInfo(Folders.RootFolder);
                        IEnumerable<FileInfo> files = dirInfo.EnumerateFiles("*.EML", SearchOption.AllDirectories);

                        // Purged Files Collection
                        List<FileInfo> purgedFiles = new List<FileInfo>();

                        // Date to Purge
                        DateTime purgeDate = DateTime.Now.Subtract(MessagePurgeLifespan);

                        // Retrieve all files on the queues
                        foreach (FileInfo fi in files)
                        {
                            // Refresh the FileInfo to get the proper CreationTime
                            fi.Refresh();

                            // Check Date
                            if (fi.CreationTime.CompareTo(purgeDate) < 0)
                            {
                                // Calls the Purge Event
                                PurgeFileEventArgs purgeFileEventArgs = new PurgeFileEventArgs(fi, purgeDate);
                                bool cancelPurge = false;

                                if (MessagePurging != null)
                                {
                                    MessagePurging.Invoke(this, purgeFileEventArgs);
                                    cancelPurge = purgeFileEventArgs.Cancel;
                                }
                                
                                // Purge or Keep File
                                if (!cancelPurge)
                                {
                                    // Delete the file
                                    File.Delete(fi.FullName);

                                    // Add it to collection (since the refresh method was called before, the information does not get lost)
                                    purgedFiles.Add(fi);
                                }
                            }
                        }

                        // Trigger event if handled
                        if (purgedFiles.Count > 0)
                            MessagesPurged?.Invoke(this, new PurgeFilesEventArgs(purgedFiles, purgeDate));
                    }
                    catch (Exception e)
                    {
                        // Call the GeneralError Event Handler
                        GeneralError?.Invoke(this, new GeneralErrorEventArgs(e, nameof(PurgeOldMessages)));
                    }

                    // Wait for 10 minutes before trying it again (cancellationToken forces it to exit too)
                    Task.Delay(new TimeSpan(0, 10, 0)).Wait(cancellationToken);
                }
                else
                {
                    // Wait for 10 seconds trying it again (cancellationToken forces it to exit too)
                    Task.Delay(10000).Wait(cancellationToken);
                }
            }
        }

        #endregion Queue Methods

    }
        
}
