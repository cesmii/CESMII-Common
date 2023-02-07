
namespace CESMII.Common.SelfServiceSignUp.Models
{
    using CESMII.Common.SelfServiceSignUp.Entities;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;


    /// <summary>
    /// SubmitInputModel - Structure is created with user attributes named for
    /// JSON fields. For standard user fields, this is just fine.
    /// 
    /// For custom, or "extension", attributes, things get a little dicier. The
    /// custom name has the GUI for the creating app embedded within it (minus
    /// the dashes. This applies to the following two fields:
    ///      -- extension_c9d077d37595472ebfc533555830328d_OrganizationName
    ///      -- extension_c9d077d37595472ebfc533555830328d_CESMIIMember
    /// 
    /// The easy way to find the attribute names is to start Powershell, connect
    /// to the Azure AD tenant, then query for the extension attributes as
    /// described here: https://learn.microsoft.com/en-us/powershell/azure/active-directory/using-extension-attributes-sample
    /// 
    /// Short-cut: Get all extension properties defined in
    /// a tenant with the following PowerShell commands:
    ///      C:\> Get-AzureADApplication | Get-AzureADApplicationExtensionProperty
    /// 
    /// </summary>

#pragma warning disable 8600, 8602

    public class SubmitInputModel
    {
        public string? email { get; set; }

        public List<IdentityModel>? identities { get; set; }

        public string? displayName { get; set; }

        public string? givenName { get; set; }

        public string? surName { get; set; }

        public string? phoneNumber { get; set; }

        public string? ui_locales { get; set; }

        public string? OrganizationName { get; set; }

        public string? CESMIIMember { get; set; }

        public string? inputData { get; set; }


        public UserData CreateUserData()
        {
            var data = new UserData();
            data.Email = email;
            data.DisplayName = displayName;
            data.GivenName = givenName;
            data.SurName = surName;
            data.PhoneNumber = phoneNumber;
            data.Identities = identities?.Select(x => new IdentityData() { Id = Guid.NewGuid().ToString(), Issuer = x.issuer, IssuerAssignedId = x.issuerAssignedId, SignInType = x.signInType }).ToList();
            data.Locale = ui_locales;
            data.InputData = inputData; 
            data.ApprovalStatus = "Pending";
            data.Organization = OrganizationName;
            data.CESMIIMember = CESMIIMember.ToLower()=="true" ? true : false;

            return data;
        }


        public SubmitInputModel(string strInput)
        {
            if (!string.IsNullOrEmpty(strInput))
            {
                JsonTextReader reader = new JsonTextReader(new StringReader(strInput));

                while (reader.Read())
                {
                    if (reader.Value != null)
                    {
                        // Console.WriteLine($"{iItem}) Token: {reader.TokenType}, Value: {reader.Value}");
                        if (reader.TokenType ==JsonToken.PropertyName && reader.Value != null)
                        {
                            string strName = reader.Value.ToString();
                            if (reader.Read())
                            {
                                if(reader.TokenType == JsonToken.String && reader.Value != null)
                                {
                                    string strValue = reader.Value.ToString();
                                    switch (strName)
                                    { 
                                        case "email": this.email = strValue; break;
                                        case "displayName": this.displayName = strValue; break;
                                        case "givenName": this.givenName = strValue; break;
                                        case "surName": this.surName = strValue; break;
                                        case "phoneNumber": this.phoneNumber = strValue; break;
                                        case "ui_locales": this.ui_locales = strValue; break;
                                        default:
                                            if (strName.EndsWith("_Organization"))
                                                OrganizationName = strValue;
                                            else if (strName.EndsWith("_CESMIIMember"))
                                                CESMIIMember = strValue;
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
