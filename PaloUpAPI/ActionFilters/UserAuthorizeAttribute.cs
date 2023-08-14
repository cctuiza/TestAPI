using PaloUpAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using PaloUpAPI.Models;

//GETTING THE SERVER ID IN THE REQUEST HEADER
namespace PaloUpAPI.ActionFilters
{
   
    public class UserAuthorizeAttribute : ActionFilterAttribute, IActionFilter
    {
        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            // Grab the current request headers
            IEnumerable<string> values;

            filterContext.Request.Headers.TryGetValues("UserId", out values);

            if (values != null)
            {
                //string name = "ronel gonzales";
               UserParam.userId =int.Parse(values.First());
            }
            else
            {
                //redirect to error page
                UserParam.userId = null;
            }

            //// Additional Auditing-based Logic Here
            base.OnActionExecuting(filterContext);
        }
    }
}