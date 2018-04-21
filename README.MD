# Smtp Router
[![nuget](https://img.shields.io/nuget/v/SmtpRouter.svg)](https://www.nuget.org/packages/SmtpRouter/) 
![GitHub release](https://img.shields.io/github/release/diassoft/SmtpRouter.svg)
![NuGet](https://img.shields.io/nuget/dt/SmtpRouter.svg)

The SMTP Router is a intermediate SMTP server useful to intercept messages and route it to another smtp.

It contains a `Listener` to capture Smtp messages and a `Router` that will route the message to another Smtp Server when it matches certain `RoutingRules`.

These are some of the uses of an SMTP Router:
* Flexible SMTP Relay Server
* Switch the destination SMTP server based on the incoming message. This is especially useful when you have systems that can only accept one single SMTP configuration and you want to use more than one SMTP.
* Use Multiple SMTPs to send messages, due to daily our hourly limits defined by the email provider (*in development*)

## How To Use It

There are two different ways to use the SMTP Router. 

* You can initialize a `SmtpRouter.Server` object or;
* You can initialize a `Listener` and a `Router` manually.

The `Listener` opens a SmtpServer and listens to messages. Once a messages arrives, it will raise the `MessageReceived` event, with the `MimeKit.MimeMessage` received.

The `Router` runs in a separated thread from the `Listener`. The `Router` itself is the component which checks the `RoutingRules` and routes the message to the proper SMTP.

The `Router` creates a folder structure containing the following queues:

| Queue | Description | 
| :--- | :--- |
| *Outgoing* | Emails that need to be routed |
| *Sent* | Emails that were routed successfully |
| *Retry* | Emails that were not sent yet, but are still under the `MessageLifespan`. Those messages are sent back to the *Outgoing* queue.|
| *Error* | Emails that were not sent and the `MessageLifespan` is expired. Those messages are no longer sent unless they are moved manually to the *Outgoing* queue.|


### Prerequisites

When you choose to use the SmtpRouter Nuget Package, you will also be required to install the following packages:

| Package | Nuget Link | Author |
| :--- | ---------- | :--- |
| SmtpServer| [![nuget](https://img.shields.io/nuget/v/SmtpServer.svg)](https://www.nuget.org/packages/SmtpServer/) | cosullivan |
| MimeKit | [![nuget](https://img.shields.io/nuget/v/MimeKit.svg)](https://www.nuget.org/packages/MimeKit/) | jstedfast |
| MailKit | [![nuget](https://img.shields.io/nuget/v/MailKit.svg)](https://www.nuget.org/packages/MailKit/) | jstedfast |


### Using the Server Object

The **easiest and recommended** way to implement the SmtpRouter is by initializing an instance of the `Server` class and call the `StartAsync` to start listening to messages route them.

The code below demonstrates how to instantiate a server:
```cs
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

// Initialize Services
Task.WhenAll(server.StartAsync(CancellationToken.None)).ConfigureAwait(false);
```
The `Server` class triggers events when certain activities happen. You can hook to the events by using the code below:

```cs
// Hook Events
server.SessionCreated += Server_SessionCreated;
server.SessionCommandExecuting += Server_SessionCommandExecuting;
server.SessionCompleted += Server_SessionCompleted;
server.ListeningStarted += Server_ListeningStarted;
server.MessageReceived += Server_MessageReceived;
server.MessageRoutedSuccessfully += Server_MessageRoutedSuccessfully;
server.MessageNotRouted += Server_MessageNotRouted;
```

### Using the Listener and Router Individually

It is not necessary to instantiate the `Listener` and `Router` manually, but if you need to have more control, you can do it.

If you want to have the `Router` run in a different Service Instance, perhaps having both objects initialized separately would be the best option.

The code below demonstrates how to instantiate a `Listener`, a `Router` and hook them together using `events`.

```cs
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

// When the Listener have a message, it has to be sent to the Router. This is Automatic when you use the Server class
listener.MessageReceived += (object sender, MessageEventArgs e) =>
{
    // Make sure to enqueue the message otherwise the router doesn't know about anything
    router.Enqueue(e.MimeMessage);
};
listener.MessageReceived += Server_MessageReceived;

// Initialize Services
Task.WhenAll(listener.StartAsync(CancellationToken.None),
             router.StartAsync(CancellationToken.None)).ConfigureAwait(false);
```

## Smtp Configuration

The `Router` has a property named `DestinationSmtps`, which represents a `Dictionary<string, SmtpConfiguration>`. 

The Smtps to route messages must be cofigured and added to this collection. The `Key` field is the key to fetch the Smtp.

You can configure the Smtps when initializing the `Server` or `Router`, or you can configure a Smtp using the code below:

```cs
// Initializes a new Smtp Configuration
var smtpConfiguration = new SmtpConfiguration()
{
    Key = "Gmail",
    Description = "Gmail SMTP",
    Host = "smtp.gmail.com",
    Port = 587,
    RequiresAuthentication = true,
    UseSSL = false,
    User = "user@gmail.com",
    Password = "password"
};
```

## Routing Rules

The `Router` has a property named `RoutingRules`, which represents a `List<RoutingRule>`.

A `RoutingRule` is a Rule that can be applied and, if it matches, the Smtp Configuration for that rule will be used to route the email.

The `RoutingRule` class itself does not provide any rule, instead, it is a `base class` that must be inherited from in order to create an actual rule. 

By default the SmtpRouter offers the `MailFromDomainRoutingRule`, which is a rule that verifies if the **domain** on the **mail from** matches the domain configured on the rule. Other `RoutingRules` are to be developed.

The code below demonstrates how to add a `MailFromDomainRoutingRule` to the `Router` object: 
```cs
Router.RoutingRules.Add(new MailFromDomainRoutingRule(10, "mydomain.com", "mydomain"));
```

You can create your own Routing Rules by inheriting from `RoutingRule` class, as demonstrated on the code below:

```cs
public sealed class MyCustomRoutingRule: RoutingRule
{
    public MyCustomRoutingRule(): this(0)
    {
            
    }

    public MyCustomRoutingRule(int executionSequence, string smtpConfigurationKey): base(executionSequence)
    {
        base.SmtpConfigurationKey = smtpConfigurationKey;
    }

    public override bool Match(MimeMessage mimeMessage)
    {
        return true;
    }
}
```

You have to override the method `Match` to provide a algorithm that validades the rule.