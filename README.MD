# SMTP Router
An implementation of a <code>Listener</code> to capture Smtp messages and a <code>Router</code> that will route the message to other Smtp Server.

These are some of the uses of an SMTP Router:
* Flexible SMTP Relay Server
* Switch the destination SMTP server based on the incoming message. This is especially useful when you have systems that can only accept one single SMTP configuration and you want to use more than one SMTP.

<b><span style="color: red;">Currently this is a Development version yet, so please do not use it until this message goes away!!</span></b>

# How To Use it
You need to initialize an instance of the <code>Listener</code> class to start listening to smtp messages in a specific port.

Once you receive messages, you can process them by hooking to the event <code>MessageReceived</code>, raised by the <code>Listener</code> class.
```cs
var smtpListener = new SmtpRouter.Listener();
```
