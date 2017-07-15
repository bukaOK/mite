using Mite.ExternalServices.Google.Attributes;
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
            var reqParams = new Dictionary<string, string>();
            //Тип текущего запроса
            var type = GetType();
            //Получаем свойства запроса
            var props = type.GetProperties();
            //Убираем нулевые значения
            props = props.Where(prop => prop.GetValue(this, null) != null).ToArray();
            foreach(var prop in props)
            {
                //Получаем атрибут с именем параметра для GoogleApi
                var attr = (ParamNameAttribute)Attribute.GetCustomAttribute(prop, typeof(ParamNameAttribute));
                reqParams.Add(attr.RequestParamName, (string)prop.GetValue(this, null));
            }
            return reqParams;
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
            return str.Substring(0, str.Length - 1);
        }
    }
}