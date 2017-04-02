using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Mite.Extensions
{
    public static class ValidationExtensions
    {
        public static MvcHtmlString ValidationSum(this HtmlHelper htmlHelper)
        {
            if(htmlHelper.ViewData.ModelState.IsValid)
                return new MvcHtmlString("<div class=\"ui error message\" id=\"validationSum\"></div>");
            var errors = htmlHelper.ViewData.ModelState.Where(x => x.Value.Errors.Count > 0).ToList();
            var stringBuilder = new StringBuilder("<div class=\"ui error message\" style=\"display: block\"><ul class=\"list\">");
            foreach (var error in errors)
            {
                foreach (var valueError in error.Value.Errors)
                {
                    stringBuilder.Append("<li>" + valueError.ErrorMessage + "</li>");
                }
            }
            stringBuilder.Append("</ul></div>");
            return new MvcHtmlString(stringBuilder.ToString());
        }
    }
}