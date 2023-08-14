using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Http.Controllers;
using System.Web.Http.Tracing;
using System.Web.Http;
using PaloUpAPI.Helpers;

namespace PaloUpAPI.ActionFilters
{
    public class LoggingFilterAttribute : ActionFilterAttribute
    {

        /*
                 public virtual void OnActionExecuted(HttpActionExecutedContext actionExecutedContext);
        public virtual Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken);
        //
        // Summary:
        //     Occurs before the action method is invoked.
        //
        // Parameters:
        //   actionContext:
        //     The action context.
        public virtual void OnActionExecuting(HttpActionContext actionContext);
        public virtual Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken);
         */

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            try
            {
                GlobalConfiguration.Configuration.Services.Replace(typeof(ITraceWriter), new NLogger());
                var trace = GlobalConfiguration.Configuration.Services.GetTraceWriter();
                trace.Info(actionExecutedContext.Request, "Controller : " + actionExecutedContext.ActionContext.ControllerContext.Controller + Environment.NewLine + "Is Success :" + actionExecutedContext.Response.IsSuccessStatusCode + Environment.NewLine + "Action : "
                    + actionExecutedContext.ActionContext.ActionDescriptor.ActionName, "JSON", actionExecutedContext.ActionContext.ActionArguments);
                base.OnActionExecuted(actionExecutedContext);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
       // public override void OnActionExecuting(HttpActionContext filterContext)
       // {
            //GlobalConfiguration.Configuration.Services.Replace(typeof(ITraceWriter), new NLogger());
            //var trace = GlobalConfiguration.Configuration.Services.GetTraceWriter();
            //trace.Info(filterContext.Request, "Controller : " + filterContext.ControllerContext.ControllerDescriptor.ControllerType.FullName + Environment.NewLine + "Action : " + filterContext.ActionDescriptor.ActionName, "JSON", filterContext.ActionArguments);
     //   }
    }
}