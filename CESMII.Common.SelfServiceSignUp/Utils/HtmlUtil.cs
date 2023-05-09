using System.Text.RegularExpressions;

namespace CESMII.Common.SelfServiceSignUp.Utils
{
    public class HtmlUtil
    {
        private static string strAnyHtmlTag = "<[^>]*>";
        public static string RemoveHtmlTags(string strInput)
        {
            MatchCollection mc = Regex.Matches(strInput, strAnyHtmlTag);

            string strValue = strInput;
            foreach (var anitem in mc)
            {
                string? strTag;
                if ((strTag = anitem.ToString()) != null)
                {
                    strValue = strValue.Replace(strTag, "");
                }
            }

            // Capture some common cases.
            strValue = strValue.Replace("&nbsp;", " ");
            strValue = strValue.Replace("&gt;", ">");
            strValue = strValue.Replace("&lt;", "<");
            strValue = strValue.Replace("&amp;", "&");
            strValue = strValue.Replace("&trade;", "™");
            strValue = strValue.Replace("&copy;", "©");

            return strValue;
        }

    }
}
