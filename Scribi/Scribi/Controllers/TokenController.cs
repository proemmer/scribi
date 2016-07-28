using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using Scribi.Auth;
using Scribi.Interfaces;
using Scribi.Models;

namespace Webpac.Controllers
{
    [Route("api/[controller]")]
    public class TokenController : Controller
    {
        private readonly ILogger _logger;
        private readonly TokenAuthOptions _tokenOptions;
        private readonly IAuthenticationService _authService;

        public class AuthRequest
        {
            public string username { get; set; }
            public string password { get; set; }
        }


        public TokenController(TokenAuthOptions tokenOptions,
                                IAuthenticationService authService,
                                ILogger<TokenController> logger)
        {
            _logger = logger;
            _authService = authService;
            _tokenOptions = tokenOptions;
        }

        /// <summary>
        /// Check if currently authenticated. Will throw an exception of some sort which shoudl be caught by a general
        /// exception handler and returned to the user as a 401, if not authenticated. Will return a fresh token if
        /// the user is authenticated, which will reset the expiry.
        /// </summary>
        /// <returns></returns>
        //[HttpGet]
        //[Authorize("AdministrationPolicy")]
        //public dynamic Get()
        //{
        //    /* 
        //    ******* WARNING WARNING WARNING ****** 
        //    ******* WARNING WARNING WARNING ****** 
        //    ******* WARNING WARNING WARNING ****** 
        //    THIS METHOD SHOULD BE REMOVED IN PRODUCTION USE-CASES - IT ALLOWS A USER WITH 
        //    A VALID TOKEN TO REMAIN LOGGED IN FOREVER, WITH NO WAY OF EVER EXPIRING THEIR
        //    RIGHT TO USE THE APPLICATION.
        //    Refresh Tokens (see https://auth0.com/docs/refresh-token) should be used to 
        //    retrieve new tokens. 
        //    ******* WARNING WARNING WARNING ****** 
        //    ******* WARNING WARNING WARNING ****** 
        //    ******* WARNING WARNING WARNING ****** 
        //    */
        //    bool authenticated = false;
        //    string user = null;
        //    string role = null;
        //    string token = null;
        //    DateTime? tokenExpires = default(DateTime?);

        //    var currentUser = HttpContext.User;
        //    if (currentUser != null)
        //    {
        //        authenticated = currentUser.Identity.IsAuthenticated;
        //        if (authenticated)
        //        {
        //            user = currentUser.Identity.Name;
        //            foreach (Claim c in currentUser.Claims) if (c.Type == ClaimTypes.Role) role = c.Value;
        //            tokenExpires = DateTime.UtcNow.AddMinutes(2);
        //            token = GetToken(currentUser.Identity.Name, role, tokenExpires);
        //        }
        //    }
        //    return new Authentication
        //    {
        //        Authenticated = authenticated,
        //        User = user,
        //        Role = role,
        //        Token = token,
        //        TokenExpires = tokenExpires
        //    };
        //}


        /// <summary>
        /// Request a new token for a given username/password pair.
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public Authentication Post([FromBody] AuthRequest req)
        {
            User user = null;
            // Obviously, at this point you need to validate the username and password against whatever system you wish.
            if (_authService.TryAuthorize(req.username, req.password, ref user))
            {
                var role = user.Type.ToString();
                DateTime? expires = DateTime.UtcNow.AddMinutes(_authService.ValidationTimeInMin);
                var token = GetToken(req.username, role, expires);
                return new Authentication
                {
                    Authenticated = true,
                    User = req.username,
                    Role = role,
                    Token = token,
                    TokenExpires = expires
                };
            }
            return new Authentication
            {
                Authenticated = false
            };
        }


        private string GetToken(string user, string role, DateTime? expires)
        {
            var handler = new JwtSecurityTokenHandler();

            // Here, you should create or look up an identity for the user which is being authenticated.
            // For now, just creating a simple generic identity.
            ClaimsIdentity identity = new ClaimsIdentity(new GenericIdentity(user, "TokenAuth"), new[]
            {
                new Claim(ClaimTypes.Role,role,ClaimValueTypes.String)
            });

            var securityToken = handler.CreateJwtSecurityToken(
                issuer: _tokenOptions.Issuer,
                audience: _tokenOptions.Audience,
                signingCredentials: _tokenOptions.SigningCredentials,
                subject: identity,
                expires: expires
                );
            return handler.WriteToken(securityToken);
        }
    }
}
