using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CESMII.Common
{
    using Microsoft.Extensions.Configuration;
    using CESMII.Common.SelfServiceSignUp.Models;

    public class ConfigUtil
    {
        private readonly IConfiguration _configuration;

        public ConfigUtil(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        internal MailConfig MailSettings
        {
            get
            {
                var result = new MailConfig();
                _configuration.GetSection("MailSettings").Bind(result);
                return result;
            }
        }
    }
}
