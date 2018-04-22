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
    /// router.MessageRoutedSuccessfully += Server_MessageRoutedSuccessfully;
    /// router.MessageNotRouted += Server_MessageNotRouted;
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
        /// Event triggered when a general error happens on the processing
        /// </summary>
        /// <remarks>Usually general errors stop the processing so it's important to handle this event</remarks>
        public event EventHandler<GeneralErrorEventArgs> GeneralError;

        #endregion Events

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

            // Initialize Queues
            try
            {
                // Create Folders if they do not exist already
                if (!Directory.Exists(Folders.OutgoingFolder)) Directory.CreateDirectory(Folders.OutgoingFolder);
                if (!Directory.Exists(Folders.SentFolder)) Directory.CreateDirectory(Folders.SentFolder);
                if (!Directory.Exists(Folders.RetryFolder)) Directory.CreateDirectory(Folders.RetryFolder);
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
            await Task.WhenAll(Task.Run(() => ProcessOutgoingQueue(cancellationToken)),
                               Task.Run(() => ProcessRetryQueue(cancellationToken)),
                               Task.Run(() => PurgeQueues(cancellationToken))).ConfigureAwait(false);
        }

        #endregion Initialization Methods

        #region Queue Methods

        /// <summary>
        /// Processes the messages on the Outgoing Queue
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        private void ProcessOutgoingQueue(CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                // Ensure the service is not paused
                if (!IsPaused)
                {
                    // Get messages from the Outgoing Folder
                    try
                    {
                        // Get all files to process (since the number of files is usually small, it is ok to use GetFiles method)
                        DirectoryInfo dirInfo = new DirectoryInfo(Folders.OutgoingFolder);
                        IEnumerable<FileInfo> files = dirInfo.EnumerateFiles("*.EML");
                        
                        // Process each file on the queue, but sort it by creation time
                        foreach (FileInfo fi in (from f in files orderby f.CreationTime select f))
                        {
                            MimeMessage message = null;

                            try
                            {
                                // Load MimeMessage
                                message = MimeMessage.Load(fi.FullName);

                                // Routes the Message
                                RouteMessage(message);

                                // Send it to the SentFolder
                                File.Move(fi.FullName, Path.Combine(Folders.SentFolder, Path.GetFileName(fi.FullName)));

                                // Throws the event to inform the message was sent
                                MessageRoutedSuccessfully?.Invoke(this, new MessageEventArgs(message));
                            }
                            catch (Exception e)
                            {
                                // Get the file information
                                if ((DateTime.Now - fi.CreationTime).TotalSeconds > MessageLifespan.TotalSeconds)
                                {
                                    // Message Expired the Lifespan, send it to the error queue
                                    File.Move(fi.FullName, Path.Combine(Folders.ErrorFolder, Path.GetFileName(fi.FullName)));
                                }
                                else
                                {
                                    // Message did not expire the Lifespan, send it to the retry queue
                                    File.Move(fi.FullName, Path.Combine(Folders.RetryFolder, Path.GetFileName(fi.FullName)));
                                }

                                // Invoke the MessageNotRouted Event
                                MessageNotRouted?.Invoke(this, new MessageErrorEventArgs(message, e));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // Call the GeneralError Event Handler
                        GeneralError?.Invoke(this, new GeneralErrorEventArgs(e, nameof(ProcessOutgoingQueue)));
                    }
                }

                // Wait 10 seconds before trying again
                Task.Delay(10000).Wait();
            }
        }

        /// <summary>
        /// Routes the Message to the proper SMTP
        /// </summary>
        /// <param name="message">A <see cref="MimeMessage"/> to be routed</param>
        private void RouteMessage(MimeMessage message)
        {
            try
            {
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

                // Ensure there is a valid MimeMessage
                if (message == null)
                    throw new Exception("The Message is not valid");

                // The rule selected for use
                RoutingRule selectedRule = null;

                // Fetch Rules sorted by the Execution Sequence
                foreach (RoutingRule rule in (from r in RoutingRules orderby r.ExecutionSequence select r))
                {
                    // Try the rule
                    if (rule.Match(message))
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

                // Connect to the SMTP
                SmtpClient client = new SmtpClient();

                client.Connect(smtpConfiguration.Host,
                               smtpConfiguration.Port,
                               smtpConfiguration.UseSSL ? MailKit.Security.SecureSocketOptions.SslOnConnect :
                                                          MailKit.Security.SecureSocketOptions.Auto);

                if (smtpConfiguration.RequiresAuthentication)
                {
                    client.Authenticate(smtpConfiguration.User,
                                        smtpConfiguration.Password);
                }

                // Sends the MimeMessage thru the SMTP
                client.Send(message);

                // Quit session
                client.Disconnect(true);
            }
            catch (Exception e)
            {
                // The system could not route the message
                throw new MessageNotSentException(message, e);
            }
        }

        /// <summary>
        /// Processes the messages on the Retry Queue
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        private void ProcessRetryQueue(CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                // Ensure the service is not paused
                if (!IsPaused)
                {
                    // Get messages from the RetryFolder
                    try
                    {
                        // Get all files to process (since the number of files is usually small, it is ok to use GetFiles method)
                        string[] filesToProcess = Directory.GetFiles(Folders.RetryFolder, "*.EML");
                        Array.Sort<string>(filesToProcess);

                        // Process each file on the queue
                        foreach (string file in filesToProcess)
                        {
                            // Move file to the Outgoing Queue
                            File.Move(file, Path.Combine(Folders.OutgoingFolder, Path.GetFileName(file)));
                        }
                    }
                    catch (Exception e)
                    {
                        // Call the GeneralError Event Handler
                        GeneralError?.Invoke(this, new GeneralErrorEventArgs(e, nameof(ProcessRetryQueue)));
                    }
                }

                // Wait 2 minutes before trying again
                Task.Delay(120000).Wait();
            }
        }

        /// <summary>
        /// Adds a message to the proper queue
        /// </summary>
        /// <param name="message"></param>
        public void Enqueue(MimeMessage message)
        {
            // Defines the file name
            string messageFilename = string.Format("{0}-{1}.EML", DateTime.Now.ToString("yyyyMMddHHmmss"), Guid.NewGuid().ToString());

            try
            {
                // Defines the format of the file
                FormatOptions dosLineFormat = new FormatOptions()
                {
                    NewLineFormat = NewLineFormat.Dos,
                };

                // Writes it to a text file
                message.WriteTo(dosLineFormat, Path.Combine(Folders.OutgoingFolder, messageFilename));
            }
            catch (Exception e)
            {
                throw new Exception("Unable to Enqueue message", e);
            }
        }

        /// <summary>
        /// Purges queues for messages older then the <see cref="MessagePurgeLifespan"/>
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        public void PurgeQueues(CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                try
                {
                    // Get all files inside the Queue
                    DirectoryInfo dirInfo = new DirectoryInfo(Folders.RootFolder);
                    IEnumerable<FileInfo> files = dirInfo.EnumerateFiles("*.EML", SearchOption.AllDirectories);

                    // Retrieve all files older than the MessagePurgeLifespan 
                    foreach (FileInfo fi in (from f in files where f.CreationTime < DateTime.Now.Subtract(MessagePurgeLifespan) select f))
                    {
                        File.Delete(fi.FullName);
                    }
                }
                catch (Exception e)
                {
                    // Call the GeneralError Event Handler
                    GeneralError?.Invoke(this, new GeneralErrorEventArgs(e, nameof(PurgeQueues)));
                }

                // Wait for 10 minutes before trying it again
                Task.Delay(new TimeSpan(0, 10, 0)).Wait();
            }
        }

        #endregion Queue Methods

        #region Setup Data Load Methods

        /// <summary>
        /// Load Routing Rules from a Text File
        /// </summary>
        /// <param name="filename">The file name</param>
        /// <param name="appendToCurrentRules">A flag to define whether to append to the existing <see cref="RoutingRules"/> or to replace the existing rules</param>
        /// <returns>A <see cref="bool"/> to inform if the rules were loaded</returns>
        public bool LoadRoutingRules(string filename, bool appendToCurrentRules = false)
        {
            //TODO: Implement this method
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves the existing routing rules to a text file
        /// </summary>
        /// <param name="filename">The file name</param>
        public void SaveRoutingRules(string filename)
        {
            //TODO: Implement this method
            throw new NotImplementedException();
        }

        /// <summary>
        /// Load Smtp Configuration from a Text File
        /// </summary>
        /// <param name="filename">The file name</param>
        /// <param name="appendToCurrentConfiguration">A flag to define whether to append to the existing <see cref="DestinationSmtps"/> or to replace the existing Smtps</param>
        /// <returns></returns>
        public bool LoadSmtpConfiguration(string filename, bool appendToCurrentConfiguration = false)
        {
            //TODO: Implement this method
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves the existing Smtp Configuration to a Text File
        /// </summary>
        /// <param name="filename">The file name</param>
        public void SaveSmtpConfiguration(string filename)
        {
            //TODO: Implement this method
            throw new NotImplementedException();
        }

        #endregion Setup Data Load Methods

    }

    #region Auxiliary Classes

    /// <summary>
    /// A class peresenting the working folders of the <see cref="Router"/>
    /// </summary>
    public sealed class WorkingFolders
    {
        /// <summary>
        /// Initializes a new instance of Working Folders
        /// </summary>
        internal WorkingFolders(): this(System.IO.Directory.GetCurrentDirectory()) { }

        /// <summary>
        /// Initializes a new instance of Working Folders
        /// </summary>
        /// <param name="rootFolder">The root folder</param>
        internal WorkingFolders(string rootFolder)
        {
            RootFolder = rootFolder;
        }

        /// <summary>
        /// The root folder
        /// </summary>
        public string RootFolder { get; internal set; }
        /// <summary>
        /// The folder where the pending messages should be sent to
        /// </summary>
        public string OutgoingFolder { get { return System.IO.Path.Combine(RootFolder, "Outgoing"); } }
        /// <summary>
        /// The folder where all sent messages are stored
        /// </summary>
        public string SentFolder { get { return System.IO.Path.Combine(RootFolder, "Sent"); } }
        /// <summary>
        /// The folder where all messages to resend are stored
        /// </summary>
        public string RetryFolder { get { return System.IO.Path.Combine(RootFolder, "Retry"); } }
        /// <summary>
        /// The folder where all messages that were not sent after the <see cref="Router.MessageLifespan"/> expires
        /// </summary>
        public string ErrorFolder { get { return System.IO.Path.Combine(RootFolder, "Error"); } }
    }

    #endregion Auxiliary Classes
}
