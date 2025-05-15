namespace CESMII.Common.SelfServiceSignUp.Services
{
    using CESMII.Common;
    using CESMII.Common.SelfServiceSignUp.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using SendGrid;
    using SendGrid.Helpers.Mail;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Mail;
    using System.Text;
    using System.Threading.Tasks;

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
        /// SendEmailSendGrid - Called from Self-Service Sign-Up - But not used now. Hmm
        /// </summary>
        /// <param name="message">The packaged up message we want to send.</param>
        /// <param name="strCaller">String with name of the caller (debug helper - added to log)</param>
        /// <returns></returns>
        public async Task<bool> SendEmailSendGrid(MailMessage message, string strCaller = "")
        {
            _logger.LogError($"{strCaller}MailRelayService: SendEmailSendGrid@40 called (caller = {strCaller})");

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
                    // If message contains "To" recipient(s), use them.
                    var myToList = message.To;
                    if (myToList.Count > 0)
                    {
                        foreach (var myItem in myToList)
                        {
                            if (myItem != null)
                            {
                                var myEmailAddress = new EmailAddress(myItem.Address, myItem.DisplayName);
                                leaTo.Add(myEmailAddress);
                                _logger.LogInformation($"{strCaller}Adding Email To: {myItem.Address} ({myItem.DisplayName})");
                            }
                        }
                    }
                    else
                    {
                        // Otherwise use default configured set of recipients.
                        foreach (var address in _config.ToAddresses)
                        {
                            leaTo.Add(new EmailAddress(address));
                            _logger.LogInformation($"{strCaller}Adding Email To: {address}");
                        }
                    }
                }

                await SendEmailSendGrid(message, leaTo, strCaller);

            }
            catch (Exception ex)
            {
                _logger.LogError($"{strCaller}MailRelayService.SendEmailSendGrid: Exception -- {ex.Message}");
            }

            return bSuccess;
        }

        /// <summary>
        /// SendEmailSendGrid - Send mail to SendGrid
        /// </summary>
        /// <param name="message">Packaged up email message to send.</param>
        /// <param name="leaTo">List of recipients for this email.</param>
        /// <param name="strCaller">String with name of function that called us (debug helper - is written to log)</param>
        /// <returns></returns>
        public async Task<bool> SendEmailSendGrid(MailMessage message, List<EmailAddress> leaTo, string strCaller = "")
        {
            _logger.LogInformation($"{strCaller}MailRelayService.SendEmailSendGrid@85: Entering called (caller = {strCaller})");

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
                    _logger.LogInformation($"{strCaller}SendEmailSendGrid - leaTo[{i}] = {address.Email} ");
                    i++;
                }
            }

            if (string.IsNullOrEmpty(_config.FromAddress))
            {
                _logger.LogError($"{strCaller}SendEmailSendGrid: _config.FromAddress was null or empty. Required value.");
                return false;
            }

            if (string.IsNullOrEmpty(_config.MailFromAppName))
            {
                _config.MailFromAppName = "CESMII";
                _logger.LogInformation($"{strCaller}SendEmailSendGrid: _config.MailFromAppName was null or empty. Setting to CESMII.");
            }

            string strApiKey = _config.ApiKey;
            if (string.IsNullOrEmpty(strApiKey))
            {
                _logger.LogError($"{strCaller}SendEmailSendGrid: _config.ApiKey was null or empty. Not sending.");
                return false;
            }
            var eaFrom = new EmailAddress(_config.FromAddress, _config.MailFromAppName);

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
                _logger.LogInformation($"{strCaller}SendEmailSendGrid: subject was null or empty.");
            }

            _logger.LogInformation($"{strCaller}MailRelayService.SendEmailSendGrid@85: Calling CreateSingleEmailToMultipleRecipients()");
            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(eaFrom, leaTo, subject, null, message.Body);

            if (!string.IsNullOrEmpty(_config.BccAddress))
            {
                msg.AddBcc(_config.BccAddress);
            }

            var myCCList = message.CC;
            if (myCCList.Count > 0)
            {
                foreach (var myItem in myCCList)
                {
                    if (myItem != null)
                    {
                        // SendGrid doesn't like when a CC address is also in the To list.
                        bool bFound = false;
                        var myToList = message.To;
                        foreach (var towhom in myToList)
                        {
                            if (towhom.Address.ToLower() == myItem.Address.ToLower())
                            {
                                bFound = true;
                            }
                        }

                        if (!bFound)
                        {
                            var myEmailAddress = new EmailAddress(myItem.Address, myItem.DisplayName);
                            msg.AddCc(myEmailAddress);
                            _logger.LogInformation($"{strCaller}Adding Cc: {myItem.Address} ({myItem.DisplayName})");
                        }
                    }
                }
            }

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

            //DumpConfigState($"{strCaller}@161: ");

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

                //_config.FromAddress
                strResult = PutStringIntoString(_config.FromAddress);
                sb.Append($"_config.FromAddress: {strResult} $$$ ");

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



        public async Task<bool> SendEmail(MailMessage message)
        {
            switch (_config.Provider)
            {
                case "SMTP":
                    if (!await SendSmtpEmail(message))
                    {
                        return false;
                    }

                    break;
                case "SendGrid":
                    if (!await SendEmailSendGrid(message))
                    {
                        return false;
                    }
                    break;
                default:
                    _logger.LogError("MailRelayService::SendEmail - Provider key value is invalid");
                    throw new InvalidOperationException("The configured email provider is invalid.");
            }
            return true;
        }


        private async Task<bool> SendSmtpEmail(MailMessage message)
        {
            // Do not proceed further if mail relay is disabled.
            if (!_config.Enabled)
            {
                _logger.LogWarning("Mail Relay is disabled.");
                return true;
            }

            // Configure the SMTP client and send the message
            var client = new SmtpClient
            {
                Host = _config.Address,
                Port = _config.Port,

                // Use whatever SSL mode is set.
                EnableSsl = _config.EnableSSL,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            if (_config.EnableSSL)
            {
                // ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;
                _logger.LogDebug(".NET 9 does not support server certificate validation. If you need this done, please configure this on your email server.");
            }

            _logger.LogDebug($"Email configuration | Server: {_config.Address} Port: {_config.Port} SSL: {_config.EnableSSL}");

            message.From = new MailAddress(_config.FromAddress, "SM Marketplace");

            // If Mail Relay is in debug mode set all addresses to the configuration file.
            if (_config.Debug)
            {
                _logger.LogDebug($"Mail relay is in debug mode. Redirecting target email to: {string.Join("|", _config.DebugToAddresses)}");
                message.To.Clear();
                foreach (var address in _config.DebugToAddresses)
                {
                    message.To.Add(address);
                }
            }

            else
            {
                message.To.Clear();
                foreach (var address in _config.ToAddresses)
                {
                    message.To.Add(address);
                }
            }

            // If the user has setup credentials, use them.
            if (!string.IsNullOrEmpty(_config.Username) && !string.IsNullOrEmpty(_config.Password))
            {
                client.Credentials = new NetworkCredential(_config.Username, _config.Password);
                _logger.LogDebug("Credentials are set in app settings, will leverage for SMTP connection.");
            }

            try
            {
                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                if (ex is SmtpException)
                {
                    _logger.LogError("An SMTP exception occurred while attempting to relay mail.");
                }
                else
                {
                    _logger.LogError("A general exception occured while attempting to relay mail.");
                }

                _logger.LogError(ex.Message);
                return false;
            }
            finally
            {
                message.Dispose();
            }

            _logger.LogInformation("Message relay complete.");
            return true;
        }
    }
}
