using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.UI.WebControls;
using Mite.Attributes.DataAnnotations;

namespace Mite.Extensions
{
    public static class ValidationExtensions
    {
        public static MvcHtmlString ValidationSum(this HtmlHelper htmlHelper, string divId = "validationSum")
        {
            if(htmlHelper.ViewData.ModelState.IsValid)
                return new MvcHtmlString($"<div class=\"ui error message\" id=\"{divId}\" style=\"display:none\"></div>");
            var errors = htmlHelper.ViewData.ModelState.Where(x => x.Value.Errors.Count > 0).ToList();
            var stringBuilder = new StringBuilder($"<div class=\"ui error message\" style=\"display: block\" id=\"{divId}\"><ul class=\"list\">");
            foreach (var error in errors)
            {
                foreach (var valueError in error.Value.Errors)
                {
                    stringBuilder.AppendFormat("<li>{0}</li>", valueError.ErrorMessage);
                }
            }
            stringBuilder.Append("</ul></div>");
            return new MvcHtmlString(stringBuilder.ToString());
        }
        /// <summary>
        /// Рендерит валидацию форме на js(плагин semantic form validation)
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="formSelector">Селектор формы</param>
        /// <param name="actionOn">На какое событие реагировать(список на semantic ui)</param>
        /// <returns></returns>
        public static MvcHtmlString FormValidation<TModel>(this HtmlHelper<TModel> htmlHelper, string formSelector, string actionOn = "submit")
        {
            var modelType = typeof(TModel);
            var modelProps = modelType.GetProperties();

            var str = new StringBuilder($"$('{formSelector}').form({{on: '{actionOn}', fields: {{");
            foreach(var prop in modelProps)
            {
                if (prop.GetCustomAttribute<OffClientValidationAttribute>() != null)
                    continue;

                var propType = prop.PropertyType;
                var id = "#" + htmlHelper.Id(prop.Name);

                str.Append($"{prop.Name}:{{rules:[");

                var reqAttr = prop.GetCustomAttribute<RequiredAttribute>();
                if(reqAttr != null)
                    str.Append( $"{{type: 'empty',prompt: $('{id}').data('valRequired')}},");

                var regExpAttr = prop.GetCustomAttribute<RegularExpressionAttribute>();
                if(regExpAttr != null)
                    str.Append($"{{type: 'regExp', value: '{regExpAttr.Pattern}',prompt: $('{id}').data('val-regex')}},");

                var minLengthAttr = prop.GetCustomAttribute<MinLengthAttribute>();
                if(minLengthAttr != null)
                str.Append($"{{type: 'minLength[{minLengthAttr.Length}]', prompt: $('{id}').data('val-minlength')}},");

                var maxLengthAttr = prop.GetCustomAttribute<MaxLengthAttribute>();
                if(maxLengthAttr != null)
                    str.Append($"{{type: 'maxLength[{maxLengthAttr.Length}]',prompt: $('{id}').data('val-maxlength')}},");

                var emailAttr = prop.GetCustomAttribute<EmailAddressAttribute>();
                if(emailAttr != null)
                    str.Append($"{{type: 'email', prompt: $('{id}').data('val-email')}},");

                var compareAttr = prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.CompareAttribute>();
                if(compareAttr != null)
                    str.Append($"{{type: 'match[{compareAttr.OtherProperty}]',prompt: $('{id}').data('valEqualto')}},");

                if(propType == typeof(int) || propType == typeof(long) || propType == typeof(int?) || propType == typeof(long?) ||
                    propType == typeof(double) || propType == typeof(float) || propType == typeof(decimal)
                    || propType == typeof(double?) || propType == typeof(float?) || propType == typeof(decimal?))
                {
                    str.Append("{type: 'number', prompt: 'Введите численное значение.'},");
                }

                str.Append("]},");
            }
            str.Append("}});");
            return new MvcHtmlString(str.ToString());
        }
    }
}