namespace CESMII.Common.SelfServiceSignUp.Services
{
    using System;
    using System.Net;
    using System.Net.Mail;

    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SendGrid;
    using SendGrid.Helpers.Mail;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;

    using CESMII.Common.SelfServiceSignUp.Models;
    using CESMII.Common;
    using Microsoft.AspNetCore.Components.Web;
    using System.Text;

#pragma warning disable 8601, 8602, 8604  // Suppress warnings that items might have a null value.
    public class MailRelayService
    {
        protected readonly ILogger<MailRelayService> _logger;
        private readonly MailConfig _config;

        public MailRelayService(
            ILogger<MailRelayService> logger,
            IConfiguration configuration)
        {
            _config = new ConfigUtil(configuration).MailSettings;
            _logger = logger;
        }


#pragma warning disable 8600

        /// <summary>
        /// SendEmailSendGrid - from SSSU
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> SendEmailSendGrid(MailMessage message, string strCaller = "")
        {
            _logger.LogError($"{strCaller}MailRelayService: SendEmailSendGrid@150 called (caller = {strCaller})");
            DumpConfigState($"{strCaller}@46: ");

            bool bSuccess = false;
            try
            {
                var leaTo = new List<EmailAddress>();
                // If Mail Relay is in debug mode set all addresses to the configuration file.
                if (_config.Debug)
                {
                    _logger.LogInformation($"{strCaller}Mail relay is in debug mode. Redirecting target email to: {string.Join("|", _config.DebugToAddresses)}");
                    foreach (var address in _config.DebugToAddresses)
                    {
                        leaTo.Add(new EmailAddress(address));
                        _logger.LogInformation($"{strCaller}Adding Email To: {address}");
                    }
                }
                else
                {
                    foreach (var address in _config.ToAddresses)
                    {
                        leaTo.Add(new EmailAddress(address));
                        _logger.LogInformation($"{strCaller}Adding Email To: {address}");
                    }
                }

                await SendEmailSendGrid(message, leaTo, strCaller);

            }
            catch (Exception ex)
            {
                _logger.LogError($"{strCaller}MailRelayService: Exception -- {ex.Message}");
            }

            DumpConfigState($"{strCaller}@79: ");
            return bSuccess;
        }

        /// <summary>
        /// SendEmailSendGrid - From publish
        /// </summary>
        /// <param name="message"></param>
        /// <param name="leaTo"></param>
        /// <returns></returns>
        public async Task<bool> SendEmailSendGrid(MailMessage message, List<EmailAddress> leaTo, string strCaller = "")
        {
            _logger.LogError($"{strCaller}MailRelayService: SendEmailSendGrid@192 called (caller = {strCaller})");
            DumpConfigState($"{strCaller}@92: ");

            bool bSuccess = false;

            if (leaTo == null || leaTo.Count == 0)
            {
                _logger.LogError($"{strCaller}SendEmailSendGrid: leaTo is null or empty");
            }
            else
            {
                int i = 0;
                foreach (var address in leaTo)
                {
                    _logger.LogError($"{strCaller}SendEmailSendGrid - leaTo[{i}] = {address.Email} ");
                    i++;
                }
            }

            if (string.IsNullOrEmpty(_config.MailFromAddress))
            {
                _config.MailFromAddress = "paul.yao@c-labs.com";
                _logger.LogError($"{strCaller}SendEmailSendGrid: _config.MailFromAddress was null or empty. Setting to Secret Santa default.");
            }

            if (string.IsNullOrEmpty(_config.MailFromAppName))
            {
                _config.MailFromAppName = "Profile Designer (Staging)";
                _logger.LogError($"{strCaller}SendEmailSendGrid: _config.MailFromAppName was null or empty. Setting to Secret Santa default.");
            }

            string strApiKey = _config.ApiKey;
            if (string.IsNullOrEmpty(strApiKey))
            {
                _logger.LogError($"{strCaller}SendEmailSendGrid: _config.ApiKey was null or empty. Not sending.");
                return false;
            }
            var eaFrom = new EmailAddress(_config.MailFromAddress, _config.MailFromAppName);

            if (_config.ToAddresses == null || _config.ToAddresses.Count == 0)
            {
                _logger.LogError($"{strCaller}MailRelayService::SendEmailSendGrid - ToAddresses is null or empty");

                _config.ToAddresses = new List<string>();
                _config.ToAddresses.Add("paul.yao@c-labs.com");
            }

            var client = new SendGridClient(strApiKey);
            var subject = message.Subject;
            if (string.IsNullOrEmpty(subject))
            {
                _logger.LogError($"{strCaller}SendEmailSendGrid: subject was null or empty.");
            }

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(eaFrom, leaTo, subject, null, message.Body);

            var response = await client.SendEmailAsync(msg);
            if (response == null)
            {
                _logger.LogError($"{strCaller}MailRelayService: SendEmailSendGrid Error. Response is null");
            }
            else
            {
                bSuccess = response.IsSuccessStatusCode;
                if (!bSuccess)
                {
                    _logger.LogError($"{strCaller}MailRelayService: SendEmailSendGrid Error. Status Code: {response.StatusCode}");
                }
            }

            DumpConfigState($"{strCaller}@161: ");

            return bSuccess;
        }


