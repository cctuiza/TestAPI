using PaloUpAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;



namespace PaloUpAPI.ActionFilters
{
   
    public class CustomAuthorizeAttribute : ActionFilterAttribute, IActionFilter
    {
        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            // Grab the current request headers
            IEnumerable<string> values;

            filterContext.Request.Headers.TryGetValues("SERVERID", out values);

            if (values != null)
            {
                //string name = "ronel gonzales";
                tokenn.serverId = values.First();
            }
            else
            {
                //redirect to error page
                string last = " gonzales";
              

            }

            //// Ensure that all of your properties are present in the current Request
            //if (!String.IsNullOrEmpty(headers["SERVERID"]))
            //{
            //    // All of those properties are available, handle accordingly

            //    // You can redirect your user based on the following line of code
            //    //filterContext.Result = new RedirectResult(url);
            //}
            //else
            //{
            //    // Those properties were not present in the header, redirect somewhere else
            //}

            //// Additional Auditing-based Logic Here
            base.OnActionExecuting(filterContext);
        }
    }
}