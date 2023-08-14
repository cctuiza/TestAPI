using PaloUpAPI.Core.BusinessServices;
using PaloUpAPI.Filters;
using PaloUpAPI.Core.BusinessEntity;
using PaloUpAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace FscCore.Controllers
{
    [ApiAuthenticationFilter]
    public class AuthenticateController : ApiController
    {
        #region Private variable.
        private PALOUPEntities _unitOfWork;
        private readonly ITokenServices _tokenServices;

        #endregion

        #region Public Constructor

        /// <summary>
        /// Public constructor to initialize product service instance
        /// </summary>
        /// 

        public AuthenticateController() { 
        }
        public AuthenticateController(ITokenServices tokenServices)
        {
            _tokenServices = tokenServices;
        }

        #endregion

        /// <summary>
        /// Authenticates user and returns token with expiry.
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        public HttpResponseMessage Start()
        {
            if (System.Threading.Thread.CurrentPrincipal != null && System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                var basicAuthenticationIdentity = System.Threading.Thread.CurrentPrincipal.Identity as BasicAuthenticationIdentity;
                if (basicAuthenticationIdentity != null)
                {
                    var userId = basicAuthenticationIdentity.UserId;
                
                    return GetAuthToken(userId);
                }
            }
            return null;
        }

         //<summary>
         //Returns auth token for the validated user.
         //</summary>
         //<param name="userId"></param>
         //<returns></returns>
        private HttpResponseMessage GetAuthToken(string userId)
        {
            _unitOfWork = new PALOUPEntities();
            var token = new TokenServices(_unitOfWork);
            TokenEntity tokEntity = token.GenerateToken(userId);
            var response = Request.CreateResponse(HttpStatusCode.OK, "Authorized");

            response.Headers.Add("Token", tokEntity.AuthToken);
            response.Headers.Add("TokenExpiry", ConfigurationManager.AppSettings["AuthTokenExpiry"]);
            response.Headers.Add("Access-Control-Expose-Headers", "Token,TokenExpiry");
            return response;
        }
    }
}
