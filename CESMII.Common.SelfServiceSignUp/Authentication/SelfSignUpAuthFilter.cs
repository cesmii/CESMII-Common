﻿namespace CESMII.Common.SelfServiceSignUp
{
    using CESMII.Common.SelfServiceSignUp.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Text;

    public class SelfSignUpAuthFilter : IAuthorizationFilter
    {
        protected readonly ILogger<SelfSignUpAuthFilter> _logger;
        private IConfiguration _config;

        public SelfSignUpAuthFilter(
            ILogger<SelfSignUpAuthFilter> logger, 
            IConfiguration config)
        {
            _config = config.GetSection("SelfSignUpAuth");
            _logger = logger;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            _logger.LogInformation($"OnAuthorization: Validating API Connector");
            try
            {
                string authHeader = context.HttpContext.Request.Headers["Authorization"];
                if (authHeader != null)
                {
                    var authHeaderValue = AuthenticationHeaderValue.Parse(authHeader);
                    //var authHeaderValue = this.Parse(authHeader);
                    string strWhatIsThis = AuthenticationSchemes.Basic.ToString();
                    if (authHeaderValue.Scheme.Equals(AuthenticationSchemes.Basic.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        var credentials = Encoding.UTF8
                            .GetString(Convert.FromBase64String(authHeaderValue.Parameter ?? string.Empty))
                            .Split(':', 2);
                        if (credentials.Length == 2)
                        {
                            if (IsAuthorized(context, credentials[0], credentials[1]))
                            {
                                _logger.LogInformation($"OnAuthorization: Valid!");
                                return;
                            }
                        }
                    }
                }

                _logger.LogError("OnAuthorization: Credentials are incorrect.");
                ReturnUnauthorizedResult(context);
            }
            catch (FormatException ex)
            {
                _logger.LogError($"OnAuthorization: Credential exception: {ex.Message}.");
                ReturnUnauthorizedResult(context);
            }
        }

        public bool IsAuthorized(AuthorizationFilterContext context, string username, string password)
        {
            return IsValidUser(username, password);
        }

        private bool IsValidUser(string username, string password)
        {
            if (username.Equals(_config.GetValue(typeof(string), "ApiUsername")) && password.Equals(_config.GetValue(typeof(string), "ApiPassword")))
            {
                return true;
            }

            return false;
        }

        private void ReturnUnauthorizedResult(AuthorizationFilterContext context)
        {
            context.Result = new UnauthorizedResult();
        }
    }
}
