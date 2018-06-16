# Smtp Router - v2.3.0.0
[![nuget](https://img.shields.io/nuget/v/SmtpRouter.svg)](https://www.nuget.org/packages/SmtpRouter/) 
![GitHub release](https://img.shields.io/github/release/diassoft/SmtpRouter.svg)
![NuGet](https://img.shields.io/nuget/dt/SmtpRouter.svg)
![license](https://img.shields.io/github/license/diassoft/SMTPRouter.svg)

The SMTP Router is a intermediate SMTP server useful to intercept messages and route it to another smtp.

It contains a `Listener` to capture Smtp messages and a `Router` that will route the message to another Smtp Server when it matches certain `RoutingRules`.
It also contains a `Server` object that encapsulates both the `Listener` and the `Router`.

These are some of the uses of an SMTP Router:
* SMTP Relay Server
* Switch the destination SMTP server based on the incoming message. This is especially useful when you have systems that can only accept one single SMTP configuration and you want to use more than one SMTP.
* Use Multiple SMTPs to send messages, due to daily our hourly limits defined by the email provider (*in development*)

This is a new release of the Smtp Router component, which is now on it's **major version 2**. Version 2 include the following features:

* Improved performance enhancement by using multiple Smtp Connections (Several Improvements)
* Better folder structure organization
* File grouping when saving data in the Sent Folder
* Removal of the **Retry** folder
* Fix problem when the Smtp Envelope does not match the Mime Message headers

>Implementations using previous versions will need to be modified when moving to **Major Version 2**.

>If you wish to contact the creator of this component, you can make it thru the [Nuget.org](https://www.nuget.org/packages/SmtpRouter/) page or by email [olavodias@gmail.com](mailto:olavodias@gmail.com).

## In this repository

* [SMTP Router Documentation](https://diassoft.github.io/SMTPRouter_v2000)
* [How To Use It](#how-to-use-it)
    * [Prerequisites](#prerequisites)
    * [Using the Server object](#using-the-server-object)
    * [Using the Listener and Router individually](#using-the-listener-and-router-individually)
    * [Using the Server object](#using-the-server-object)
* [Smtp Configuration](#smtp-configuration)
* [Routing Rules](#routing-rules)

## Additional Repositories

There are other interesting repositories regarding the SMTPRouter Project.

| Repository | Description | 
| :-- | :-- | 
| [SMTPRouter.Windows](https://github.com/diassoft/SMTPRouter.Windows) | Contains types to allow the use of the SMTPRouter Component in a .NET Framework. It also contains an implementation of the SMTPRouter as a Windows Service |

*We are currently working on a solution to have it run in other OSs*

## How To Use It

There are two different ways to use the SMTP Router. 

* You can initialize a `SmtpRouter.Server` object or;
* You can initialize a `Listener` and a `Router` manually.

The `Listener` opens a SmtpServer and listens to messages. Once a messages arrives, it will raise the `MessageReceived` event, with the `MimeKit.MimeMessage` received.

The `Router` runs in a separated thread from the `Listener`. The `Router` itself is the component which checks the `RoutingRules` and routes the message to the proper SMTP.

The `Router` creates a folder structure containing the following queues:

| Queue | Description | 
| :--- | :--- |
| *Outgoing* | Emails that need to be enqueued |
| *InQueue* | Emails that are queued to be routed |
| *Error* | Emails that were not queued. Those messages are no longer sent unless they are moved manually to the *Outgoing* queue.|

Each `SmtpConfiguration` will have its own set of folders, following the structure below:

| Queue | Description | 
| :--- | :--- |
| *InQueue* | Routed emails waiting to be sent by any of the available Smtp Connections |
| *Error* | Emails that were not sent. Those messages are no longer sent unless they are moved manually to the *Outgoing* queue at the Root level. |
| *Sent* | Emails that were sent. By setting the `GroupingOption` parameter of the `SmtpConfiguration` object, you can define how the files will be grouped inside the `Sent` folder. |

>Previous versions had a `Retry` queue. This is no longer available since it did not seem to be necessary to keep trying to send the same message many times.

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
    MessagePurgeLifespan = new TimeSpan(90, 0, 0, 0),
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
                SecureSocketOption = 1,
                ActiveConnections = 2,
                GroupingOption = FileGroupingOptions.GroupByDateAndHour,
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
                GroupingOption = FileGroupingOptions.GroupByDateAndHour,
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
server.MessagePurging += Server_MessagePurging;
server.MessagesPurged += Server_MessagesPurged;
server.MessageNotSent += Server_MessageNotSent;_
```

For more information, refer to the [Server Class](https://diassoft.github.io/SMTPRouter_v2000/html/T_SMTPRouter_Server.htm) in the documentation.

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

For more information, refer to the [Listener Class](https://diassoft.github.io/SMTPRouter_v2000/html/T_SMTPRouter_Listener.htm) and the [Router Class](https://diassoft.github.io/SMTPRouter_v2000/html/T_SMTPRouter_Router.htm]) in the documentation.

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
    Password = "password",
    SecureSocketOption = 1,
    ActiveConnections = 2,
    GroupingOption = FileGroupingOptions.GroupByDateAndHour
};
```

The `SmtpConfiguration` object derives from an `ObservableObject`, which makes it compatible with **Mvvm Implementations**.

Valid values for the `SecureSocketOption` are:

| Value | Description | Usage |
| :--- | :--- | :--- |
| `0` | `None` | No SSL or TLS encryption should be used |
| `1` | `Auto` | The system will decide whether to use SSL or TLS |
| `2` | `SslOnConnect` | The connection should use SSL or TLS encryption immediately |
| `3` | `StartTls` | Elevates the connection to use TLS encryption immediatelly after reading the greeting a server capabilities |
| `4` | `StartTlsWhenAvailable` | Elevates the connection to use TLS encryption immediatelly after reading the greeting a server capabilities, but only if the server supports that |

When you set the `UseSSL` property to `true`, the system changes the port to `465` and the `SecureSocketOption` to `SslOnConnect`.

In order to improve the performance, you can define how many Smtp Connections will be created by setting the property `ActiveConnections`. If you leave it `0`, the system will create only `1` connection.

When messages are sent, you can define `Sent` folder will have sub-folders per Date or per Date and Hour. Valid values for `GroupingOption` are:

| Value | Description | Usage |
| :--- | :--- | :--- |
| `0` | `NoGrouping` | Sent files will be copied to the Sent Folder |
| `1` | `GroupByDate` | Sent files will be organized by Date on the Sent folder |
| `2` | `GroupByDateAndHour` | Sent files will be organized by Date and Hour on the Sent folder |

For more information, refer to the [SmtpConfiguration](https://diassoft.github.io/SMTPRouter_v2000/html/T_SMTPRouter_Models_SmtpConfiguration.htm) in the documentation.

## Routing Rules

The `Router` has a property named `RoutingRules`, which represents a `List<RoutingRule>`.

A `RoutingRule` is a Rule that can be applied and, if it matches, the Smtp Configuration for that rule will be used to route the email.

The `RoutingRule` class itself does not provide any rule, instead, it is a `base class` that must be inherited from in order to create an actual rule. 

By default the SmtpRouter offers the following Routing Rules:

| Routing Rule | Description |
| :--- | :--- |
| `MailFromDomainRoutingRule` | A rule that verifies if the **domain** on the **mail from** matches the domain configured on the rule. |
| `MailFromRegexMatchRoutingRule` | A a rule that runs a `System.Text.RegularExpressions` to validate the **mail from**. |
| `RelayRoutingRule` | A a rule always returns **true**. Use it when you need an **SMTP Relay** that routes messages just one single server. You can modify the email sender by setting the `ReplaceMailFromAddress` to another email address. |

The code below demonstrates how to add a `MailFromDomainRoutingRule` to the `Router` object: 
```cs
Router.RoutingRules.Add(new MailFromDomainRoutingRule(10, "mydomain.com", "mydomain"));
```

There is a static method in the `RoutingRule` class that allows to create a `RoutingRule` dynamically:

```cs
var ruleGmail = Models.RoutingRule.CreateRule(10, 
                                              "SMTPRouter.Models.MailFromDomainRoutingRule, SMTPRouter", 
                                              "Gmail", 
                                              "Domain=gmail.com");

var ruleRegex = Models.RoutingRule.CreateRule(20, 
                                              "SMTPRouter.Models.MailFromRegexMatchRoutingRule, SMTPRouter", 
                                              "Regex", 
                                              "RegexExpression=\d");

var ruleRelay = Models.RoutingRule.CreateRule(30, 
                                              "SMTPRouter.Models.RelayRoutingRule, SMTPRouter", 
                                              "Relay");
```

You can define your own Routing Rules by inheriting from `RoutingRule` class, as demonstrated on the code below:

```cs
public sealed class MyCustomRoutingRule: RoutingRule
{
    // The Default Constructor
    //   you always have to provide a parameterless constructor for a RoutingRule
    //   otherwise the System.Reflection will not be able to instantiate it
    public MyCustomRoutingRule(): this(0, "")
    {
            
    }

    public MyCustomRoutingRule(int executionSequence, string smtpConfigurationKey): base(executionSequence)
    {
        base.SmtpConfigurationKey = smtpConfigurationKey;
    }

    public override bool Match(RoutableMessage routableMessage)
    {
        return true;
    }
}
```

You have to override the method `Match` to provide a algorithm that validades the rule.

The `RoutingRule` object derives from an `ObservableObject`, which makes it compatible with **Mvvm Implementations**.

For more information, refer to the [RoutingRule](https://diassoft.github.io/SMTPRouter_v2000/html/T_SMTPRouter_Models_RoutingRule.htm) in the documentation.
