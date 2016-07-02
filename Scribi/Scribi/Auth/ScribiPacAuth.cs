using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Scribi.Auth
{
    public static class ScribiAuth
    {
        const string TokenAudience = "ScribiAudience";
        const string TokenIssuer = "ScribiIssuer";
        private static RsaSecurityKey _key;
        private static TokenAuthOptions _tokenOptions;


        public static void AddScribiAuthentication(this IServiceCollection services, string tokenFile = null)
        {
            // *** CHANGE THIS FOR PRODUCTION USE ***
            // Here, we're generating a random key to sign tokens - obviously this means
            // that each time the app is started the key will change, and multiple servers 
            // all have different keys. This should be changed to load a key from a file 
            // securely delivered to your application, controlled by configuration.
            //
            // See the RSAKeyUtils.GetKeyParameters method for an example of loading from
            // a JSON file.
            RSAParameters keyParams;

            if (!string.IsNullOrWhiteSpace(tokenFile) )
            {
                if (!File.Exists(tokenFile))
                {
                    if (Directory.Exists(Path.GetDirectoryName(tokenFile)))
                        RSAKeyUtils.GenerateKeyAndSave(tokenFile);
                    else
                        throw new FileNotFoundException(tokenFile);
                }
                keyParams = RSAKeyUtils.GetKeyParameters(tokenFile);
            }
            else
                keyParams = RSAKeyUtils.GetRandomKey();

            // Create the key, and a set of token options to record signing credentials 
            // using that key, along with the other parameters we will need in the 
            // token controller.
            _key = new RsaSecurityKey(keyParams);
            _tokenOptions = new TokenAuthOptions
            {
                Audience = TokenAudience,
                Issuer = TokenIssuer,
                SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.RsaSha256Signature)
            };

            // Save the token options into an instance so they're accessible to the 
            // controller.
            services.AddSingleton(_tokenOptions);
            ConfigureAuthenticationService(services);
        }


        public static void UseScribiAuth(this IApplicationBuilder app)
        {
            //this is needed to support Authorization via query string or SignalR
            app.Use(async (context, next) =>
            {
                if (string.IsNullOrWhiteSpace(context.Request.Headers["Authorization"]) && context.Request.QueryString.HasValue)
                {
                    var token = context.Request.QueryString.Value
                        .Split('&')
                        .SingleOrDefault(x => x.Contains("BearerToken"))?.Split('=')[1];

                    if (!string.IsNullOrWhiteSpace(token))
                        context.Request.Headers.Add("Authorization", new[] { $"Bearer {token}" });
                }
                await next.Invoke();
            });


            app.UseJwtBearerAuthentication(GetBearerOptions());
        }

        private static JwtBearerOptions GetBearerOptions()
        {
            var options = new JwtBearerOptions();

            options.AutomaticAuthenticate = true;
            options.AutomaticChallenge = true;

            // Basic settings - signing key to validate with, audience and issuer.
            options.TokenValidationParameters.IssuerSigningKey = _key;
            options.TokenValidationParameters.ValidAudience = _tokenOptions.Audience;
            options.TokenValidationParameters.ValidIssuer = _tokenOptions.Issuer;

            // When receiving a token, check that we've signed it.
            //options.TokenValidationParameters.ValidateSignature = true;

            // When receiving a token, check that it is still valid.
            options.TokenValidationParameters.ValidateLifetime = true;

            // This defines the maximum allowable clock skew - i.e. provides a tolerance on the token expiry time 
            // when validating the lifetime. As we're creating the tokens locally and validating them on the same 
            // machines which should have synchronized time, this can be set to zero. Where external tokens are
            // used, some leeway here could be useful.
            options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(0);

            return options;
        }

        /// <summary>
        /// Add Policies for the authorization and us Bearer for Authentication
        /// </summary>
        /// <param name="services"></param>
        private static void ConfigureAuthenticationService(IServiceCollection services)
        {
            services.AddAuthentication(config =>
            {
                config.SignInScheme = JwtBearerDefaults.AuthenticationScheme;
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdministrationPolicy", new AuthorizationPolicyBuilder()
                    .RequireClaim(ClaimTypes.Role, "Admin")
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser()
                    .Build());

                options.AddPolicy("ReadWritePolicy", new AuthorizationPolicyBuilder()
                    .RequireClaim(ClaimTypes.Role, "ReadWrite", "Admin")
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser()
                    .Build());

                options.AddPolicy("ReadOnlyPolicy", new AuthorizationPolicyBuilder()
                    .RequireClaim(ClaimTypes.Role, "ReadOnly", "ReadWrite", "Admin")
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser()
                    .Build());
            });
        }
    }
}
