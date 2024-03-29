﻿namespace CESMII.Common.SelfServiceSignUp.Models
{
    using System.Collections.Generic;
    public class MailConfig
    {
        public bool Enabled { get; set; }

        public bool Debug { get; set; }

        public List<string>? ToAddresses { get; set; }

        public List<string>? DebugToAddresses { get; set; }

        public string? BaseUrl { get; set; }

        public string? FromAddress { get; set; }

        public string? BccAddress { get; set; }

        public string? MailFromAppName { get; set; }

        public string? Address { get; set; }

        public int Port { get; set; }

        public bool EnableSSL { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public List<TemplateUrlsConfig>? TemplateUrls { get; set; }

        public string? Provider { get; set; }

        public string? ApiKey { get; set; }
    }

    public class TemplateUrlsConfig
    {
        public string? Key { get; set; }

        public string? Value { get; set; }
    }
}
