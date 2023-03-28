//// #define LOCALTEST
//namespace CESMII.Common.SelfServiceSignUp
//{
//    using CESMII.Common.SelfServiceSignUp.Models;
//    using CESMII.Common.SelfServiceSignUp.Services;
//    ////////using CESMII.ProfileDesigner.DAL;
//    ////////using CESMII.ProfileDesigner.DAL.Models;
//    using Microsoft.AspNetCore.Mvc;
//    using Microsoft.Extensions.Logging;
//    using SendGrid.Helpers.Mail;
//    using System.Collections;
//    using System.Net;
//    using System.Net.Mail;
//    using System.Threading.Tasks;


//#pragma warning disable 8600, 8602     // Ignore worries about null values.

//    /// <summary>
//    /// This class contains two member functions that serve as the "API Connectors" 
//    /// for an Azure "External Identities" Self-Services Sign-Up.
//    /// 
//    /// As of this writing, there are two types of API Connector functions for Self-Service Sign-Up:
//    /// (1) Self-Sign Up Check Status -- not present.
//    ///     This function gets called after a user (who wants to get added as an external identity
//    ///     to an Azure AD domain) has signed into their home domain. In other words, they prove
//    ///     that they are who they say they are. The "CheckStatus" function lets us review the
//    ///     return information from them logging in, but before they are asked to provide any
//    ///     other user information that might be needed (aka "User Attributes")
//    /// 
//    /// (2) Self-Sign Up Submit -- handled here by the Submit function
//    ///     This function gets called after a user has entered whatever user attributes might
//    ///     be needed to create an external user. We get a chance to review their input and
//    ///     reject anything that is incorrect or reject, for example, any user with missing
//    ///     details.
//    ///     
//    /// The original version of this code supported an approval process that allowed an admin
//    /// to accept or reject each and every request for an external account. This is going to be
//    /// disabled for now, but the code left commented out in case there is a need to add this
//    /// back in.
//    /// </summary>
//    //[ApiController]
//    // [Route("api/SelfServiceSignUp/[action]")]
//    public partial class SelfServiceSignUpNotifyController : Controller
//    {
//        //private readonly MailRelayService _mailService;
//        //////////private readonly UserDAL _dalUser;
//        //protected readonly ILogger<SelfServiceSignUpNotifyController> _logger;

//        //public SelfServiceSignUpNotifyController(
//        //    ILogger<SelfServiceSignUpNotifyController> logger,
//        //    MailRelayService mailservice
//        //    ////////, 
//        //    ////////UserDAL dal
//        //    )
//        //{
//        //    this._logger = logger;
//        //    this._mailService = mailservice;
//        //    ////////this._dalUser = dal;
//        //}

//        // public async Task<IActionResult> Submit([FromBody] SubmitInputModel input)  // Azure AD prefers this - we don't use it to avoid weird custom attribute names.


//        // When running in LOCALTEST mode:
//        // -- We remove the authentication ([SelfSignUpAuth]), since that's a headache to set up and deal with.
//        // -- Test parameters are passed in through a simple string that has some hand-coded JSON.
//        // When running in PRODUCTION mode:
//        // -- Enable authentication
//        // -- No input parameters, since we'll pluck that from the header using runtime magic.

//#if LOCALTEST
//        [HttpPost]
//        [ActionName("submitmarketplace")]

//        // To test -- in line 1, uncomment definition for LOCALTEST, then paste the following JSON into Swagger:
//        // {  "email": "yaopaul@washington.edu",  "identities": [    {      "signInType": "string",      "issuer": "string",      "issuerAssignedId": "string"    }  ],  "displayName": "Paul Yao",  "givenName": "George",  "surName": "Anderson",  "phoneNumber": "(206) 355-9363",  "ui_locales": "string",  "organization": "University of Washington",  "cesmiiMember": "No",  "inputData": "string"}
//        public async Task<IActionResult> SubmitMarketplace(string strInput)            // We for local testing
//        {
//#else
//        //[HttpPost]
//        //[SelfSignUpAuth]
//        //[ActionName("submitmarketplace")]
//        public async Task<IActionResult> SubmitMarketplace()                           // Use for production.
//        {
//            // To avoid weirdly long names for custom user attributes,
//            // read these values in ourselves then smartly parse them.
//            string strInput = String.Empty;
//            using (var reader = new StreamReader(Request.Body))
//            {
//                strInput = await reader.ReadToEndAsync();
//            }
//#endif
//            if (string.IsNullOrEmpty(strInput))
//            {
//                string strError = "Cannot read claims from header.";
//                _logger.LogError(strError);
//                return BadRequest(new ResponseContent("ValidationFailed", strError, HttpStatusCode.BadRequest, action: "ValidationError"));
//            }


//            // We get Json - send it to be parsed in the SubmitInputModel constructor
//            SubmitInputModel simInputValues = null;
//            try
//            {
//                simInputValues = new SubmitInputModel(strInput);

//                if (simInputValues == null)
//                {
//                    string strError = "Can not deserialize simInputValues claims.";
//                    _logger.LogError(strError);
//                    return BadRequest(new ResponseContent("ValidationFailed", strError, HttpStatusCode.BadRequest, action: "ValidationError"));
//                }

//                if (string.IsNullOrEmpty(simInputValues.email))
//                {
//                    string strError = "The value entered for Email is invalid";
//                    _logger.LogError(strError);
//                    return BadRequest(new ResponseContent("EmailEmpty", strError, HttpStatusCode.BadRequest, action: "ValidationError"));
//                }

