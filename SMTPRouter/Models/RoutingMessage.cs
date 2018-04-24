using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SMTPRouter.Models
{
    /// <summary>
    /// A class representing a <see cref="MimeKit.MimeMessage"/> to be routed
    /// </summary>
    /// <remarks></remarks>
    public sealed class RoutingMessage: Diassoft.Mvvm.ObservableObjectBase
    {
        private string _ID;
        /// <summary>
        /// The Message Unique Identifier
        /// </summary>
        public string ID
        {
            get { return _ID; }
            set { SetProperty<string>(ref _ID, value); }
        }

        private DateTime _creationDateTime;
        /// <summary>
        /// The DateTime stamp when the message was first created
        /// </summary>
        public DateTime CreationDateTime
        {
            get { return _creationDateTime; }
            set { SetProperty<DateTime>(ref _creationDateTime, value); }
        }

#if (NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET461 || NET47 || NET471)
        
        private Stream _rawMessage;
        /// <summary>
        /// A <see cref="Stream"/> containing the <see cref="RoutingMessage"/>
        /// </summary>
        public Stream RawMessage
        {
            get { return _rawMessage; }
            set { SetProperty<Stream>(ref _rawMessage, value); }
        }
#else

        private string _rawMessage;
        /// <summary>
        /// A <see cref="string"/> containing the <see cref="RoutingMessage"/>
        /// </summary>
        /// <remarks>Ensure to have the RawMessage use <see cref="NewLineFormat.Dos"/> as the new line format</remarks>
        public string RawMessage
        {
            get { return _rawMessage; }
            set { SetProperty<string>(ref _rawMessage, value); }
        }

#endif

        /// <summary>
        /// A Readonly property that converts the <see cref="RawMessage"/> into a valid <see cref="MimeMessage"/>.
        /// </summary>
        public MimeMessage Message
        {
            get
            {
                MimeMessage message;

#if (NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET461 || NET47 || NET471)

                // Load Message from Stream (.NET Framework Only)
                _rawMessage.Position = 0;
                message = MimeMessage.Load(_rawMessage);
#else
                // Creates a memory stream to be used by the MimeMessage.Load function
                using (MemoryStream ms = new MemoryStream())
                {
                    // Writes the String to a Stream
                    using (StreamWriter sw = new StreamWriter(ms))
                    {
                        sw.Write(_rawMessage);
                        sw.Flush();
                    }

                    // Loads the Data
                    ms.Position = 0;
                    message = MimeMessage.Load(ms);
                }
#endif

                // Returns the Message itself
                return message;
            }

        }

        private int _attemptCount;
        /// <summary>
        /// The number of attempts to send the message
        /// </summary>
        public int AttemptCount
        {
            get { return _attemptCount; }
            set { SetProperty<int>(ref _attemptCount, value); }
        }

        private string _label;
        /// <summary>
        /// A string containing the Label of the message. Used on the MSQM system
        /// </summary>
        public string Label
        {
            get { return _label; }
            set { SetProperty<string>(ref _label, value); }
        }

        /// <summary>
        /// Initializes a new instance of a Routing Message
        /// </summary>
        public RoutingMessage()
        {

        }

    }
}
