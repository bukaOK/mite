using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Mite.ExternalServices.Google.Core
{
    public interface IRequestParams
    {
        IDictionary<string, string> DictionaryParams();
        string StringParams();
    }
    public abstract class RequestParams : IRequestParams
    {
        public IDictionary<string, string> DictionaryParams()
        {
            var type = GetType();
            var props = type.GetProperties();
            return props.Where(prop => prop.GetValue(this, null) != null)
            .ToDictionary(prop => prop.Name.ToLower(), prop => prop.GetValue(this, null).ToString());
        }

        public string StringParams()
        {
            var strB = new StringBuilder();
            foreach (var pair in DictionaryParams())
            {
                strB.AppendFormat("{0}={1}", pair.Key, pair.Value);
                strB.Append("&");
            }
            var str = strB.ToString();
            return str.Substring(str.Length - 1, 1);
        }
    }
}