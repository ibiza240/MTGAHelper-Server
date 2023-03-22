using AutoMapper;
using Google.Apis.Auth;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MTGAHelper.Entity;
using MTGAHelper.Lib;
using MTGAHelper.Lib.Config.Users;
using MTGAHelper.Lib.OutputLogParser;
using MTGAHelper.Web.Models.Request.Account;
using MTGAHelper.Web.Models.Response.Account;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MTGAHelper.Web.UI.Controllers.Account
{
    [Controller]
    [Route("api/[controller]")]
    public class WpfLoginController : MtgaHelperControllerBase
    {
        private readonly IMapper mapper;
        private readonly AccountRepository accountRepository;

        private XPS x = new XPS();

        public WpfLoginController(
            IMapper mapper,
            CacheSingleton<IReadOnlySet<string>> cacheMembers,
            ISessionContainer container,
            IConfigManagerUsers configUsers,
            AccountRepository accountRepository
            )
            : base(cacheMembers, container, configUsers)
        {
            this.mapper = mapper;
            this.accountRepository = accountRepository;
        }

        [HttpGet("ReturnUrl")]
        public IActionResult ReturnUrl()
        {
            var content = "Invalid user";

            if (HttpContext.User.IsAuthenticated())
            {
                var account = accountRepository.GetUserByEmail(HttpContext.User.Identity.GetSubjectId());
                if (account != null)
                {
                    content = JsonConvert.SerializeObject(mapper.Map<AccountResponse>(account).WithAuthenticated());
                }
            }

            return Content(content, "text/html", Encoding.UTF8);
        }

        [HttpGet("AutoSigninLocalUser")]
        public async Task<AccountResponse> AutoSigninLocalUser([FromQuery] string email, [FromQuery] string hash)
        {
            var account = accountRepository.AutoSigninLocalUser(email, hash);
            if (account == null)
                return new ResponseBuilder<AccountResponse>().WithError("Invalid email/password, could not auto-signin.").Build();
            else
            {
                // issue authentication cookie with subject ID and username
                await HttpContext.SignInAsync(/*account.SubjectId*/account.Email, account.Email, new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddDays(30) });

                SetCookieUserId(account.MtgaHelperUserId);

                return mapper.Map<AccountResponse>(account).WithAuthenticated();
            }
        }

        [HttpPost("ValidateTokenGoogle")]
        public Task<AccountResponse> ValidateTokenGoogle([FromBody] ValidatExternalTokenRequest requestBody)
        {
            return ValidateToken("Google", requestBody);
        }

        [HttpPost("ValidateTokenFacebook")]
        public Task<AccountResponse> ValidateTokenFacebook([FromBody] ValidatExternalTokenRequest requestBody)
        {
            return ValidateToken("Facebook", requestBody);
        }

        public async Task<AccountResponse> ValidateToken(string provider, ValidatExternalTokenRequest requestBody)
        {
            //RegisterBase();

            try
            {
                string email;
                AccountModel user;
                ICollection<Claim> claims = null;
                switch (provider)
                {
                    case "Google":
                        (email, user, claims) = await ValidateTokenGoogle(requestBody.Token);
                        break;

                    case "Facebook":
                        (email, user, claims) = await ValidateTokenFacebook(requestBody.Token);
                        break;

                    default:
                        throw new Exception("Unsupported provider in WpfLoginController.ValidateToken");
                }
                //var principal = new ClaimsPrincipal();
                //principal.AddIdentity(new ClaimsIdentity(claims));

                //var (user, provider, providerSubjectId, claims) = externalUserProcessor.FindUserFromExternalProvider(tokenInfo., result.Properties.Items["scheme"]);
                if (user == null)
                {
                    // this might be where you might initiate a custom workflow for user registration
                    // in this sample we don't show how that would be done, as our sample implementation
                    // simply auto-provisions new external user
                    user = accountRepository.AutoProvisionUser(provider, userId, claims.ToList());
                }

                SetCookieUserId(user.MtgaHelperUserId);

                // issue authentication cookie for user
                await HttpContext.SignInAsync(user.Email, user.Email, new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddDays(30) });//, provider);

                return mapper.Map<AccountResponse>(user).WithAuthenticated();
            }
            catch (InvalidJwtException ex)
            {
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task<(string email, AccountModel user, ICollection<Claim> claims)> ValidateTokenFacebook(string accessToken)
        {
            //var appId = "";
            //var appSecret = "";
            //var url = $"https://graph.facebook.com/oauth/access_token?client_id={appId}&client_secret={appSecret}&grant_type=client_credentials";
            //dynamic jsonData = null;
            //string appToken = null;
            //using (HttpClient client = new HttpClient())
            //{
            //    var strResponse = await client.GetStringAsync(url);
            //    jsonData = JsonConvert.DeserializeObject<dynamic>(strResponse);
            //    appToken = jsonData.access_token;
            //}
            var url = $"https://graph.facebook.com/me?fields=name,email&access_token={accessToken}";
            dynamic jsonData = null;
            using (HttpClient client = new HttpClient())
            {
                var strResponse = await client.GetStringAsync(url);
                jsonData = JsonConvert.DeserializeObject<dynamic>(strResponse);
            }

            var email = jsonData.email.ToString();
            var user = accountRepository.GetUserByEmail(email);
            var claims = new List<Claim>
                {
                    //new Claim(ClaimTypes.NameIdentifier, tokenInfo.Name),
                    //new Claim(ClaimTypes.Name, tokenInfo.Name),
                    //new Claim(JwtRegisteredClaimNames.FamilyName, tokenInfo.FamilyName),
                    //new Claim(JwtRegisteredClaimNames.GivenName, tokenInfo.GivenName),
                    new Claim(JwtRegisteredClaimNames.Email, jsonData.email.ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, jsonData.id.ToString()),
                    new Claim(JwtRegisteredClaimNames.GivenName, jsonData.name.ToString()),
                };

            return (email, user, claims);
        }

        private async Task<(string email, AccountModel user, ICollection<Claim> claims)> ValidateTokenGoogle(string token)
        {
            var tokenInfo = await GoogleJsonWebSignature.ValidateAsync(token,
                new GoogleJsonWebSignature.ValidationSettings { Audience = new[] { x.G().i } });

            var email = tokenInfo.Email;
            var user = accountRepository.GetUserByEmail(email);
            var claims = new List<Claim>
                {
                    //new Claim(ClaimTypes.NameIdentifier, tokenInfo.Name),
                    //new Claim(ClaimTypes.Name, tokenInfo.Name),
                    //new Claim(JwtRegisteredClaimNames.FamilyName, tokenInfo.FamilyName),
                    //new Claim(JwtRegisteredClaimNames.GivenName, tokenInfo.GivenName),
                    new Claim(JwtRegisteredClaimNames.Email, tokenInfo.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, tokenInfo.Subject),
                    new Claim(JwtRegisteredClaimNames.Iss, tokenInfo.Issuer),
                };

            return (email, user, claims);
        }

        [HttpGet("AccountSalt")]
        public string GetAccountSalt()
        {
            if (HttpContext.User.IsAuthenticated())
            {
                var account = accountRepository.GetUserByEmail(HttpContext.User.Identity.GetSubjectId());
                if (account != null)
                {
                    return account.Salt;
                }
            }

            return "Invalid request";
        }
    }
}