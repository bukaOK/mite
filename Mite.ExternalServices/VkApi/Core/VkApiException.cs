using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Core
{
    public class VkApiException : Exception
    {
        public int ErrorCode { get; }
        public VkApiException(int code, string message) : base(message)
        {
            ErrorCode = code;
        }
    }
}
