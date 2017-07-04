using Google.Apis.Auth.OAuth2.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.Apis.Auth.OAuth2.Flows;
using System.Web.Mvc;
using Google.Apis.Auth.OAuth2;
using Mite.Constants;
using Google.Apis.AdSense.v1_4;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using Google.Apis.Util.Store;

namespace Mite.ExternalServices.Google
{
    public class AppFlowMetadata : FlowMetadata
    {
        public override string AuthCallback => "/GoogleAuth/IndexAsync";
        private readonly IAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = GoogleApiSettings.ClientId,
                ClientSecret = GoogleApiSettings.ClientSecret
            },
            Scopes = new[] { AdSenseService.Scope.AdsenseReadonly },
        });
        public override IAuthorizationCodeFlow Flow => flow;

        public override string GetUserId(Controller controller)
        {
            return controller.User.Identity.GetUserId();
        }
    }
}