using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Provider;
using Mite.ExternalServices.VkApi.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Xml;

namespace Mite.Modules.Vk.Provider
{
    public class VkAuthenticatedContext : BaseContext
    {
        /// <summary>
        /// Initializes a <see cref="VkAuthenticatedContext"/>
        /// </summary>
        /// <param name="context">The OWIN environment</param>
        /// <param name="userxml">The XML document with user info</param>
        /// <param name="accessToken">Access token</param>
        /// <param name="expires">Seconds until expiration</param>
        public VkAuthenticatedContext(IOwinContext context, User userResp, string accessToken, string expires)
            : base(context)
        {
            AccessToken = accessToken;

            if (Int32.TryParse(expires, NumberStyles.Integer, CultureInfo.InvariantCulture, out int expiresValue))
            {
                ExpiresIn = TimeSpan.FromSeconds(expiresValue);
            }

            Id = userResp.Id;
            FirstName = userResp.FirstName;
            LastName = userResp.LastName;
            UserName = userResp.Domain;
            PhotoLink = userResp.Photo_200;

        }

        /// <summary>
        /// Gets the document with user info
        /// </summary>
        public XmlDocument UserXml { get; private set; }

        /// <summary>
        /// Gets the access token
        /// </summary>
        public string AccessToken { get; private set; }

        /// <summary>
        /// Gets the access token expiration time
        /// </summary>
        public TimeSpan? ExpiresIn { get; set; }

        /// <summary>
        /// Gets the user ID
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the user's name
        /// </summary>
        public string FirstName { get; private set; }

        /// <summary>
        /// Gets the user's last name
        /// </summary>
        public string LastName { get; private set; }

        /// <summary>
        /// Gets the user's full name
        /// </summary>
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        /// <summary>
        /// Gets the user's DefaultName
        /// </summary>
        public string DefaultName
        {
            get
            {
                if (!String.IsNullOrEmpty(UserName))
                    return UserName;

                if (!String.IsNullOrEmpty(Nickname))
                    return Nickname;

                return FullName;
            }
        }

        /// <summary>
        /// Get's the user's Email
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Gets the user's picture link
        /// </summary>
        public string PhotoLink { get; private set; }

        /// <summary>
        /// Gets the username
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Gets the Nickname
        /// </summary>
        public string Nickname { get; private set; }

        /// <summary>
        /// Gets the <see cref="ClaimsIdentity"/> representing the user
        /// </summary>
        public ClaimsIdentity Identity { get; set; }

        /// <summary>
        /// Gets or sets a property bag for common authentication properties
        /// </summary>
        public AuthenticationProperties Properties { get; set; }
    }
}