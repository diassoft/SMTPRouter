using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
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
            Console.WriteLine("Test SMTPRouter");
            Console.WriteLine("===================================================================================");
            Console.WriteLine();
            Console.WriteLine("1 - Test Using a Server Component");
            Console.WriteLine("2 - Test Using Listener and Router Individually");
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

            Console.WriteLine();
            Console.WriteLine("Press any key to shutdown...");
            Console.ReadLine();

        }

        static void TestServer()
        {
            // Creates the Server
            var server = new SMTPRouter.Server("localhost", 25, false, false, "SMTPRouter", "C:\\SMTPRouter\\Queues")
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
                            Port = 587,
                            RequiresAuthentication = true,
                            User = "user@gmail.com",
                            Password = "",
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
                        }
                    }
                },
            };

            // Hook Events
            server.SessionCreated += Server_SessionCreated;
            server.SessionCommandExecuting += Server_SessionCommandExecuting;
            server.SessionCompleted += Server_SessionCompleted;
            server.ListeningStarted += Server_ListeningStarted;
            server.MessageReceived += Server_MessageReceived;
            server.MessageRoutedSuccessfully += Server_MessageRoutedSuccessfully;
            server.MessageNotRouted += Server_MessageNotRouted;

            // Initialize Services
            Task.WhenAll(server.StartAsync(CancellationToken.None)).ConfigureAwait(false);

            // Pause Routing
            //server.Router.IsPaused = true;

            // Send Emails
            SendEmail("user@gmail.com", "user@gmail.com", 1);
            SendEmail("user@hotmail.com", "user@hotmail.com", 2);
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
                            Port = 587,
                            RequiresAuthentication = true,
                            User = "user@gmail.com",
                            Password = "",
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
                router.Enqueue(e.MimeMessage);
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

        private static void Server_MessageNotRouted(object sender, MessageErrorEventArgs e)
        {
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine("**** ERROR ****: Message Not Routed!");
            Console.WriteLine($"From....: {((MailboxAddress)e.MimeMessage.From[0]).Address.ToString()}");
            Console.WriteLine($"To......: {String.Join(",", (from t in e.MimeMessage.To where t is MailboxAddress select ((MailboxAddress)t).Address.ToString()))}");
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
            Console.WriteLine($"From....: {((MailboxAddress)e.MimeMessage.From[0]).Address.ToString()}");
            Console.WriteLine($"To......: {String.Join(",", (from t in e.MimeMessage.To where t is MailboxAddress select ((MailboxAddress)t).Address.ToString()))}");
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
            Console.WriteLine($"From....: {((MailboxAddress)e.MimeMessage.From[0]).Address.ToString()}");
            Console.WriteLine($"To......: {String.Join(",", (from t in e.MimeMessage.To where t is MailboxAddress select ((MailboxAddress)t).Address.ToString()))}");
            Console.WriteLine();
            Console.WriteLine(e.MimeMessage.ToString());
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine();
        }

        private static void Server_ListeningStarted(object sender, EventArgs e)
        {
            Console.WriteLine("Listening Started...");
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
    }
}
