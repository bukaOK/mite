using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.DeviantArt
{
    public class DeviantArtException : Exception
    {
        public override string Message { get; }
        public DeviantArtException(string message) : base(message)
        {
        }

        public DeviantArtException(HttpStatusCode statusCode)
        {
            switch ((int)statusCode)
            {
                case 400:
                    Message = "Request failed due to client error, e.g. validation failed or User not found";
                    break;
                case 429:
                    Message = "Rate limit reached or service overloaded see Rate Limits";
                    break;
                case 500:
                    Message = "Our servers encountered an internal error, try again";
                    break;
                case 503:
                    Message = "Our servers are currently unavailable, try again later. This is normally due to planned or emergency maintenance.";
                    break;
                default:
                    Message = "Inner exception";
                    break;
            }
        }
    }
}
