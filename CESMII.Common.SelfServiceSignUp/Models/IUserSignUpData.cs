namespace CESMII.Common.SelfServiceSignUp.Models
{
    public interface IUserSignUpData
    {
        public int Where(string strEmail);
        public void AddUser(UserSignUpModel user);
    }
   
}
