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
            _logger.LogError("MailRelayService::MailRelayService() - constructor called");
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

        internal async Task<bool> SendEmailSendGrid(MailMessage message)
        {
            bool bReturn = false;
            try
            {
                _config.MailFromAddress = "paul.yao@c-labs.com";
                _config.MailFromAppName = "Profile Designer (Staging)";
                string strApiKey = _config.ApiKey;
                _logger.LogError($"MailRelayService: SendEmailSendGrid [Entry]");
                _logger.LogError($"MailRelayService: From: {_config.MailFromAddress}");
                _logger.LogError($"MailRelayService: AppName: {_config.MailFromAppName}");
                _logger.LogError($"MailRelayService: SendEmailSendGrid [Entry]");
                _logger.LogError($"MailRelayService: SendEmailSendGrid [Entry]");

                var client = new SendGridClient(strApiKey);
                var from = new EmailAddress(_config.MailFromAddress, _config.MailFromAppName);
                var subject = message.Subject;

                _logger.LogError($"MailRelayService: Subject: {subject}");

                var mailTo = new List<EmailAddress>();
                // If Mail Relay is in debug mode set all addresses to the configuration file.
                if (_config.Debug)
                {
                    _logger.LogError($"Mail relay is in debug mode. Redirecting target email to: {string.Join("|", _config.DebugToAddresses)}");
                    foreach (var address in _config.DebugToAddresses)
                    {
                        mailTo.Add(new EmailAddress(address));
                        _logger.LogError($"Adding Email To: {address}");
                    }
                }
                else
                {
                    foreach (var address in _config.ToAddresses)
                    {
                        mailTo.Add(new EmailAddress(address));
                        _logger.LogError($"Adding Email To: {address}");
                    }
                }

                _logger.LogError($"Total Recipient Count = {mailTo.Count}");

                _logger.LogError("MailRelayService: SendEmailSendGrid About to call CreateSingleEmailToMultipleRecipients");

                var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, mailTo, subject, null, message.Body);

                _logger.LogError("MailRelayService: SendEmailSendGrid About to call SendEmailAsync");
                var response = await client.SendEmailAsync(msg);
                if (response == null)
                {
                    _logger.LogError("MailRelayService: SendEmailSendGrid error - response == null");
                }
                else
                {
                    _logger.LogError($"MailRelayService: SendEmailSendGrid Status Code: {response.StatusCode} IsSuccess: {response.IsSuccessStatusCode}");
                    bReturn = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"MailRelayService: Exception -- {ex.Message}");
            }

            return bReturn;
        }

    }
}
