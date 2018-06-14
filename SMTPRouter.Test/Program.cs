﻿using MailKit.Net.Smtp;
using MimeKit;
using SMTPRouter.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SMTPRouter.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            // Decide what to test
            Console.WriteLine("===================================================================================");
            Console.WriteLine("Test SMTPRouter (.NET Framework Version)");
            Console.WriteLine("===================================================================================");
            Console.WriteLine();
            Console.WriteLine("SMTP Routing");
            Console.WriteLine("   1 - Test Using a Server Component");
            Console.WriteLine("   2 - Test Using Listener and Router Individually");
            Console.WriteLine();
            Console.WriteLine("Other");
            Console.WriteLine("  90 - Create a Routing Rule dynamically");
            Console.WriteLine();

            Console.Write("Enter your selection --> ");

            string answer = Console.ReadLine();

            if (answer == "1")
            {
                TestServer();
            }
            else if (answer == "2")
            {
                TestIndividual();
            }
            else if (answer == "90")
            {
                CreateRuleDynamically();
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to shutdown...");
            Console.ReadLine();

        }

        private static void CreateRuleDynamically()
        {
            Console.WriteLine();
            var mailFromDomainRoutingRule = Models.RoutingRule.CreateRule(10, "SMTPRouter.Models.MailFromDomainRoutingRule, SMTPRouter", "Gmail", "Domain=gmail.com");
            Console.WriteLine(mailFromDomainRoutingRule.ToString());

            Console.WriteLine();
            var regexRoutingRule = Models.RoutingRule.CreateRule(20, "SMTPRouter.Models.MailFromRegexMatchRoutingRule, SMTPRouter", "Gmail", "RegexExpression=[U]");
            Console.WriteLine(regexRoutingRule.ToString());

            Console.WriteLine();
            var relayRoutingRule = Models.RoutingRule.CreateRule(30, "SMTPRouter.Models.RelayRoutingRule, SMTPRouter", "Gmail", "");
            Console.WriteLine(relayRoutingRule.ToString());
        }

        static void TestServer()
        {
            // Creates the Server
            var server = new SMTPRouter.Server("localhost", 25, false, false, "SMTPRouter", "C:\\SMTPRouter2\\Queues")
            {
                MessageLifespan = new TimeSpan(0, 15, 0),
                RoutingRules = new List<Models.RoutingRule>()
                {
                    new Models.MailFromDomainRoutingRule(10, "gmail.com", "gmail"),
                    new Models.MailFromDomainRoutingRule(20, "hotmail.com", "hotmail")
                },
                DestinationSmtps = new Dictionary<string, Models.SmtpConfiguration>
                {
                    { "gmail", new Models.SmtpConfiguration()
                        {
                            Key = "Gmail",
                            Host = "smtp.gmail.com",
                            Description = "Google Mail SMTP",
                            Port = 587,
                            RequiresAuthentication = true,
                            User = "user@gmail.com",
                            Password = "",
                            SecureSocketOption = 1,
                            ActiveConnections = 1,
                            GroupingOption = FileGroupingOptions.GroupByDateAndHour
                        }
                    },
                    { "hotmail", new Models.SmtpConfiguration()
                        {
                            Host = "smtp.live.com",
                            Description = "Hotmail SMTP",
                            Port = 587,
                            RequiresAuthentication = true,
                            User = "user@hotmail.com",
                            Password = "",
                            SecureSocketOption = 1,
                            ActiveConnections = 1,
                            GroupingOption = FileGroupingOptions.GroupByDateAndHour
                        }
                    }
                },
            };

            // Hook Events
            server.ListenerStarted += ((o, e) =>
            {
                server.Listener.SessionCreated += Server_SessionCreated;
                server.Listener.SessionCommandExecuting += Server_SessionCommandExecuting;
                server.Listener.SessionCompleted += Server_SessionCompleted;
                server.Listener.ListeningStarted += Server_ListeningStarted;
                server.Listener.MessageReceived += Server_MessageReceived;
            });

            server.RouterStarted += ((o, e) =>
            {
                server.Router.MessageRoutedSuccessfully += Server_MessageRoutedSuccessfully;
                server.Router.MessageNotRouted += Server_MessageNotRouted;
                server.Router.MessagePurging += Server_MessagePurging;
                server.Router.MessagesPurged += Server_MessagesPurged;
                server.Router.MessageNotSent += Server_MessageNotSent;
                server.Router.MessageSentSuccessfully += Server_MessageSentSuccessfully;
            });

            // Initialize Services
            Task.WhenAll(server.StartAsync(CancellationToken.None)).ConfigureAwait(false);

            // Pause Routing
            //server.Router.IsPaused = true;

            // Send 20 Emails
            for (int iMail = 1; iMail <= 20; iMail++)
            {
                //SendEmail("user@gmail.com", "user@gmail.com", iMail);
                //SendEmailTweakHeader("user@gmail.com", new List<string>() { "user@gmail.com" }, new List<string>() { "user@gmail.com", "user2@gmail.com" }, iMail);
                //SendEmailBcc("user@gmail.com", new List<string>() { "user@gmail.com" }, new List<string>() { "user@hotmail.com" }, iMail);
            }
            
            //SendEmail("user@gmail.com", "user@gmail.com", 1);
            //SendEmail("user@hotmail.com", "user@hotmail.com", 2);
            //SendEmail(new MailboxAddress("User Name", "user@gmail.com"), new MailboxAddress("User Name", "user@gmail.com"), 3);

            //SendEmailUsingDefaultClient("user@gmail.com", "user@gmail.com;user2@gmail.com", 10);
            //SendEmailUsingDefaultClient("user@hotmail.com", "user@hotmail.com", 10);
        }


        private static void TestIndividual()
        {
            // Create the Listener
            var listener = new SMTPRouter.Listener()
            {
                ServerName = "localhost",
                Ports = new int[] { 25, 587 },
                RequiresAuthentication = false,
                UseSSL = false
            };
            listener.SessionCreated += Server_SessionCreated;
            listener.SessionCommandExecuting += Server_SessionCommandExecuting;
            listener.SessionCompleted += Server_SessionCompleted;
            listener.ListeningStarted += Server_ListeningStarted;

            // Create the Router
            var router = new SMTPRouter.Router("SMTPRouter", "C:\\SMTPRouter\\Queues")
            {
                MessageLifespan = new TimeSpan(0, 15, 0),
                RoutingRules = new List<Models.RoutingRule>()
                {
                    new Models.MailFromDomainRoutingRule(10, "gmail.com", "gmail"),
                    new Models.MailFromDomainRoutingRule(20, "hotmail.com", "hotmail")
                },
                DestinationSmtps = new Dictionary<string, Models.SmtpConfiguration>
                {
                    { "gmail", new Models.SmtpConfiguration()
                        {
                            Host = "smtp.gmail.com",
                            Description = "Google Mail SMTP",
                            Port = 25,
                            RequiresAuthentication = true,
                            User = "user@gmail.com",
                            Password = "",
                        }
                    },
                    { "hotmail", new Models.SmtpConfiguration()
                        {
                            Host = "smtp.live.com",
                            Description = "Hotmail SMTP",
                            Port = 25,
                            RequiresAuthentication = true,
                            User = "user@hotmail.com",
                            Password = "",
                        }
                    }
                },
            };
            router.MessageRoutedSuccessfully += Server_MessageRoutedSuccessfully;
            router.MessageNotRouted += Server_MessageNotRouted;

            // Enqueue Message
            listener.MessageReceived += (object sender, MessageEventArgs e) =>
            {
                // Make sure to enqueue the message otherwise the router doesn't know about anything
                router.Enqueue(e.RoutableMessage);
            };
            listener.MessageReceived += Server_MessageReceived;

            // Initialize Services
            Task.WhenAll(listener.StartAsync(CancellationToken.None),
                         router.StartAsync(CancellationToken.None)).ConfigureAwait(false);

            // Pause Routing
            //server.Router.IsPaused = true;

            // Send Emails
            SendEmail("user@gmail.com", "user@gmail.com", 10);
            SendEmail("user@hotmail.com", "user@hotmail.com", 22);
        }

        #region Event Handlers

        private static void Server_MessagesPurged(object sender, PurgeFilesEventArgs e)
        {
            // Files Purged
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var fi in e.Files)
            {
                Console.WriteLine($"Purged.... {fi.CreationTime.ToString("yyyy-MM-dd HH:mm:ss")} ... {fi.FullName}");
            }
            Console.ResetColor();
        }

        private static void Server_MessagePurging(object sender, PurgeFileEventArgs e)
        {
            // Informs Client
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Purging file: {e.File.FullName}");
            Console.WriteLine($"   CreationTime.....: {e.File.CreationTime.ToString("yyyy-MM-dd HH:mm:ss")}");
            Console.WriteLine($"   PurgeDate........: {e.PurgeDate.ToString("yyyy-MM-dd HH:mm:ss")}");
            Console.ResetColor();
        }

        private static void Server_MessageNotRouted(object sender, MessageErrorEventArgs e)
        {
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine("**** ERROR ****: Message Not Routed!");
            Console.WriteLine($"From....: {((MailboxAddress)e.RoutableMessage.MailFrom).Address.ToString()}");
            Console.WriteLine($"To......: {String.Join(",", (from t in e.RoutableMessage.Recipients where t is MailboxAddress select ((MailboxAddress)t).Address.ToString()))}");
            Console.WriteLine();
            Console.WriteLine("Exception:");
            Console.WriteLine($"   Error..........: {e.Exception.Message}");
            Console.WriteLine($"   Stack Trace....: {e.Exception.StackTrace}");

            if (e.Exception.InnerException != null)
            {
                Console.WriteLine();
                Console.WriteLine("Inner Exception:");
                Console.WriteLine($"   Error..........: {e.Exception.InnerException.Message}");
                Console.WriteLine($"   Stack Trace....: {e.Exception.InnerException.StackTrace}");
            }

            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine();
        }

        private static void Server_MessageRoutedSuccessfully(object sender, MessageEventArgs e)
        {
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine("Message Routed Successfully!");
            Console.WriteLine($"From....: {((MailboxAddress)e.RoutableMessage.MailFrom).Address.ToString()}");
            Console.WriteLine($"To......: {String.Join(",", (from t in e.RoutableMessage.Recipients where t is MailboxAddress select ((MailboxAddress)t).Address.ToString()))}");
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine();
        }

        private static void Server_MessageSentSuccessfully(object sender, MessageEventArgs e)
        {
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine("Message Sent Successfully!");
            Console.WriteLine($"From....: {((MailboxAddress)e.RoutableMessage.MailFrom).Address.ToString()}");
            Console.WriteLine($"To......: {String.Join(",", (from t in e.RoutableMessage.Recipients where t is MailboxAddress select ((MailboxAddress)t).Address.ToString()))}");
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine();
        }

        private static void Server_SessionCompleted(object sender, SmtpServer.SessionEventArgs e)
        {
            Console.WriteLine("SMTP Session Completed");
        }

        private static void Server_SessionCommandExecuting(object sender, SmtpServer.SmtpCommandExecutingEventArgs e)
        {
            Console.WriteLine("SMTP Session Command Executing...");
            Console.WriteLine($"     {e.Command.ToString()}");
        }

        private static void Server_SessionCreated(object sender, SmtpServer.SessionEventArgs e)
        {
            Console.WriteLine("SMTP Session Completed");
        }

        private static void Server_MessageReceived(object sender, MessageEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine("A message was received!!");
            Console.WriteLine();
            Console.WriteLine($"From....: {((MailboxAddress)e.RoutableMessage.MailFrom).Address.ToString()}");
            Console.WriteLine($"To......: {String.Join(",", (from t in e.RoutableMessage.Recipients where t is MailboxAddress select ((MailboxAddress)t).Address.ToString()))}");
            Console.WriteLine();
            Console.WriteLine(e.RoutableMessage.ToString());
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine();
        }

        private static void Server_ListeningStarted(object sender, EventArgs e)
        {
            Console.WriteLine("Listening Started...");
        }

        private static void Server_MessageNotSent(object sender, MessageErrorEventArgs e)
        {
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine("**** ERROR ****: Message Not Sent!");
            Console.WriteLine($"From....: {((MailboxAddress)e.RoutableMessage.MailFrom).Address.ToString()}");
            Console.WriteLine($"To......: {String.Join(",", (from t in e.RoutableMessage.Recipients where t is MailboxAddress select ((MailboxAddress)t).Address.ToString()))}");
            Console.WriteLine();
            Console.WriteLine("Exception:");
            Console.WriteLine($"   Error..........: {e.Exception.Message}");
            Console.WriteLine($"   Stack Trace....: {e.Exception.StackTrace}");

            if (e.Exception.InnerException != null)
            {
                Console.WriteLine();
                Console.WriteLine("Inner Exception:");
                Console.WriteLine($"   Error..........: {e.Exception.InnerException.Message}");
                Console.WriteLine($"   Stack Trace....: {e.Exception.InnerException.StackTrace}");
            }

            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine();
        }

        #endregion Event Handlers

        private static void SendEmail(string from, string to, int number)
        {
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress(from));
            message.To.Add(new MailboxAddress(to));

            BodyBuilder builder = new BodyBuilder();
            builder.TextBody = $"Hey User, this is just a test; Number {number}";

            message.Subject = $"Email Routed {number}";
            message.Body = builder.ToMessageBody();

            try
            {
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Connect("localhost", 25, MailKit.Security.SecureSocketOptions.Auto);

                smtpClient.Send(message);

                smtpClient.Disconnect(true);

            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to send email to Local Smtp");
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine($"StackTrace: {e.StackTrace}");
                Console.WriteLine();
            }
        }

        private static void SendEmail(MailboxAddress from, MailboxAddress to, int number)
        {
            MimeMessage message = new MimeMessage();
            message.From.Add(from);
            message.To.Add(to);

            BodyBuilder builder = new BodyBuilder();
            builder.TextBody = $"Hey User, this is just a test; Number {number}";

            message.Subject = $"Email Routed {number}";
            message.Body = builder.ToMessageBody();

            try
            {
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Connect("localhost", 25, MailKit.Security.SecureSocketOptions.Auto);

                smtpClient.Send(message);

                smtpClient.Disconnect(true);

            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to send email to Local Smtp");
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine($"StackTrace: {e.StackTrace}");
                Console.WriteLine();
            }
        }

        private static void SendEmailBcc(string from, List<string> bccRecipients, List<string> toHeader, int number)
        {
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress(from));

            foreach (var mailTo in toHeader)
                message.To.Add(new MailboxAddress(mailTo));

            foreach (var mailTo in bccRecipients)
                message.Bcc.Add(new MailboxAddress(mailTo));

            BodyBuilder builder = new BodyBuilder();
            builder.TextBody = $"Hey User, this is just a test; Number {number}";

            message.Subject = $"Email Routed {number}";
            message.Body = builder.ToMessageBody();

            try
            {
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Connect("localhost", 25, MailKit.Security.SecureSocketOptions.Auto);

                List<MailboxAddress> mailboxAddresses = new List<MailboxAddress>();
                foreach (var mailTo in bccRecipients)
                    mailboxAddresses.Add(new MailboxAddress(mailTo));

                smtpClient.Send(message, new MailboxAddress(from), mailboxAddresses);

                smtpClient.Disconnect(true);

            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to send email to Local Smtp");
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine($"StackTrace: {e.StackTrace}");
                Console.WriteLine();
            }
        }


        private static void SendEmailTweakHeader(string from, List<string> recipients, List<string> recipientsOnHeader, int number)
        {
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress(from));

            foreach (var mailTo in recipientsOnHeader)
                message.To.Add(new MailboxAddress(mailTo));

            BodyBuilder builder = new BodyBuilder();
            builder.TextBody = $"Hey User, this is just a test; Number {number}";

            message.Subject = $"Email Routed {number}";
            message.Body = builder.ToMessageBody();

            try
            {
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Connect("localhost", 25, MailKit.Security.SecureSocketOptions.Auto);

                List<MailboxAddress> mailboxAddresses = new List<MailboxAddress>();
                foreach (var mailTo in recipients)
                    mailboxAddresses.Add(new MailboxAddress(mailTo));

                smtpClient.Send(message, new MailboxAddress(from), mailboxAddresses);

                smtpClient.Disconnect(true);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to send email to Local Smtp");
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine($"StackTrace: {e.StackTrace}");
                Console.WriteLine();
            }
        }

        private static void SendEmailTweakHeader(string from, string to, string toHeader, int number)
        {
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress(from));
            message.To.Add(new MailboxAddress(to));

            BodyBuilder builder = new BodyBuilder();
            builder.TextBody = $"Hey User, this is just a test; Number {number}";

            message.Subject = $"Email Routed {number}";
            message.Body = builder.ToMessageBody();

            try
            {
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Connect("localhost", 25, MailKit.Security.SecureSocketOptions.Auto);

                smtpClient.Send(message, new MailboxAddress(from), new List<MailboxAddress>() { new MailboxAddress(toHeader) });

                smtpClient.Disconnect(true);

            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to send email to Local Smtp");
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine($"StackTrace: {e.StackTrace}");
                Console.WriteLine();
            }
        }

        private static void SendGmail()
        {
            MimeMessage m = new MimeMessage();
            m.From.Add(new MailboxAddress("user@gmail.com"));
            m.To.Add(new MailboxAddress("user2@autoliv.com"));
            m.Subject = "Routed Mesage";

            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = "Mail To is Autoliv, but mailboxaddress is Gmail";

            m.Body = bodyBuilder.ToMessageBody();

            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Connect("smtp.gmail.com", 25, MailKit.Security.SecureSocketOptions.Auto);
            smtpClient.Authenticate("user@gmail.com", "password");

            
            List<MailboxAddress> mailboxAddresses = new List<MailboxAddress>()
            {
                new MailboxAddress("user@gmail.com"),
                new MailboxAddress("user2@jdemasters.com"),
            };

            smtpClient.Send(m, new MailboxAddress("user@gmail.com"), mailboxAddresses);

            smtpClient.Disconnect(true);

        }
    }
}