//                if (string.IsNullOrEmpty(simInputValues.displayName))
//                {
//                    string strError = "The value entered for Display Name is invalid";
//                    _logger.LogError(strError);
//                    return BadRequest(new ResponseContent("DisplayNameEmpty", strError, HttpStatusCode.BadRequest, action: "ValidationError"));
//                }

//                if (string.IsNullOrEmpty(simInputValues.Organization))
//                {
//                    string strError = "The value entered for Organization is invalid";
//                    _logger.LogError(strError);
//                    return BadRequest(new ResponseContent("OrganizationNameEmpty", strError, HttpStatusCode.BadRequest, action: "ValidationError"));
//                }
//            }
//            catch (Exception ex)
//            {
//                string strError = ex.Message.ToString();
//                _logger.LogError($"API Connector exception: {strError}");
//                return BadRequest(new ResponseContent("Exception", strError, HttpStatusCode.BadRequest, action: "ValidationError"));
//            }

//            bool bIsCesmiiMember = false;
//            if (simInputValues.CESMIIMember != null)
//            {
//                bool.TryParse(simInputValues.CESMIIMember, out bIsCesmiiMember);
//            }

//            // Add user to public.user database
//            // Note: This is the first half of collecting user information.
//            //       The other half occurs in the InitLocalUser function, which is found
//            //       here: ProfileDesigner\api\CESMII.ProfileDesigner.Api\Controllers\AuthController.cs
//            Sssu_User_Model um = new Sssu_User_Model()
//            {
//                DisplayName = simInputValues.displayName,
//                Email = simInputValues.email,
//                Organization_Name = simInputValues.Organization,
//                IsCesmiiMember = bIsCesmiiMember,
//            };

//            if (!string.IsNullOrEmpty(simInputValues.givenName))
//                um.FirstName = simInputValues.givenName;

//            if (!string.IsNullOrEmpty(simInputValues.surName))
//                um.LastName = simInputValues.surName;

//            ////////// Search whether we already signed up this user.
//            ////////var listMatchEmailAddress = _dalUser.Where(x => x.EmailAddress.ToLower().Equals(um.Email.ToLower()), null).Data;
//            ////////bool bFirstTime = (listMatchEmailAddress.Count == 0);
//            ////////if (bFirstTime)
//            ////////{
//            ////////    // Add record to database if not previously added.
//            ////////    var id = await _dalUser.AddAsync(um, new UserToken());
//            ////////}

//            bool bFirstTime = true;
/////////            await Sssu_EmailMarketplace(this, simInputValues, um, bFirstTime);

//            _logger.LogInformation($"SelfServiceSignUpNotifyController-Submit: Completed.");

//            // Let's go ahead and create an account for these nice people.
//            return Ok(new ResponseContent(string.Empty, string.Empty, HttpStatusCode.OK, action: "Allow"));
//        }

//        protected const string MARKETPLACE_SIGNUP_SUBJECT = "CESMII | Marketplace | {{Type}}";

//        /// <summary>
//        /// Sssu_EmailMarketplace - Self-Service Sign-Up notify via email that we have a new user.
//        /// </summary>
//        /// <param name="controller"></param>
//        /// <param name="sim"></param>
//        /// <param name="user"></param>
//        /// <returns></returns>
//        //private async Task Sssu_EmailMarketplace(SelfServiceSignUpNotifyController controller, SubmitInputModel sim, Sssu_User_Model user, bool bFirstTime)
//        //{
//        //    // Send email to notify recipient that we have received the cancel publish request
//        //    try
//        //    {
//        //        var strSubject = MARKETPLACE_SIGNUP_SUBJECT.Replace("{{Type}}", "User Sign Up");
//        //        if (!bFirstTime)
//        //            strSubject = MARKETPLACE_SIGNUP_SUBJECT.Replace("{{Type}}", "User Sign Up -- Repeat Customer");
//        //        var emailInfo = new EmailDataModel(user, strSubject);

//        //        // Note: This email template resides in the folders of the CESMII.ProfileDesigner.Api project.
//        //        //       While MVC supports adding new folders in the search for views, the simpler and easier
//        //        //       approach is to put all of these views in the same location. (Your mileage may vary.)
//        //        string strViewName = "~/Views/Template/EmailSignUpNotification.cshtml";
//        //        string strBody = await controller.RenderViewAsync(strViewName, sim);
//        //        if (strBody.Contains("ERROR"))
//        //            throw new Exception("Unable to load email template. Check that files are in the CESMII.ProfileDesigner.Api project folder");

//        //        await SendEmail(emailInfo, strBody);
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        _logger.LogError(ex, $"SelfServiceSignUp -- Notification email for new user {sim.displayName} [{sim.email}] not sent. Message={ex.Message}");
//        //    }
//        //}

//        ///// <summary>
//        ///// SendEmail -- Package and deliver to SendGrid service
//        ///// </summary>
//        //private async Task SendEmail(EmailDataModel emailInfo, string body)
//        //{
//        //    // Setup "To" list 
//        //    // List of recipients for the notification email.
//        //    List<EmailAddress> leaTo = new List<EmailAddress>
//        //    {
//        //        new EmailAddress(emailInfo.SenderEmail, emailInfo.SenderDisplayName)
//        //    };

//        //    // Setup Contents of our email message.
//        //    MailMessage mm = new MailMessage()
//        //    {
//        //        Subject = emailInfo.Subject,
//        //        Body = body
//        //    };

//        //    await _mailService.SendEmailSendGrid(mm, leaTo);
//        //}

//    }
//}