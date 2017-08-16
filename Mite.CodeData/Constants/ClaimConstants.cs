using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mite.CodeData.Constants
{
    public static class ClaimConstants
    {
        public const string AvatarSrc = "AvatarSrc";

        //Используются в Startup.cs
        public const string ExternalServiceToken = "ExternalServiceToken";
        public const string ExternalServiceExpires = "ExternalServiceExpires";
        public const string ExternalServiceName = "ExternalServiceName";
    }
}