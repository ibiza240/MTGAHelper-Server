using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using IdentityModel;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Lib;
using MTGAHelper.Lib.Config;
using Newtonsoft.Json;
using Serilog;

namespace MTGAHelper.Web.UI
{
    public class AccountRepository
    {
        readonly PasswordHasher passwordHasher;
        //string filePath = "accounts.json";
        readonly string folderDataAccounts;

        public AccountRepository(PasswordHasher passwordHasher, IAccountPath configAccountPath)
        {
            this.passwordHasher = passwordHasher;
            folderDataAccounts = configAccountPath.FolderDataAccounts;
            //if (File.Exists(filePath) == false)
            //    SaveAllAccountsToDisk(new AccountModel[0]);
        }

        //public bool IsAccountExisting(string email)
        //{
        //    var allAccounts = GetAllAccountsFromDisk();
        //    return allAccounts.Any(i => i.Email == email);
        //}

        public void AddAccount(AccountModel account)
        {
            //var allAccounts = GetAllAccountsFromDisk().ToList();
            //allAccounts.Add(account);
            //SaveAllAccountsToDisk(allAccounts);
            SaveAccountToDisk(account);
        }

        //internal AccountModel GetUserBySubjectId(string subjectId)
        //{
        //    var allAccounts = GetAllAccountsFromDisk().ToList();
        //    return allAccounts.FirstOrDefault(i => i.SubjectId == subjectId);
        //}

        internal AccountModel GetUserByEmail(string email)
        {
            if (email == null)
                return null;
            //var allAccounts = GetAllAccountsFromDisk().ToList();
            //return allAccounts.FirstOrDefault(i => i.Email == email);
            var emailHash = EmailToHash(email.ToLower());
            return GetUserByEmailHash(emailHash);
        }

        internal AccountModel GetUserByEmailHash(string emailHash)
        {
            var fileConfig = Path.Join(folderDataAccounts, $"{emailHash}.json");

            if (File.Exists(fileConfig) == false)
                return null;

            var account = JsonConvert.DeserializeObject<AccountModel>(File.ReadAllText(fileConfig));
            if (account.MtgaHelperUserId == null)
            {
                account.MtgaHelperUserId = Guid.NewGuid().ToString().Replace("-", "");
                Log.Error("!!! User id was NULL for email {email}, a new one has been assigned ({newUserId})", account.Email, account.MtgaHelperUserId);
                SaveAccountToDisk(account);
            }

            return account;
        }

        public enum AuthenticateAccountStatusEnum
        {
            InvalidEmailPassword,
            Success,
            AccountExternalProvider
        }

        internal AccountModel AutoSigninLocalUser(string email, string hash)
        {
            var account = GetUserByEmail(email);
            if (account == null || hash != account.PasswordHashed)
                return null;
            else
                return account;
        }

        public (AccountModel model, AuthenticateAccountStatusEnum status) AuthenticateAccount(string email, string password)
        {
            var account = GetUserByEmail(email);

            if (account == null)
                return (null, AuthenticateAccountStatusEnum.InvalidEmailPassword);

            if (account.Salt == null)
                return (null, AuthenticateAccountStatusEnum.AccountExternalProvider);

            var passwordHashed = passwordHasher.Hash(password, account.Salt);
            if (passwordHashed != account.PasswordHashed)
                return (null, AuthenticateAccountStatusEnum.InvalidEmailPassword);

            return (account, AuthenticateAccountStatusEnum.Success);

            //var allAccounts = GetAllAccountsFromDisk().ToList();
            //return allAccounts.FirstOrDefault(i => i.Email == email && i.Password == password);
        }

        public AccountModel CheckVerificationCode(string emailHash, string verificationCode)
        {
            //var allAccounts = GetAllAccountsFromDisk();
            //var validatedAccount = allAccounts.FirstOrDefault(i => i.SubjectId == subjectId && i.VerificationCode == verificationCode);
            var validatedAccount = GetUserByEmailHash(emailHash);
            if (validatedAccount != null && validatedAccount.VerificationCode == verificationCode)
            {
                validatedAccount.Status = AccountStatusEnum.Active;
                //validatedAccount.MtgaHelperUserId = Guid.NewGuid().ToString().Replace("-", "");
                UpdateAccount(validatedAccount);
            }

            return validatedAccount;
        }

        private void UpdateAccount(AccountModel account)
        {
            //var allAccounts = GetAllAccountsFromDisk().ToList();
            //var thisAccount = allAccounts.FirstOrDefault(i => i.SubjectId == account.SubjectId);
            var thisAccount = GetUserByEmail(account.Email);

            if (thisAccount != null)
            {
                //var idx = allAccounts.IndexOf(thisAccount);
                //allAccounts[idx] = account;
                //SaveAllAccountsToDisk(allAccounts);
                SaveAccountToDisk(account);
            }
        }

        internal AccountModel CreateAccount(string email, string password, string userId, bool activateNow = false)
        {
            var (salt, hash) = passwordHasher.GenerateSaltAndHash(password);

            //var userId = Guid.NewGuid().ToString().Replace("-", "");
            var newAccount = new AccountModel
            {
                Email = email,
                PasswordHashed = hash,
                Salt = salt,
                Status = activateNow ? AccountStatusEnum.Active : AccountStatusEnum.WaitingForVerificationCode,
                VerificationCode = Guid.NewGuid().ToString().Replace("-", ""),
                //Username = email,
                MtgaHelperUserId = userId,
                //SubjectId = userId,
            };

            AddAccount(newAccount);

            return newAccount;
        }

