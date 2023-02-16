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
                EnableSsl = _config.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            if (_config.EnableSsl)
            {
                ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;
            }

            _logger.LogDebug($"Email configuration | Server: {_config.Address} Port: {_config.Port} SSL: {_config.EnableSsl}");

            message.From = new MailAddress(_config.MailFromAddress, "SM Marketplace");

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

#pragma warning disable 8600

        public async Task<bool> SendEmailSendGrid(MailMessage message)
        {
            bool bSuccess = false;
            try
            {
                var leaTo = new List<EmailAddress>();
                // If Mail Relay is in debug mode set all addresses to the configuration file.
                if (_config.Debug)
                {
                    _logger.LogInformation($"Mail relay is in debug mode. Redirecting target email to: {string.Join("|", _config.DebugToAddresses)}");
                    foreach (var address in _config.DebugToAddresses)
                    {
                        leaTo.Add(new EmailAddress(address));
                        _logger.LogInformation($"Adding Email To: {address}");
                    }
                }
                else
                {
                    foreach (var address in _config.ToAddresses)
                    {
                        leaTo.Add(new EmailAddress(address));
                        // _logger.LogInformation($"Adding Email To: {address}");
                        _logger.LogError($"Adding Email To: {address}");
                    }
                }


                await SendEmailSendGrid(message, leaTo);

            }
            catch (Exception ex)
            {
                _logger.LogError($"MailRelayService: Exception -- {ex.Message}");
            }

            return bSuccess;
        }

        public async Task<bool> SendEmailSendGrid(MailMessage message, List<EmailAddress> leaTo)
        {
            bool bSuccess = false;

            if (leaTo == null || leaTo.Count == 0)
            {
                _logger.LogError($"SendEmailSendGrid: leaTo is null or empty");
            }
            else
            {
                int i = 0;
                foreach (var address in leaTo)
                {
                    _logger.LogError($"SendEmailSendGrid - leaTo[{i}] = {address} ");
                    i++;
                }
            }

            if (string.IsNullOrEmpty(_config.MailFromAddress))
            {
                _config.MailFromAddress = "paul.yao@c-labs.com";
                _logger.LogError($"SendEmailSendGrid: _config.MailFromAddress was null or empty. Setting to Secret Santa default.");
            }

            if (string.IsNullOrEmpty(_config.MailFromAppName))
            {
                _config.MailFromAppName = "Profile Designer (Staging)";
                _logger.LogError($"SendEmailSendGrid: _config.MailFromAppName was null or empty. Setting to Secret Santa default.");
            }

            string strApiKey = _config.ApiKey;

            var eaFrom = new EmailAddress(_config.MailFromAddress, _config.MailFromAppName);

            if (_config.ToAddresses == null || _config.ToAddresses.Count == 0)
            {
                _logger.LogError($"MailRelayService::SendEmailSendGrid - ToAddresses is null or empty");
            }
            else
            {
                int i = 0;
                foreach (var address in _config.ToAddresses)
                {
                    _logger.LogError($"MailRelayService::SendEmailSendGrid - ToAddresses[{i}] = {address} ");
                    i++;
                }
            }

            try
            {
                _logger.LogError($"MailRelayService::SendEmailSendGrid - APIKey = {strApiKey}");
                _logger.LogError($"MailRelayService::SendEmailSendGrid - Enabled = {_config.Enabled}");
                _logger.LogError($"MailRelayService::SendEmailSendGrid - Debug = {_config.Debug}");
                _logger.LogError($"MailRelayService::SendEmailSendGrid - DebugToAddresses = {_config.DebugToAddresses.ToString()}");
                _logger.LogError($"MailRelayService::SendEmailSendGrid - BaseUrl = {_config.BaseUrl}");
                _logger.LogError($"MailRelayService::SendEmailSendGrid - MailFromAddress = {_config.MailFromAddress}");
                _logger.LogError($"MailRelayService::SendEmailSendGrid - Address = {_config.Address}");
                _logger.LogError($"MailRelayService::SendEmailSendGrid - Port = {_config.Port}");
                _logger.LogError($"MailRelayService::SendEmailSendGrid - EnableSsl = {_config.EnableSsl}");
                _logger.LogError($"MailRelayService::SendEmailSendGrid - Username = {_config.Username}");
                _logger.LogError($"MailRelayService::SendEmailSendGrid - Password = {_config.Password}");
                //_logger.LogError($"MailRelayService::SendEmailSendGrid - TemplateUrls = {_config.TemplateUrls.ToString()}");
                //_logger.LogError($"MailRelayService::SendEmailSendGrid - Provider = {_config.Provider}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"SendEmailSendGrid: Exception in debug code: {ex.Message}");
            }

            var client = new SendGridClient(strApiKey);
            var subject = message.Subject;
            if (string.IsNullOrEmpty(subject))
            {
                _logger.LogError($"SendEmailSendGrid: subject was null or empty.");
            }
            else
            {
                _logger.LogError($"SendEmailSendGrid: subject = {subject}");

            }

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(eaFrom, leaTo, subject, null, message.Body);

            var response = await client.SendEmailAsync(msg);
            if (response == null)
            {
                _logger.LogError($"MailRelayService: SendEmailSendGrid Error. Response is null");
            }
            else
            {
                bSuccess = response.IsSuccessStatusCode;
                if (bSuccess)
                {
                    _logger.LogError($"MailRelayService: SendEmailSendGrid Success! Status Code: {response.StatusCode}");
                }
                else
                {
                    _logger.LogError($"MailRelayService: SendEmailSendGrid Error. Status Code: {response.StatusCode}");
                }
            }

            return bSuccess;
        }
    }
}