        private void DumpConfigState(string strCaller)
        {
            StringBuilder sb = new StringBuilder();
            if (_config == null)
            {
                sb.Append("_config is null");
            }
            else
            {
                //_config.ApiKey
                string strResult = PutStringIntoString(_config.ApiKey);
                sb.Append($"_config.ApiKey: {strResult} $$$ ");

                // _config.Debug
                sb.Append($"_config.Debug={_config.Debug.ToString()} $$$ ");

                // _config.Enabled
                sb.Append($"_config.Enabled={_config.Enabled.ToString()} $$$ ");

                //_config.DebugToAddresses
                strResult = PutListIntoString(_config.DebugToAddresses);
                sb.Append($"_config.DebugToAddresses: {strResult} $$$ ");

                //_config.BaseUrl
                strResult = PutStringIntoString(_config.BaseUrl);
                sb.Append($"_config.BaseUrl: {strResult} $$$ ");

                //_config.MailFromAddress
                strResult = PutStringIntoString(_config.MailFromAddress);
                sb.Append($"_config.MailFromAddress: {strResult} $$$ ");

                //_config.MailFromAppName
                strResult = PutStringIntoString(_config.MailFromAppName);
                sb.Append($"_config.MailFromAppName: {strResult} $$$ ");

                //_config.ToAddresses
                strResult = PutListIntoString(_config.ToAddresses);
                sb.Append($"_config.ToAddresses: {strResult} $$$ ");

                //_config.Address
                strResult = PutStringIntoString(_config.Address);
                sb.Append($"_config.Address: {strResult} $$$ ");

                //_config.Port
                strResult = PutStringIntoString(_config.Port.ToString());
                sb.Append($"_config.Port: {strResult} $$$ ");

                // _config.EnableSSL
                sb.Append($"_config.EnableSSL={_config.EnableSSL.ToString()} $$$ ");

                //_config.Username
                strResult = PutStringIntoString(_config.Username);
                sb.Append($"_config.Username: {strResult} $$$ ");

                //_config.Password
                strResult = PutStringIntoString(_config.Password);
                sb.Append($"_config.Password: {strResult} $$$ ");

                //_config.Address
                strResult = PutStringIntoString(_config.Address);
                sb.Append($"_config.Address: {strResult} $$$ ");
            }

            string strOutput = sb.ToString();
            _logger.LogError($"{strCaller}MailRelayService [DumpConfigState]: {strOutput}");
        }

        private string PutListIntoString(List<string> l)
        {
            string strReturn = "empty";
            if (l != null && l.Count > 0)
            {
                strReturn = "[";
                foreach (string str in l)
                {
                    strReturn += $"{str}, ";
                }

                int i = strReturn.LastIndexOf(",");
                if (i > 1)
                {
                    strReturn = strReturn.Substring(0, i);
                }
                strReturn += "]";
            }

            return strReturn;
        }

        private string PutStringIntoString(string str)
        {
            string strReturn = "empty";
            if (!string.IsNullOrEmpty(str))
            {
                strReturn = $"[{str}]";
            }

            return strReturn;
        }
    }
}



//public async Task<bool> SendEmail(MailMessage message)
//{
//    switch (_config.Provider)
//    {
//        case "SMTP":
//            if (!await SendSmtpEmail(message))
//            {
//                return false;
//            }

//            break;
//        case "SendGrid":
//            if (!await SendEmailSendGrid(message))
//            {
//                return false;
//            }
//            break;
//        default:
//            _logger.LogError("MailRelayService::SendEmail - Provider key value is invalid");
//            throw new InvalidOperationException("The configured email provider is invalid.");
//    }
//    return true;
//}

//private async Task<bool> SendSmtpEmail(MailMessage message)
//{
//    // Do not proceed further if mail relay is disabled.
//    if (!_config.Enabled)
//    {
//        _logger.LogWarning("Mail Relay is disabled.");
//        return true;
//    }

//    // Configure the SMTP client and send the message
//    var client = new SmtpClient
//    {
//        Host = _config.Address,
//        Port = _config.Port,

//        // Use whatever SSL mode is set.
//        EnableSsl = _config.EnableSsl,
//        DeliveryMethod = SmtpDeliveryMethod.Network
//    };

//    if (_config.EnableSsl)
//    {
//        ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;
//    }

//    _logger.LogDebug($"Email configuration | Server: {_config.Address} Port: {_config.Port} SSL: {_config.EnableSsl}");

//    message.From = new MailAddress(_config.MailFromAddress, "SM Marketplace");

//    // If Mail Relay is in debug mode set all addresses to the configuration file.
//    if (_config.Debug)
//    {
//        _logger.LogDebug($"Mail relay is in debug mode. Redirecting target email to: {string.Join("|", _config.DebugToAddresses)}");
//        message.To.Clear();
//        foreach (var address in _config.DebugToAddresses)
//        {
//            message.To.Add(address);
//        }
//    }

//    else
//    {
//        message.To.Clear();
//        foreach (var address in _config.ToAddresses)
//        {
//            message.To.Add(address);
//        }
//    }

//    // If the user has setup credentials, use them.
//    if (!string.IsNullOrEmpty(_config.Username) && !string.IsNullOrEmpty(_config.Password))
//    {
//        client.Credentials = new NetworkCredential(_config.Username, _config.Password);
//        _logger.LogDebug("Credentials are set in app settings, will leverage for SMTP connection.");
//    }

//    try
//    {
//        await client.SendMailAsync(message);
//    }
//    catch (Exception ex)
//    {
//        if (ex is SmtpException)
//        {
//            _logger.LogError("An SMTP exception occurred while attempting to relay mail.");
//        }
//        else
//        {
//            _logger.LogError("A general exception occured while attempting to relay mail.");
//        }

//        _logger.LogError(ex.Message);
//        return false;
//    }
//    finally
//    {
//        message.Dispose();
//    }

//    _logger.LogInformation("Message relay complete.");
//    return true;
//}
