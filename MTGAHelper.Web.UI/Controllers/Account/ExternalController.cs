﻿using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MTGAHelper.Lib;
using MTGAHelper.Lib.Config.Users;
using MTGAHelper.Web.UI.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MTGAHelper.Web.UI.Controllers.Account
{
    //[SecurityHeaders]
    [AllowAnonymous]
    [Controller]
    public class ExternalController : MtgaHelperControllerBase
    {
        private readonly AccountRepository accountRepository;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;

        //private readonly ILogger<ExternalController> _logger;
        private readonly IEventService _events;

        private ExternalUserProcessor externalUserProcessor;

        public ExternalController(
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IEventService events,
            //ILogger<ExternalController> logger,
            ISessionContainer container,
            IConfigManagerUsers configUsers,
            AccountRepository accountRepository,
            ExternalUserProcessor externalUserProcessor,
            CacheSingleton<IReadOnlySet<string>> cacheMembers
            )
        : base(cacheMembers, container, configUsers)
        {
            // if the TestUserStore is not in DI, then we'll just use the global users collection
            // this is where you would plug in your own custom identity management library (e.g. ASP.NET Identity)
            this.accountRepository = accountRepository;

            _interaction = interaction;
            _clientStore = clientStore;
            //_logger = logger;
            _events = events;
            this.externalUserProcessor = externalUserProcessor;
        }

        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        [HttpGet("/External/Challenge")]
        public IActionResult Challenge([FromQuery] string provider, [FromQuery] string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

            // validate returnUrl - either it is a valid OIDC URL or back to a local page
            if (Url.IsLocalUrl(returnUrl) == false && _interaction.IsValidReturnUrl(returnUrl) == false)
            {
                // user might have clicked on a malicious link - should be logged
                throw new Exception("invalid return URL");
            }

            //if (AccountOptions.WindowsAuthenticationSchemeName == provider)
            //{
            //    // windows authentication needs special handling
            //    return await ProcessWindowsLoginAsync(returnUrl);
            //}
            else
            {
                // start challenge and roundtrip the return URL and scheme
                var props = new AuthenticationProperties
                {
                    RedirectUri = Url.Action(nameof(Callback)),
                    Items =
                    {
                        { "returnUrl", returnUrl },
                        { "scheme", provider },
                    }
                };

                return Challenge(props, provider);
            }
        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        [HttpGet("/External/Callback")]
        public async Task<IActionResult> Callback()
        {
            await RegisterBase();

            // read external identity from the temporary cookie
            var result = await HttpContext.AuthenticateAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            //if (_logger.IsEnabled(LogLevel.Debug))
            //{
            //    var externalClaims = result.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
            //    _logger.LogDebug("External claims: {@claims}", externalClaims);
            //}

            // lookup our user and external provider info
            var (user, provider, providerSubjectId, claims) = externalUserProcessor.FindUserFromExternalProvider(result.Principal, result.Properties.Items["scheme"]);
            if (user == null)
            {
                // this might be where you might initiate a custom workflow for user registration
                // in this sample we don't show how that would be done, as our sample implementation
                // simply auto-provisions new external user
                user = accountRepository.AutoProvisionUser(provider, userId, claims.ToList());
            }

            // this allows us to collect any additonal claims or properties
            // for the specific prtotocols used and store them in the local auth cookie.
            // this is typically used to store data needed for signout from those protocols.
            //var additionalLocalClaims = new List<Claim>();
            //var localSignInProps = new AuthenticationProperties();
            //ProcessLoginCallbackForOidc(result, additionalLocalClaims, localSignInProps);
            //ProcessLoginCallbackForWsFed(result, additionalLocalClaims, localSignInProps);
            //ProcessLoginCallbackForSaml2p(result, additionalLocalClaims, localSignInProps);

            // issue authentication cookie for user
            await HttpContext.SignInAsync(/*user.SubjectId*/user.Email, user.Email, provider, new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddDays(30) });//, localSignInProps, additionalLocalClaims.ToArray());

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme);

            // retrieve return URL
            var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

            // check if external login is in the context of an OIDC request
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerSubjectId, /*user.SubjectId*/user.Email, user.Email, true, context?.ClientId));

            //if (context != null)
            //{
            //    if (await _clientStore.IsPkceClientAsync(context.ClientId))
            //    {
            //        // if the client is PKCE then we assume it's native, so this change in how to
            //        // return the response is for better UX for the end user.
            //        return View("Redirect", new RedirectViewModel { RedirectUrl = returnUrl });
            //    }
            //}

            SetCookieUserId(user.MtgaHelperUserId);

            return Redirect(returnUrl);
        }

        //private async Task<IActionResult> ProcessWindowsLoginAsync(string returnUrl)
        //{
        //    // see if windows auth has already been requested and succeeded
        //    var result = await HttpContext.AuthenticateAsync(AccountOptions.WindowsAuthenticationSchemeName);
        //    if (result?.Principal is WindowsPrincipal wp)
        //    {
        //        // we will issue the external cookie and then redirect the
        //        // user back to the external callback, in essence, treating windows
        //        // auth the same as any other external authentication mechanism
        //        var props = new AuthenticationProperties()
        //        {
        //            RedirectUri = Url.Action("Callback"),
        //            Items =
        //            {
        //                { "returnUrl", returnUrl },
        //                { "scheme", AccountOptions.WindowsAuthenticationSchemeName },
        //            }
        //        };

        //        var id = new ClaimsIdentity(AccountOptions.WindowsAuthenticationSchemeName);
        //        id.AddClaim(new Claim(JwtClaimTypes.Subject, wp.Identity.Name));
        //        id.AddClaim(new Claim(JwtClaimTypes.Name, wp.Identity.Name));

        //        // add the groups as claims -- be careful if the number of groups is too large
        //        if (AccountOptions.IncludeWindowsGroups)
        //        {
        //            var wi = wp.Identity as WindowsIdentity;
        //            var groups = wi.Groups.Translate(typeof(NTAccount));
        //            var roles = groups.Select(x => new Claim(JwtClaimTypes.Role, x.Value));
        //            id.AddClaims(roles);
        //        }

        //        await HttpContext.SignInAsync(
        //            IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme,
        //            new ClaimsPrincipal(id),
        //            props);
        //        return Redirect(props.RedirectUri);
        //    }
        //    else
        //    {
        //        // trigger windows auth
        //        // since windows auth don't support the redirect uri,
        //        // this URL is re-triggered when we call challenge
        //        return Challenge(AccountOptions.WindowsAuthenticationSchemeName);
        //    }
        //}

        private void ProcessLoginCallbackForOidc(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
            // if the external system sent a session id claim, copy it over
            // so we can use it for single sign-out
            var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // if the external provider issued an id_token, we'll keep it for signout
            var id_token = externalResult.Properties.GetTokenValue("id_token");
            if (id_token != null)
            {
                localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = id_token } });
            }
        }

        private void ProcessLoginCallbackForWsFed(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
        }

        private void ProcessLoginCallbackForSaml2p(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
        }
    }
}