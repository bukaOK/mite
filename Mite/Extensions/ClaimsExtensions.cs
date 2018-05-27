using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Owin.Security;

namespace Mite.Extensions
{
    public static class ClaimsExtensions
    {
        public static void AddUpdateClaim(this IIdentity currentIdentity, IAuthenticationManager authManager, string key, string value)
        {
            if (!(currentIdentity is ClaimsIdentity identity))
                return;

            var existingClaim = identity.FindFirst(key);
            if(existingClaim != null)
                identity.RemoveClaim(existingClaim);

            identity.AddClaim(new Claim(key, value));

            authManager.AuthenticationResponseGrant = new AuthenticationResponseGrant(identity,
                new AuthenticationProperties {IsPersistent = true});
        }

        public static string GetClaimValue(this IIdentity currentIdentity, string key)
        {
            var identity = (ClaimsIdentity) currentIdentity;

            var claim = identity.FindFirst(x => x.Type == key);
            return claim?.Value;
        }
    }
}