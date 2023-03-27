using CESMII.Common.SelfServiceSignUp.Models;

namespace CESMII.Common.SelfServiceSignUp
{
    ////////using CESMII.ProfileDesigner.DAL.Models;

    public class EmailDataModel
    {
        private readonly string _senderEmail;
        private readonly string _senderDisplayName;
        private readonly string _subject;

        public EmailDataModel(Sssu_User_Model user, string subject)
        {
            _senderEmail = user.Email;
            _senderDisplayName = user.DisplayName;
            _subject = subject;
        }

        public string SenderEmail { get { return _senderEmail; } }
        public string SenderDisplayName { get { return _senderDisplayName; } }
        public string Subject { get { return _subject; } }
    }
}
