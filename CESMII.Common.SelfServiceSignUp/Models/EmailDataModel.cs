namespace CESMII.Common.SelfServiceSignUp.Models
{

    public class EmailDataModel
    {
        private readonly string _senderEmail;
        private readonly string _senderDisplayName;
        private readonly string _subject;

        public EmailDataModel(UserSignUpModel user, string subject)
        {
            _senderEmail = (user.Email == null) ? "" : user.Email;
            _senderDisplayName = (user.DisplayName == null) ? "" : user.DisplayName;
            _subject = subject;
        }

        public string SenderEmail { get { return _senderEmail; } }
        public string SenderDisplayName { get { return _senderDisplayName; } }
        public string Subject { get { return _subject; } }
    }
}
