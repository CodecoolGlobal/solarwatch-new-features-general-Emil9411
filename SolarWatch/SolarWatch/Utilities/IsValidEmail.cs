using System.Net.Mail;

namespace SolarWatch.Utilities;

public class IsValidEmail
{
    public bool EmailValidation(string email)
    {
        try
        {
            var mailAddress = new MailAddress(email);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}