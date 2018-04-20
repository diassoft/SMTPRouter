using SMTPRouter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SMTPRouter
{
    /// <summary>
    /// A class representing a Message Router.
    /// </summary>
    /// <remarks>The router receives the message and then routes it</remarks>
    public sealed partial class Router
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
        public Dictionary<string, SmtpConfiguration> SmtpConfiguration { get; set; }

        /// <summary>
        /// List of Rules to be applied when routing messages
        /// </summary>
        public List<RoutingRule> RoutingRules { get; set; }

        /// <summary>
        /// A structure representing the queue folders
        /// </summary>
        public WorkingFolders Folders { get; set; }

        /// <summary>
        /// The maximum attempts to deliver a message
        /// </summary>
        public int MaximumDeliveryAttempt { get; set; }

        /// <summary>
        /// The number of minutes the message must be on the queue in order to be sent back for processing
        /// </summary>
        public int MinutesOnQueueForRetry { get; set; }

        #endregion Properties

        #region Events

        /// <summary>
        /// Event triggered when a message is routed successfully
        /// </summary>
        public event EventHandler<EventArgs> MessageRoutedSuccessfully;

        /// <summary>
        /// Event triggered when a message could not be routed successfully
        /// </summary>
        public event EventHandler<EventArgs> MessageNotRouted;

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

            // Set Default Values
            MaximumDeliveryAttempt = 3;
            MinutesOnQueueForRetry = 2;

            // Set the folder structure
            Folders = new WorkingFolders(queuePath);

            // Initialize Queues
            try
            {
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
            await Task.WhenAll(Task.Run(() => Task.Delay(2000).Wait()));
        }

        #endregion Initialization Methods

        #region Queue Processing Methods

        //TODO: continue from here

        #endregion Queue Processing Methods
    }

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
        /// The folder where all messages that were not sent after the <see cref="Router.MaximumDeliveryAttempt"/> was reached
        /// </summary>
        public string ErrorFolder { get { return System.IO.Path.Combine(RootFolder, "Error"); } }
    }
}
