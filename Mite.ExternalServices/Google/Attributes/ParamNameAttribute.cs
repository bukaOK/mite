using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.ExternalServices.Google.Attributes
{
    public class ParamNameAttribute : Attribute
    {
        /// <summary>
        /// Имя параметра запроса в google api
        /// </summary>
        public string RequestParamName { get; set; }

        public ParamNameAttribute(string paramName)
        {
            RequestParamName = paramName;
        }
    }
}