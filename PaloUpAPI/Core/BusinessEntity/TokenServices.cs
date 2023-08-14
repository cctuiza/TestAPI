using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PaloUpAPI.Core.BusinessEntity;
using System.Text;
using PaloUpAPI.Models;

namespace PaloUpAPI.Core.BusinessServices
{
    public class TokenServices : ITokenServices
    {
        #region Private member variables.
        private  PALOUPEntities _unitOfWork;
        #endregion

        #region Public constructor.
        /// <summary>
        /// Public constructor.
        /// </summary>
        public TokenServices(PALOUPEntities unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #endregion


        #region Public member methods.

        /// <summary>
        ///  Function to generate unique token with expiry against the provided userId.
        ///  Also add a record in database for generated token.
        /// </summary>

        public TokenEntity GenerateToken(string userId)
        {

            using (_unitOfWork = new PALOUPEntities())
            {


              

                DateTime issuedOn = DateTime.Now;
                DateTime expiredOn = DateTime.Now.AddDays(Convert.ToDouble(ConfigurationManager.AppSettings["AuthTokenExpiry"]));


                byte[] byteToken = Encoding.UTF8.GetBytes(userId + ":" + Guid.NewGuid().ToString() + String.Concat("tsaf") + ":" + expiredOn);

                string token = Convert.ToBase64String(byteToken).ToString(); //basis on checking generated token if it is fastcargo


                var tokendomain = new token
                {
                    UserId = userId,
                    AuthToken = token,
                    IssuedOn = issuedOn,
                    ExpiresOn = expiredOn,
                    IsExpire = true
                };

                _unitOfWork.tokens.Add(tokendomain);
                _unitOfWork.SaveChanges();
                var tokenModel = new TokenEntity()
                {
                    UserId = userId,
                    IssuedOn = issuedOn,
                    ExpiresOn = expiredOn,
                    AuthToken = token
                };

                return tokenModel;
            }
        }

        /// <summary>
        /// Method to validate token against expiry and existence in database.
        /// </summary>
        /// <param name="tokenId"></param>
        /// <returns></returns>
        public bool ValidateToken(string tokenId)
        {
            using (_unitOfWork = new PALOUPEntities())
            {

                var FromBase64 = Encoding.UTF8.GetString(Convert.FromBase64String(tokenId));

                var credentials = FromBase64.Split(':');
                if (credentials.Length == 2)
                {
                
                    if (credentials[1].Substring(credentials[1].Length - 4).Equals("tsaf")) //if token came from fast
                    {
                        var token = _unitOfWork.tokens.Where(t => t.AuthToken == tokenId).ToList();// && t.ExpiresOn > DateTime.Now);


                        if (token != null && token.First().IsExpire == false)
                        {
                            return true;
                        }
                        else
                        {
                            if (token != null && !(DateTime.Now > token.First().ExpiresOn))
                            {
                                //update the token that is expire is true
                                if (token.First().IsExpire == true)
                                {
                                    token.First().ExpiresOn = DateTime.Now.AddDays(Convert.ToDouble(ConfigurationManager.AppSettings["AuthTokenExpiry"]));
                                    _unitOfWork.SaveChanges();
                                }
                                return true;
                            }
                        }
                        
                    }
                }
                else
                {
                    if (credentials[1].Substring(credentials[1].Length - 4).Equals("tsaf")) //if token came from fast
                    {
                        var token = _unitOfWork.tokens.Where(t => t.AuthToken == tokenId).ToList();// && t.ExpiresOn > DateTime.Now);


                        if (token != null && token.First().IsExpire == false)
                        {
                            return true;
                        }
                        else
                        {
                            if (token != null && !(DateTime.Now > token.First().ExpiresOn))
                            {
                                //update the token that is expire is true
                                if (token.First().IsExpire == true)
                                {
                                    token.First().ExpiresOn = DateTime.Now.AddDays(Convert.ToDouble(ConfigurationManager.AppSettings["AuthTokenExpiry"]));
                                    _unitOfWork.SaveChanges();
                                }
                                return true;
                            }
                        }

                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Method to kill the provided token id.
        /// </summary>
        /// <param name="tokenId">true for successful delete</param>
        public bool Kill(string tokenId)
        {
            using (_unitOfWork = new PALOUPEntities())
            {

                var tblToken = _unitOfWork.tokens.Where(x => x.AuthToken == tokenId).First();
                _unitOfWork.tokens.Remove(tblToken);
                _unitOfWork.SaveChanges();
                var isNotDeleted = _unitOfWork.tokens.Where(x => x.AuthToken == tokenId).Any();
                if (isNotDeleted) { return false; }
                return true;
            }
        }

        /// <summary>
        /// Delete tokens for the specific deleted user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>true for successful delete</returns>
        public bool DeleteByUserId(string userId)
        {
            using (_unitOfWork = new PALOUPEntities())
            {

                var tblToken = _unitOfWork.tokens.Where(x => x.UserId == userId).First();
                _unitOfWork.tokens.Remove(tblToken);
                _unitOfWork.SaveChanges();

                var isNotDeleted = _unitOfWork.tokens.Where(x => x.UserId == userId).Any();
                return !isNotDeleted;
            }
        }

        #endregion
    }
}