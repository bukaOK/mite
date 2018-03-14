using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Provider;

namespace Mite.Modules.Vk.Provider
{
    public class VkReturnEndpointContext : ReturnEndpointContext
    {
        /// <summary>
        /// Информация о контексте для провайдеров
        /// </summary>
        /// <param name="context">OWIN environment</param>
        /// <param name="ticket">The authentication ticket</param>
        public VkReturnEndpointContext(IOwinContext context, AuthenticationTicket ticket)
            : base(context, ticket)
        {
        }
    }
}