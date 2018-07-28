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
    /// <remarks>This object encapsulates the message, sender and recipients</remarks>
    public sealed class RoutableMessage: Diassoft.Mvvm.ObservableObjectBase
    {
        // Constants for the SmtpRouter Header Tag
        internal const string SMTPROUTER_HEADER = "SmtpRouter-Header";
        internal const string SMTPROUTER_HEADER_BEGIN = "SmtpRouter-Header-Begin";
        internal const string SMTPROUTER_HEADER_VERSION = "SmtpRouter-Header-Version";
        internal const string SMTPROUTER_HEADER_FROM = "SmtpRouter-Header-From";
        internal const string SMTPROUTER_HEADER_TO = "SmtpRouter-Header-To";
        internal const string SMTPROUTER_HEADER_END = "SmtpRouter-Header-End";
        internal const string SMTPROUTER_HEADER_CREATIONTIME = "SmtpRouter-Header-CreationTime";
        internal const string SMTPROUTER_VERSION = "2.0.0.0";

        /// <summary>
        /// The default format to use when serializing and/or deserializing messages from streams
        /// </summary>
        public const string SMTPROUTER_HEADER_CREATIONTIME_FORMAT = "yyyy-MM-dd_HH-mm-ss";

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

        private MimeMessage _Message;

        /// <summary>
        /// The <see cref="MimeMessage"/> to be routed
        /// </summary>
        public MimeMessage Message
        {
            get { return _Message; }
            set { SetProperty<MimeMessage>(ref _Message, value); }
        }


        private string _SmtpConfigurationKey;

        /// <summary>
        /// The Smtp Key to be used to send the email
        /// </summary>
        public string SmtpConfigurationKey
        {
            get { return _SmtpConfigurationKey; }
            set { SetProperty<string>(ref _SmtpConfigurationKey, value); }
        }

        private MailboxAddress _MailFrom;
        /// <summary>
        /// The user sending the message
        /// </summary>
        public MailboxAddress MailFrom
        {
            get { return _MailFrom; }
            set { SetProperty<MailboxAddress>(ref _MailFrom, value); }
        }

        private List<MailboxAddress> _Recipients;
        /// <summary>
        /// List of recipients of the message
        /// </summary>
        public List<MailboxAddress> Recipients
        {
            get { return _Recipients; }
            set { SetProperty<List<MailboxAddress>>(ref _Recipients, value); }
        }

        private string _FileName;
        /// <summary>
        /// The Message File Name (containing the full path)
        /// </summary>
        public string FileName
        {
            get { return _FileName; }
            set { SetProperty<string>(ref _FileName, value); }
        }

        /// <summary>
        /// Initializes a new instance of a Routing Message
        /// </summary>
        public RoutableMessage(): this(null, null, new List<MailboxAddress>(), "")
        {

        }

        /// <summary>
        /// Initializes a new instance of a Routing Message
        /// </summary>
        /// <param name="message">The <see cref="MimeMessage"/> to be sent</param>
        /// <param name="mailFrom">A <see cref="MailboxAddress"/> containing the sender address</param>
        /// <param name="recipients">A <see cref="List{T}"/> of <see cref="MailboxAddress"/> containing all the recipients of the message</param>
        /// <param name="fileName">The full path of the message file</param>
        public RoutableMessage(MimeMessage message, MailboxAddress mailFrom, List<MailboxAddress> recipients, string fileName)
        {
            _Message = message;
            _MailFrom = mailFrom;
            _Recipients = recipients;
            _FileName = fileName;
        }


        /// <summary>
        /// Saves the Routable Message to a file
        /// </summary>
        /// <remarks>It will use the file name from the property <see cref="FileName"/></remarks>
        public void SaveToFile()
        {
            SaveToFile(this.FileName);
        }

        /// <summary>
        /// Saves the Routable Message to a file
        /// </summary>
        /// <param name="fileName">The file where the message is being saved</param>
        public void SaveToFile(string fileName)
        {
            try
            {
                // Ensure all variables are informed
                if (CreationDateTime == DateTime.MinValue)
                    CreationDateTime = DateTime.Now;

                // Defines the format of the file
                FormatOptions dosLineFormat = new FormatOptions()
                {
                    NewLineFormat = NewLineFormat.Dos,                    
                };

                // Prepare File Output
                using (var fileStream = File.Create(fileName))
                {
                    // Creates a stream for the header
                    using (MemoryStream streamHeader = new MemoryStream())
                    {
                        using (StreamWriter streamHeaderWriter = new StreamWriter(streamHeader, Encoding.GetEncoding(28592)))
                        {
                            // Ensure to not Automatically Flush
                            streamHeaderWriter.AutoFlush = false;

                            // Writes the SmtpRouter Header
                            streamHeaderWriter.WriteLine(SMTPROUTER_HEADER_BEGIN);
                            streamHeaderWriter.WriteLine($"{SMTPROUTER_HEADER_VERSION}: {SMTPROUTER_VERSION}");
                            streamHeaderWriter.WriteLine($"{SMTPROUTER_HEADER_CREATIONTIME}: {CreationDateTime.ToString(SMTPROUTER_HEADER_CREATIONTIME_FORMAT)}");
                            streamHeaderWriter.WriteLine($"{SMTPROUTER_HEADER_FROM}: {MailFrom.Address.ToString()}");

                            foreach (var mailTo in Recipients)
                                streamHeaderWriter.WriteLine($"{SMTPROUTER_HEADER_TO}: {mailTo.Address.ToString()}");

                            streamHeaderWriter.WriteLine(SMTPROUTER_HEADER_END);

                            // Flushes to the file
                            streamHeaderWriter.Flush();

                            // Copy Header to file
                            streamHeader.Seek(0, SeekOrigin.Begin);
                            streamHeader.CopyTo(fileStream);
                        }
                    }

                    // Creates a stream for the Message
                    using (MemoryStream streamMessage = new MemoryStream())
                    {
                        Message.WriteTo(dosLineFormat, streamMessage);

                        streamMessage.Seek(0, SeekOrigin.Begin);
                        streamMessage.CopyTo(fileStream);
                    }

                    // Flush it to the file
                    fileStream.Flush();
                }
            }
            catch (Exception e)
            {
                throw new MessageNotQueuedException(this, e);
            }

        }

        /// <summary>
        /// Loads the Routable Message based on a file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static RoutableMessage LoadFromFile(string fileName)
        {
            // The Routable Message
            RoutableMessage routableMessage = new RoutableMessage();
            routableMessage.FileName = fileName;

            try
            {
                // Creates a Stream with the remaining values
                MemoryStream messageStream = new MemoryStream();

                // Read File
                using (FileStream fileStream = File.OpenRead(fileName))
                {
                    // Text File Line
                    string line = "";

                    // Reader for the Text File
                    using (StreamReader fileStreamReader = new StreamReader(fileStream, Encoding.GetEncoding(28592)))
                    {
                        // Writer for the Message Stream
                        using (StreamWriter messageStreamWriter = new StreamWriter(messageStream, Encoding.GetEncoding(28592)))
                        {
                            // Flag to define whether the system is reading the header or not anymore
                            bool inHeader = true;

                            // Ensure the stream will have data
                            messageStreamWriter.AutoFlush = true;

                            while ((!fileStreamReader.EndOfStream) && (inHeader))
                            {
                                line = fileStreamReader.ReadLine();

                                if (line.StartsWith(SMTPROUTER_HEADER))
                                {
                                    if ((!line.StartsWith(SMTPROUTER_HEADER_BEGIN)) && (!line.StartsWith(SMTPROUTER_HEADER_END)))
                                    {
                                        // Array with the contents
                                        string[] tempHeader = line.Split(':');
                                        if (tempHeader.Length == 1)
                                            throw new Exception($"Invalid Header: {line}");

                                        // Remove any special character from the string
                                        tempHeader[1] = SMTPRouter.Utils.RemoveSpecialCharacters(tempHeader[1]);

                                        // Check which type of header line is being processed
                                        if ((line.StartsWith(SMTPROUTER_HEADER_FROM)) || (line.StartsWith(SMTPROUTER_HEADER_TO)))
                                        {
                                            // Try to parse the address, if valid then add it to the list
                                            if (MailboxAddress.TryParse(tempHeader[1], out MailboxAddress _newMailboxAddress))
                                            {
                                                if (line.StartsWith(SMTPROUTER_HEADER_FROM))
                                                    routableMessage.MailFrom = _newMailboxAddress;
                                                else if (line.StartsWith(SMTPROUTER_HEADER_TO))
                                                    routableMessage.Recipients.Add(_newMailboxAddress);
                                            }
                                            else
                                            {
                                                throw new Exception($"Unable to parse '{tempHeader[1]}' to a valid email address");
                                            }
                                        }
                                        else if (line.StartsWith(SMTPROUTER_HEADER_CREATIONTIME))
                                        {
                                            // Load the creation time
                                            if (DateTime.TryParseExact(tempHeader[1], SMTPROUTER_HEADER_CREATIONTIME_FORMAT, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime dateTime))
                                                routableMessage.CreationDateTime = dateTime;
                                            else
                                                routableMessage.CreationDateTime = DateTime.Now;
                                        }
                                    }
                                    else if (line.StartsWith(SMTPROUTER_HEADER_END))
                                    {
                                        // No longer in header
                                        inHeader = false;
                                    }
                                }
                            }

                            // Add remaining lines
                            messageStreamWriter.Write(fileStreamReader.ReadToEnd());

                            // Load Message
                            messageStream.Position = 0;
                            routableMessage.Message = MimeMessage.Load(messageStream);
                        }
                    }
                }

                // Returns the Routable Message
                return routableMessage;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }

}
