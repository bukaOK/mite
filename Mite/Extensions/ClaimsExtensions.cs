using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Owin.Security;

namespace Mite.Extensions
{
    public static class ClaimsExtensions
    {
        public static void AddUpdateClaim(this IPrincipal currentPrincipal, IAuthenticationManager authManager, string key, string value)
        {
            var identity = currentPrincipal.Identity as ClaimsIdentity;
            if(identity == null)
                return;

            var existingClaim = identity.FindFirst(key);
            if(existingClaim != null)
                identity.RemoveClaim(existingClaim);

            identity.AddClaim(new Claim(key, value));

            authManager.AuthenticationResponseGrant = new AuthenticationResponseGrant(identity,
                new AuthenticationProperties {IsPersistent = true});
        }

        public static string GetClaimValue(this IPrincipal currentPrincipal, string key)
        {
            var identity = (ClaimsIdentity) currentPrincipal.Identity;

            var claim = identity.FindFirst(x => x.Type == key);
            return claim == null ? "" : claim.Value;
        }
    }
}