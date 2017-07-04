using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.ExternalServices.Google.Authorize
{
    public class Authenticator
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}