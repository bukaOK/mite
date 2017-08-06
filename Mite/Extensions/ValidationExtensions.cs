using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Mite.Extensions
{
    public static class ValidationExtensions
    {
        public static MvcHtmlString ValidationSum(this HtmlHelper htmlHelper, string divId = "validationSum")
        {
            if(htmlHelper.ViewData.ModelState.IsValid)
                return new MvcHtmlString($"<div class=\"ui error message\" id=\"{divId}\"></div>");
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
    }
}