using SmtpServer;
using SmtpServer.Authentication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SMTPRouter
{
    /// <summary>
    /// Class to process the Authentication of the SMTPServer
    /// </summary>
    internal class SmtpAuthenticator: UserAuthenticator
    {
        public SmtpAuthenticator()
        {

        }
        public Task<bool> AuthenticateAsync(string user, string password)
        {
            return Task.FromResult<bool>(true);
        }

        public override Task<bool> AuthenticateAsync(ISessionContext context, string user, string password, CancellationToken cancellationToken)
        {
            return Task.FromResult<bool>(true);
        }
    }
}
