﻿using SendGrid.Helpers.Mail;
using System.Net.Mail;

namespace CESMII.Common.SelfServiceSignUp.Services
{
    public interface IMailRelayService
    {
        Task<bool> SendEmail(MailMessage message);
        Task<bool> SendSmtpEmail(MailMessage message);
        Task<bool> SendEmailSendGrid(MailMessage message);
        Task<bool> SendEmailSendGrid(MailMessage message, List<EmailAddress> leaTo, string strCaller = "");
    }
}
