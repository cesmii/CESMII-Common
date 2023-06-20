namespace CESMII.Common.SelfServiceSignUp
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
                if (authHeader == null)
                {
                    _logger.LogError("OnAuthorization: authHeader == null.");
                }
                else
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
                        else
                        {
                            _logger.LogError("OnAuthorization: credentials.Length != 2.");
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
            string strConfigUser = (string)_config.GetValue(typeof(string), "ApiUsername");
            string strConfigPwd = (string)_config.GetValue(typeof(string), "ApiPassword");

            // Debug helpers
            //if (username == null) username = "[null]";
            //if (password == null) password = "[null]";

            //_logger.LogError($"OnAuthorization: username = {username}");
            //_logger.LogError($"OnAuthorization: password = {password}");

            //if (strConfigUser == null) strConfigUser="[null]";
            //if(strConfigPwd == null) strConfigPwd="[null]";

            //_logger.LogError($"OnAuthorization: strConfigUser = {strConfigUser}");
            //_logger.LogError($"OnAuthorization: strConfigPwd = {strConfigPwd}");

            if (username.Equals(strConfigUser) && password.Equals(strConfigPwd))
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
