using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SMTPRouter
{
    /// <summary>
    /// Event Arguments for the event of purging a file.
    /// </summary>
    /// <remarks>This event is triggered before the file is purged. Users can control the behavior by changing the value of the <see cref="Cancel"/> property to true to suppress the cancellation</remarks>
    public class PurgeFileEventArgs: EventArgs
    {
        /// <summary>
        /// The date for the purge. Typically everything before the <see cref="PurgeDate"/> can be purged
        /// </summary>
        public DateTime PurgeDate { get; private set; }

        /// <summary>
        /// The file being purged
        /// </summary>
        public FileInfo File { get; private set; }

        /// <summary>
        /// Defines whether to cancel the event or not. By default, this value is false
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PurgeFileEventArgs"/>
        /// </summary>
        /// <param name="file">The file being purged</param>
        /// <param name="purgeDate">The date for the purge</param>
        public PurgeFileEventArgs(FileInfo file, DateTime purgeDate)
        {
            this.File = file;
            this.PurgeDate = purgeDate;
            this.Cancel = false;
        }
    }
}
