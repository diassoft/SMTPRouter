using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SMTPRouter
{
    /// <summary>
    /// Represents the Event Arguments for an event that has files purged
    /// </summary>
    public class PurgeFilesEventArgs: EventArgs
    {
        /// <summary>
        /// List of <see cref="FileInfo"/> with the files on the purge process
        /// </summary>
        public IEnumerable<FileInfo> Files { get; private set; }

        /// <summary>
        /// The date for the purge. Typically everything before the <see cref="PurgeDate"/> can be purged
        /// </summary>
        public DateTime PurgeDate { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PurgeFilesEventArgs"/>
        /// </summary>
        /// <param name="files">A <see cref="IEnumerable{T}"/> containing the <see cref="FileInfo"/> for the files on the purge process</param>
        /// <param name="purgeDate">The purge date. Typically everything before this date can be purged</param>
        public PurgeFilesEventArgs(IEnumerable<FileInfo> files, DateTime purgeDate)
        {
            this.Files = files;
            this.PurgeDate = purgeDate;
        }
    }
}