        internal AccountModel RequestPasswordReset(string email)
        {
            var account = GetUserByEmail(email);
            if (account != null)
            {
                account.VerificationCode = Guid.NewGuid().ToString().Replace("-", "");
                UpdateAccount(account);
            }

            return account;
        }

        internal void ConfirmPasswordReset(string emailHash, string verificationCode)
        {
            var account = GetUserByEmailHash(emailHash);
            if (account != null && account.VerificationCode == verificationCode)
            {
                account.Status = AccountStatusEnum.ForgotPassword;
                UpdateAccount(account);
            }
        }

        internal void ChangePassword(AccountModel account, string password)
        {
            var (salt, hash) = passwordHasher.GenerateSaltAndHash(password);
            account.PasswordHashed = hash;
            account.Salt = salt;
            account.Status = AccountStatusEnum.Active;
            UpdateAccount(account);
        }

        //ICollection<AccountModel> GetAllAccountsFromDisk()
        //{
        //    var fileContent = File.ReadAllText(filePath);
        //    var accounts = JsonConvert.DeserializeObject<ICollection<AccountModel>>(fileContent);
        //    return accounts;
        //}

        //void SaveAllAccountsToDisk(ICollection<AccountModel> accounts)
        //{
        //    File.WriteAllText(filePath, JsonConvert.SerializeObject(accounts));
        //}

        void SaveAccountToDisk(AccountModel account)
        {
            var emailHash = EmailToHash(account.Email.ToLower());
            File.WriteAllText(Path.Combine(folderDataAccounts, $"{emailHash}.json"), JsonConvert.SerializeObject(account));
        }

        public string EmailToHash(string email)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(email.ToLower());
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                var emailHash = hashBytes.ToBase32String(false);
                return emailHash;
            }
        }

        internal void GenerateNewVerificationCode(AccountModel account)
        {
            account.VerificationCode = Guid.NewGuid().ToString().Replace("-", "");
            UpdateAccount(account);
        }

        //public AccountModel FindByExternalProvider(string provider, string userId)
        //{
        //    var allAccounts = GetAllAccountsFromDisk();
        //    return allAccounts.FirstOrDefault(x => x.ProviderName == provider && x.SubjectId == userId);
        //}

        public AccountModel AutoProvisionUser(string provider, string mtgaHelperUserId, List<Claim> claims)
        {
            (var name, var email) = claims.GetFromClaims();

            var existingAccountWithEmail = GetUserByEmail(email);
            if (existingAccountWithEmail == null)
            {
                // create new user
                var account = new AccountModel
                {
                    //SubjectId = providerSubjectId,
                    //Username = name,
                    Email = email.ToLower(),
                    Provider = provider,
                    //ProviderSubjectId = providerSubjectId,
                    //Claims = filtered
                    MtgaHelperUserId = mtgaHelperUserId,
                    Status = AccountStatusEnum.Active,
                };

                AddAccount(account);
                return account;
            }
            else
            {
                existingAccountWithEmail.Provider = provider;
                //existingAccountWithEmail.ProviderSubjectId = providerSubjectId;
                UpdateAccount(existingAccountWithEmail);
                return existingAccountWithEmail;
            }
        }

        internal void UpdateUserId(string email, string userId)
        {
            var account = GetUserByEmail(email);
            account.MtgaHelperUserId = userId;
            UpdateAccount(account);
        }
    }

    public static class ClaimsData
    {
        public static (string name, string email) GetFromClaims(this IEnumerable<Claim> claims)
        {
            // create a list of claims that we want to transfer into our store
            var filtered = new List<Claim>();

            foreach (var claim in claims)
            {
                // if the external system sends a display name - translate that to the standard OIDC name claim
                if (claim.Type == ClaimTypes.Name)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, claim.Value));
                }
                // if the JWT handler has an outbound mapping to an OIDC claim use that
                else if (JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.ContainsKey(claim.Type))
                {
                    filtered.Add(new Claim(JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap[claim.Type], claim.Value));
                }
                // copy the claim as-is
                else
                {
                    filtered.Add(claim);
                }
            }

            //// if no display name was provided, try to construct by first and/or last name
            //if (!filtered.Any(x => x.Type == JwtClaimTypes.Name))
            //{
            //    var first = filtered.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value;
            //    var last = filtered.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value;
            //    if (first != null && last != null)
            //    {
            //        filtered.Add(new Claim(JwtClaimTypes.Name, first + " " + last));
            //    }
            //    else if (first != null)
            //    {
            //        filtered.Add(new Claim(JwtClaimTypes.Name, first));
            //    }
            //    else if (last != null)
            //    {
            //        filtered.Add(new Claim(JwtClaimTypes.Name, last));
            //    }
            //}

            //// create a new unique subject id
            //var sub = CryptoRandom.CreateUniqueId();

            // check if a display name is available, otherwise fallback to subject id
            var name = filtered.FirstOrDefault(c => c.Type == JwtClaimTypes.Name)?.Value; //?? providerSubjectId;
            var email = filtered.FirstOrDefault(c => c.Type == JwtClaimTypes.Email)?.Value;

            return (name, email);
        }
    }
}
