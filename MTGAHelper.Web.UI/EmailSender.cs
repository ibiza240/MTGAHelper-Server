using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;

namespace MTGAHelper.Web.UI
{
    public class EmailSender
    {
#if DEBUG
        protected const string server = "https://localhost:5001";
#else
        protected const string server = "https://mtgahelper.com";
#endif

        SendGridClient client;

        public EmailSender Init()
        {
            // Put SendGrid api key here
            client = new SendGridClient("");

            return this;
        }

        public async void SendVerificationCode(string to, string emailHash, string verificationCode)
        {
            var link = $"{server}/api/Account/CheckCode?id={emailHash}&code={verificationCode}";

            var mailMessage = new SendGridMessage
            {
                From = new EmailAddress("noreply@mtgahelper.com", "MTGAHelper Email service"),
                Subject = "Welcome to MTGAHelper!",
            };

            mailMessage.AddTos(new List<EmailAddress> { new EmailAddress(to) });

            mailMessage.AddContent(MimeType.Html, $"<p>Welcome to MTGAHelper!</p><p>Please <strong><a href=\"{link}\">click this link</a></strong> to validate your email address.</p><p>To find out more about the tracker and its features, take a look at our <a href=\"https://www.patreon.com/mtgahelper/posts\">posts on Patreon</a>.</p><p>You can also <a href=\"https://discord.gg/GTd3RMd\">join the Discord server</a> for the latest news and community discussions.</p><p>Here's hoping this tracker will improve your experience playing Magic: The Gathering Arena.</p><p>Thanks for using the site!</p>");

            await client.SendEmailAsync(mailMessage);
        }

        public async void SendResetPassword(string to, string emailHash, string verificationCode)
        {
            var link = $"{server}/api/Account/ConfirmPasswordReset?id={emailHash}&code={verificationCode}";

            var mailMessage = new SendGridMessage
            {
                From = new EmailAddress("noreply@mtgahelper.com", "MTGAHelper Email service"),
                Subject = "MTGAHelper account password reset",
            };

            mailMessage.AddTos(new List<EmailAddress> { new EmailAddress(to) });

            mailMessage.AddContent(MimeType.Html, $"<p>A password reset was requested for your MTGAHelper account ({to}).</p><p>You can confirm the request by <strong><a href=\"{link}\">clicking on this link</a></strong>.</p><p>After that, you can use the <strong>Sign-up</strong> form again on the website to specify your email address and a new password.</p><p>Thanks for using the site!</p>");

            await client.SendEmailAsync(mailMessage);
        }
    }
}
