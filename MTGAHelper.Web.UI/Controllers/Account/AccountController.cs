using AutoMapper;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MTGAHelper.Entity;
using MTGAHelper.Lib;
using MTGAHelper.Web.Models.Request.Account;
using MTGAHelper.Web.Models.Response.Account;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTGAHelper.Web.UI.Controllers.Account
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : MtgaHelperControllerBase
    {
        private readonly IMapper mapper;
        private readonly AccountRepository accountRepository;
        private readonly EmailSender emailSender;

        public AccountController(
            IMapper mapper,
            CacheSingleton<IReadOnlySet<string>> cacheMembers,
            AccountRepository accountRepository,
            EmailSender emailSender)
            : base(cacheMembers, null, null)
        {
            this.mapper = mapper;
            this.accountRepository = accountRepository;
            this.emailSender = emailSender.Init();
        }

        [HttpGet]
        public ActionResult<AccountResponse> GetCurrentAccount()
        {
            if (HttpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var account = accountRepository.GetUserByEmail(HttpContext.User.Identity.GetSubjectId());
                if (account == null)
                    return new AccountResponse();
                else
                    return mapper.Map<AccountResponse>(account).WithAuthenticated();
            }
            else
                return new AccountResponse();
        }

        [HttpGet("Signin")]
        public async Task<ActionResult<AccountResponse>> Signin([FromQuery] string email, [FromQuery] string password)
        {
            var (account, status) = accountRepository.AuthenticateAccount(email, password);
            if (status == AccountRepository.AuthenticateAccountStatusEnum.InvalidEmailPassword)
                return new ResponseBuilder<AccountResponse>().WithError("Invalid email/password").Build();
            else if (status == AccountRepository.AuthenticateAccountStatusEnum.AccountExternalProvider)
                return new ResponseBuilder<AccountResponse>().WithError("This email is not associated with a local account").Build();
            //else if (account.Status == AccountStatusEnum.WaitingForVerificationCode)
            //    return new ResponseBuilder<AccountResponse>(mapper.Map<AccountResponse>(account))
            //        .WithMessage("This email address is still not verified")
            //        .Build();
            else
            {
                return await CompleteLogin(account);
            }
        }

        [Authorize]
        [HttpPost("Signout")]
        public async Task<IActionResult> Signout()
        {
            if (User?.Identity.IsAuthenticated == true)
            {
                Response.Cookies.Delete("userId");
                await HttpContext.SignOutAsync();
            }

            return Redirect("/");

            //// raise the logout event
            //await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));

            //var account = accountRepository.GetUserBySubjectId(User.Identity.GetSubjectId());
            //if (account.ProviderSubjectId == null)
            //    return Redirect("/");
            //else
            //{
            //    //string logoutId = null;

            //    var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
            //    if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
            //    {
            //        var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
            //        if (providerSupportsSignout)
            //        {
            //            // if there's no current logout context, we need to create one
            //            // this captures necessary info from the current logged in user
            //            // before we signout and redirect away to the external IdP for signout
            //            //logoutId = await interaction.CreateLogoutContextAsync();
            //        }
            //    }

            //    //// build a return URL so the upstream provider will redirect back
            //    //// to us after the user has logged out. this allows us to then
            //    //// complete our single sign-out processing.
            //    //string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

            //    //// this triggers a redirect to the external provider for sign-out
            //    //return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);

            //    return SignOut(new AuthenticationProperties { RedirectUri = "/" }, idp);
            //}
        }

        [HttpPost]
        public ActionResult<AccountResponse> Signup([FromBody] AccountSignupRequest req)
        {
            var email = req.Email.Trim();

            var existingAccount = accountRepository.GetUserByEmail(email);
            if (existingAccount != null)
            {
                if (existingAccount.Status == AccountStatusEnum.ForgotPassword)
                {
                    accountRepository.ChangePassword(existingAccount, req.Password);

                    return new ResponseBuilder<AccountResponse>(mapper.Map<AccountResponse>(existingAccount))
                        .WithMessage("Thanks! Your new password has been set and you can now sign-in with it")
                        .Build();
                }
                else
                    return new ResponseBuilder<AccountResponse>().WithError("Email already used").Build();
            }
            else
            {
                var newAccount = accountRepository.CreateAccount(email, req.Password, Request.Cookies["userId"]);
                emailSender.SendVerificationCode(newAccount.Email/*, newAccount.SubjectId*/, accountRepository.EmailToHash(newAccount.Email), newAccount.VerificationCode);

                return new ResponseBuilder<AccountResponse>(mapper.Map<AccountResponse>(newAccount))
                    .WithMessage("Thanks! You can login. Please check your email for a verification code.")
                    .Build();
            }
        }

        [HttpGet("CheckCode")]
        public async Task<IActionResult> CheckVerificationCode([FromQuery] string id, [FromQuery] string code)
        {
            var account = accountRepository.CheckVerificationCode(id, code);
            if (account != null)
                await CompleteLogin(account);

            return Redirect("/");
        }

        private async Task<AccountResponse> CompleteLogin(AccountModel account)
        {
            // issue authentication cookie with subject ID and username
            await HttpContext.SignInAsync(/*account.SubjectId*/account.Email, account.Email, new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddDays(30) });

            SetCookieUserId(account.MtgaHelperUserId);

            if (Request.Cookies.ContainsKey("userEmail")) Response.Cookies.Delete("userEmail");
            Response.Cookies.Append("userEmail", account.Email, new CookieOptions { Expires = DateTime.UtcNow.AddYears(2) });

            return mapper.Map<AccountResponse>(account).WithAuthenticated();
        }

        [HttpPost("ResendVerificationCode")]
        public ActionResult<ResponseBase> ResendVerificationCode([FromBody] AccountSignupRequest req)//[FromBody]ResendVerificationCodeRequest req)
        {
            //if (User?.Identity.IsAuthenticated != true)
            //    return new ResponseBuilder<ResponseBase>().WithError("Invalid id").Build();

            //var emailHash = accountRepository.EmailToHash(User?.Identity?.GetSubjectId());
            var (account, status) = accountRepository.AuthenticateAccount(req.Email, req.Password);

            if (account == null)
                return new ResponseBuilder<ResponseBase>().WithError("Invalid email/password").Build();

            accountRepository.GenerateNewVerificationCode(account);

            emailSender.SendVerificationCode(req.Email, accountRepository.EmailToHash(req.Email), account.VerificationCode);
            return new ResponseBuilder<ResponseBase>(mapper.Map<AccountResponse>(account)).WithMessage("Please check your email for a verification code").Build();
        }

        [HttpGet("RequestPasswordReset")]
        public ActionResult<AccountResponse> RequestPasswordReset([FromQuery] string email)
        {
            var account = accountRepository.RequestPasswordReset(email);
            if (account == null)
                return new ResponseBuilder<AccountResponse>().WithError("Invalid email address").Build();
            else if (account.Provider != null)
                return new ResponseBuilder<AccountResponse>().WithError("This account is not using local authentication").Build();
            else if (account.Status != AccountStatusEnum.Active && account.Status != AccountStatusEnum.ForgotPassword)
                return new ResponseBuilder<AccountResponse>().WithError("This account is not active").Build();

            emailSender.SendResetPassword(account.Email, accountRepository.EmailToHash(email), account.VerificationCode);

            return new ResponseBuilder<AccountResponse>(mapper.Map<AccountResponse>(new AccountModel()))
                .WithMessage("Please check your mailbox for further instructions")
                .Build();
        }

        [HttpGet("ConfirmPasswordReset")]
        public IActionResult ConfirmPasswordReset([FromQuery] string id, [FromQuery] string code)
        {
            accountRepository.ConfirmPasswordReset(id, code);
            return Redirect("/");
        }
    }
}