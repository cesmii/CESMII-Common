namespace CESMII.Common.SelfServiceSignUp
{
    using System;
    using Microsoft.AspNetCore.Mvc;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SelfSignUpAuthAttribute : TypeFilterAttribute
    {
        public SelfSignUpAuthAttribute() : base(typeof(SelfSignUpAuthFilter))
        {
            Arguments = new object[] { };
        }
    }
}
