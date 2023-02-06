
namespace CESMII.Common.SelfServiceSignUp.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class CheckStatusInputModel
    {
        public string? email { get; set; }

        public List<IdentityModel>? identities { get; set; }
    }
}
