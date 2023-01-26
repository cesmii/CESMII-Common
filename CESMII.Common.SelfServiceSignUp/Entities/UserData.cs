namespace CESMII.Common.SelfServiceSignUp.Entities
{
    using System.Collections.Generic;

    public class UserData : EntityBase
    {
        public string? DisplayName { get; set; }

        public string? GivenName { get; set; }

        public string? SurName { get; set; }

        public string? Email { get; set; }

        public string? AlternativeSecurityId { get; set; }

        public List<IdentityData>? Identities { get; set; }

        public string? PhoneNumber { get; set; }
        
        public string? InputData { get; set; }

        public string? ApprovalStatus { get; set; }

        public string? Locale { get; set; }
        public string? Organization { get; set; }

        public bool CESMIIMember { get; set; }
    }

    public class IdentityData : EntityBase
    {
        public string? SignInType { get; set; }

        public string? Issuer { get; set; }

        public string? IssuerAssignedId { get; set; }
    }
}
