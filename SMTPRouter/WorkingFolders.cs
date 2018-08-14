using SMTPRouter.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter
{
    /// <summary>
    /// A class peresenting the working folders of the <see cref="Router"/>
    /// </summary>
    public sealed class WorkingFolders
    {
        /// <summary>
        /// The grouping option for the files on the Sent folder
        /// </summary>
        public FileGroupingOptions GroupingOption { get; internal set; }

        /// <summary>
        /// Initializes a new instance of Working Folders
        /// </summary>
        internal WorkingFolders() : this(System.IO.Directory.GetCurrentDirectory()) { }

        /// <summary>
        /// Initializes a new instance of Working Folders
        /// </summary>
        /// <param name="rootFolder">The root folder</param>
        internal WorkingFolders(string rootFolder) : this(rootFolder, FileGroupingOptions.NoGrouping) { }

        /// <summary>
        /// Initializes a new isntance of Working Folders
        /// </summary>
        /// <param name="rootFolder">The root folder</param>
        /// <param name="fileGroupingOptions">The grouping option for sent messages</param>
        internal WorkingFolders(string rootFolder, FileGroupingOptions fileGroupingOptions)
        {
            RootFolder = rootFolder;
            GroupingOption = fileGroupingOptions;
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
        /// The folder where the messages pending to be routed are located
        /// </summary>
        public string InQueueFolder { get { return System.IO.Path.Combine(RootFolder, "InQueue"); } }
        /// <summary>
        /// The folder where all sent messages are stored
        /// </summary>
        public string SentFolder { get { return System.IO.Path.Combine(RootFolder, "Sent"); } }
        /// <summary>
        /// The folder where all messages that were not sent after the <see cref="Router.MessageLifespan"/> expires
        /// </summary>
        public string ErrorFolder { get { return System.IO.Path.Combine(RootFolder, "Error"); } }
        /// <summary>
        /// The folder where all rejected messages are stored
        /// </summary>
        public string RejectedFolder { get { return System.IO.Path.Combine(RootFolder, "Rejected"); } }

        /// <summary>
        /// Returns the folder where the messages sent should be stored, considering the file grouping options and date time
        /// </summary>
        /// <param name="dateTime">The date/time to use on the grouping option</param>
        /// <returns>A string containing the full path where the file should be stored, considering the <see cref="FileGroupingOptions"/> defined on <see cref="GroupingOption"/></returns>
        public string SentFolderWithGroupingOptions(DateTime dateTime)
        {
            if (GroupingOption == FileGroupingOptions.GroupByDate)
                return System.IO.Path.Combine(SentFolder, dateTime.ToString("yyyy-MM-dd"));
            else if (GroupingOption == FileGroupingOptions.GroupByDateAndHour)
                return System.IO.Path.Combine(SentFolder, dateTime.ToString("yyyy-MM-dd HH") + "00");
            else
                return SentFolder;
        }
    }

}
