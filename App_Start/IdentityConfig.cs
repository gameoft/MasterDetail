using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using MasterDetail.Models;
using MasterDetail.DataLayer;
using System.Net.Mail;
using System.Configuration;
using System.Net.Mime;
using Twilio;

namespace MasterDetail
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            //da adam
         
            
            
            // Plug in your email service here to send an email.
            //return Task.FromResult(0);

            return configGMailasync(message);
        }

        private Task configGMailasync(IdentityMessage message)
        {
            #region formatter
            //string text = string.Format("Please click on this link to {0}: {1}", message.Subject, message.Body);
            //string text = string.Format("{0}", message.Body);
            //string html = "Please confirm your account by clicking this link: <a href=\"" + message.Body + "\">link</a><br/>";
            //string html = string.Format("{0}", message.Body); 

            //html += HttpUtility.HtmlEncode(@"Or click on the copy the following link on the browser:" + message.Body);
            #endregion

            using (MailMessage msg = new MailMessage())
            {
                msg.From = new MailAddress("no-reply@gameogtrading.com", "Game Of Trading");
                //msg.From = new MailAddress("info.gameoftrading@gmail.com", "Game Of Trading");
                //msg.Sender = new MailAddress("info.gameoftrading@gmail.com");
                //msg.Headers
                msg.IsBodyHtml = true;
                msg.To.Add(new MailAddress(message.Destination));
                msg.Subject = message.Subject;
                //msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
                //msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));

                msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(message.Body, null, MediaTypeNames.Text.Plain));
                msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(message.Body, null, MediaTypeNames.Text.Html));

                using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", Convert.ToInt32(587)))
                {
                    string username = ConfigurationManager.AppSettings["mailAccount"];
                    string pwd = ConfigurationManager.AppSettings["mailPassword"];
                    System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(username, pwd);

                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.UseDefaultCredentials = false; 
                    smtpClient.EnableSsl = true;
                    smtpClient.Credentials = credentials;

                    //return smtpClient.SendMailAsync(msg);
                    smtpClient.Send(msg);
                }
            }
            //TODO try catch se eccezione riprovare

            return Task.FromResult(0);

            //var myMessage = new SendGridMessage();
            //myMessage.AddTo(message.Destination);
            //myMessage.From = new System.Net.Mail.MailAddress(
            //                    "no-reply@gameoftrading.com", "Game Of Trading");
            //myMessage.Subject = message.Subject;
            //myMessage.Text = message.Body;
            //myMessage.Html = message.Body;

            //var credentials = new NetworkCredential(
            //           ConfigurationManager.AppSettings["mailAccount"],
            //           ConfigurationManager.AppSettings["mailPassword"]
            //           );

            //// Create a Web transport for sending email.
            //var transportWeb = new Web(credentials);

            //// Send the email.
            //if (transportWeb != null)
            //{
            //    return transportWeb.DeliverAsync(myMessage);
            //}
            //else
            //{
            //    return Task.FromResult(0);
            //}
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.-> io uso twilio

            const string accountSid = "AC7f7f71a47ff569620580a98e179cdd98";
            const string authToken = "5b3f160be82c883d162d9db3cbfdced9";
            //in italia non sono abilitati a mandare SMS col trial account
            const string phoneNumber = "390294751973";

            var twilioRestClient = new TwilioRestClient(accountSid, authToken);
            var result = twilioRestClient.SendSmsMessage(phoneNumber, message.Destination, message.Body);

            var debug = result.Status;

            return Task.FromResult(0);
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(1);
            manager.MaxFailedAccessAttemptsBeforeLockout = 3;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = 
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }

    //Configure the role manager used in the application. RoleManager is defined in the ASP.NET Identity Core assembly.
    //manager for the application role class
    public class ApplicationRoleManager : RoleManager<ApplicationRole>
    {
        public ApplicationRoleManager(IRoleStore<ApplicationRole, string> roleStore) :base(roleStore)
        {

        }

        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            return new ApplicationRoleManager(new RoleStore<ApplicationRole>(context.Get<ApplicationDbContext>()));
        }


    }
}
