namespace CESMII.Common.SelfServiceSignUp
{
    using CESMII.Common.SelfServiceSignUp.Models;
    using CESMII.Common.SelfServiceSignUp.Services;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System.Net;
    using System.Net.Mail;
    using System.Text;
    using System.Threading.Tasks;

#pragma warning disable 8600, 8602     // Ignore worries about null values.

    /// <summary>
    /// This class contains two member functions that serve as the "API Connectors" 
    /// for an Azure "External Identities" Self-Services Sign-Up.
    /// 
    /// As of this writing, there are two types of API Connector functions for Self-Service Sign-Up:
    /// (1) Self-Sign Up Check Status -- not present.
    ///     This function gets called after a user (who wants to get added as an external identity
    ///     to an Azure AD domain) has signed into their home domain. In other words, they prove
    ///     that they are who they say they are. The "CheckStatus" function lets us review the
    ///     return information from them logging in, but before they are asked to provide any
    ///     other user information that might be needed (aka "User Attributes")
    /// 
    /// (2) Self-Sign Up Submit -- handled here by the Submit function
    ///     This function gets called after a user has entered whatever user attributes might
    ///     be needed to create an external user. We get a chance to review their input and
    ///     reject anything that is incorrect or reject, for example, any user with missing
    ///     details.
    ///     
    /// The original version of this code supported an approval process that allowed an admin
    /// to accept or reject each and every request for an external account. This is going to be
    /// disabled for now, but the code left commented out in case there is a need to add this
    /// back in.
    /// </summary>
    [ApiController]
    [Route("api/SelfServiceSignUp/[action]")]
    public class SelfServiceSignUpNotifyController : ControllerBase
    {
        private readonly MailRelayService _mailService;

        public SelfServiceSignUpNotifyController(MailRelayService mailservice)
        {
            this._mailService = mailservice;
        }

        [HttpPost]
        //[SelfSignUpAuth]
        [ActionName("submit")]
        // public async Task<IActionResult> Submit(SubmitInputModel input)
        public async Task<IActionResult> Submit(string strInput)
        {
            // We get Json - send it to be parsed in the SubmitInputModel constructor
            SubmitInputModel input = null;
            StringBuilder sb = new StringBuilder();
            try
            {
                input = new SubmitInputModel(strInput);

                sb.AppendLine("Line 57");

                if (input == null)
                {
                    sb.AppendLine("Line 72");
                    return BadRequest(new ResponseContent("ValidationFailed", "Can not deserialize input claims.", HttpStatusCode.BadRequest, action: "ValidationError"));
                }

                if (string.IsNullOrEmpty(input.email))
                {
                    sb.AppendLine("Line 78");
                    return BadRequest(new ResponseContent("EmailEmpty", "The value entered for email is invalid", HttpStatusCode.BadRequest, action: "ValidationError"));
                }

                if (string.IsNullOrEmpty(input.displayName))
                {
                    sb.AppendLine("Line 84");
                    return BadRequest(new ResponseContent("DisplayNameEmpty", "The value entered for display name is invalid", HttpStatusCode.BadRequest, action: "ValidationError"));
                }

                if (string.IsNullOrEmpty(input.OrganizationName))
                {
                    sb.AppendLine("Line 90");
                    return BadRequest(new ResponseContent("OrganizationNameEmpty", "The value entered for organization name is invalid", HttpStatusCode.BadRequest, action: "ValidationError"));
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine(ex.Message.ToString());
            }

            // Send email that we have created a new user account
            string strSender = "paul.yao@c-labs.com";
            string strRecipient = "paul.yao@c-labs.com";
            string strUserName = input.displayName;
            string strUserOrganization =  input.OrganizationName;
            string strOrgCesmiiMember = input.CESMIIMember;
            string strUserEmail = input.email;


            string strSubject = "CESMII New User Sign Up";
            string strContent = $"<p>A new user has signed up as a CESMII.org user.</p>" +
                                $"<p></p>" +
                                $"<p>User Name: <strong>{strUserName}</strong> ({strUserEmail})</p>" +
                                $"<p>Organization: <strong>{strUserOrganization}</strong></p>" +
                                $"<p>CESMII Member? <strong>{strOrgCesmiiMember}</strong></p>" +
                                $"<p></p>" +
                                $"<p>Sincerely,</p>" +
                                $"<p>The Automated Sign-Up System (Updated 1/23/2023 at 12:17pm) </p>" +
                                $"<p></p>";

            if (sb.Length == 0)
            {
                sb.AppendLine("");
                sb.AppendLine("Weird -- nothing here.");
            }


            strContent += sb.ToString();

            MailMessage mm = new MailMessage(strSender, strRecipient, strSubject, strContent);

            await _mailService.SendEmailSendGrid(mm);

            // Let's go ahead and create an account for these nice people.
            return Ok(new ResponseContent(string.Empty, string.Empty, HttpStatusCode.OK, action: "Allow"));
        }

    }
}