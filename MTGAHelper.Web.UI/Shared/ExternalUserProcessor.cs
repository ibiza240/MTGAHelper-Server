using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using MTGAHelper.Entity;

namespace MTGAHelper.Web.UI.Shared
{
    public class ExternalUserProcessor
    {
        readonly AccountRepository accountRepository;

        public ExternalUserProcessor(AccountRepository accountRepository)
        {
            this.accountRepository = accountRepository;
        }

        public (AccountModel user, string provider, string providerUserId, IEnumerable<Claim> claims) FindUserFromExternalProvider(ClaimsPrincipal externalUser, string provider)
        {
            // try to determine the unique id of the external user (issued by the provider)
            // the most common claim type for that are the sub claim and the NameIdentifier
            // depending on the external provider, some other claim type might be used
            var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                              externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new Exception("Unknown userid");

            // remove the user id claim so we don't include it as an extra claim if/when we provision the user
            var claims = externalUser.Claims.ToList();
            claims.Remove(userIdClaim);

            var providerUserId = userIdClaim.Value;

            // find external user
            //var user = _users.FindByExternalProvider(provider, providerUserId);
            (var name, var email) = claims.GetFromClaims();
            var user = accountRepository.GetUserByEmail(email);

            return (user, provider, providerUserId, claims);
        }
    }
}
