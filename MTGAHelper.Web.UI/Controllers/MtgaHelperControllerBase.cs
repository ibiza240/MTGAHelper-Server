using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using MTGAHelper.Entity.Config.Users;
using MTGAHelper.Lib;
using MTGAHelper.Lib.Config.Users;
using MTGAHelper.Web.Models.Response;
using MTGAHelper.Web.Models.Response.User;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Web.UI.Controllers
{
    public abstract class MtgaHelperControllerBase : ControllerBase
    {
        private readonly CacheSingleton<IReadOnlySet<string>> cacheMembers;
        protected readonly ISessionContainer container;
        protected readonly IConfigManagerUsers configUsers;
        protected string userId;
        protected bool isSupporter;

        //protected IHttpContextAccessor httpContextAccessor;

        protected MtgaHelperControllerBase(
            CacheSingleton<IReadOnlySet<string>> cacheMembers,
            //IHttpContextAccessor httpContextAccessor,
            ISessionContainer container,
            IConfigManagerUsers configUsers
            )
        {
            this.cacheMembers = cacheMembers;
            //this.httpContextAccessor = httpContextAccessor;
            this.container = container;
            this.configUsers = configUsers;
        }

        protected async Task<RegisterUserResponse> RegisterBase()
        {
            userId = User?.Identity.IsAuthenticated == true ? Request.Cookies["userId"] : null;
            //userId = Request.Cookies["userId"];
            SetIsSupporter();

            if (userId == null)
            {
                // Create a userId and set the cookie in the response
                userId = Guid.NewGuid().ToString().Replace("-", "");
                Response.Cookies.Append("userId", userId, new CookieOptions { Expires = DateTime.UtcNow.AddYears(2) });
            }
            //else if (container.GetUserStatus(userId) == UserStatusEnum.NonExistent)
            //    Log.Warning("RegisterUser (userId: {userId}): Cookie was already set but no data was found with that id", userId);

            var referer = Request.Headers.ContainsKey("Referer") ? Request.Headers["Referer"].ToString() : null;
            var changesSinceLastLogin = await container.RegisterUser(userId, referer);
            var configUser = User?.Identity.IsAuthenticated == true ? configUsers.Get(userId) : new ConfigModelUser();

            var email = "";
            if (Request.Cookies.ContainsKey("userEmail") == false)
                Response.Cookies.Append("userEmail", "");
            else
                email = Request.Cookies["userEmail"];

            return new RegisterUserResponse(userId, email, configUser.NbLogin, changesSinceLastLogin, isSupporter)
            {
                NotificationsInactive = configUser.NotificationsInactive,
            };
        }

        private void SetIsSupporter()
        {
            if (User?.Identity.IsAuthenticated != true)
                return;

            var userEmail = Request.Cookies["userEmail"];
            if (string.IsNullOrWhiteSpace(userEmail))
                userEmail = User.Identity.Name;

            isSupporter = IsSupporter(userEmail);
        }

        protected bool IsSupporter(string userEmail)
        {
#if DEBUG
            return true;
#else
            //isSupporter = userEmail != null ? cacheMembers.Get().Any(i => string.Compare(i, userEmail, true) == 0) : false;
            return userEmail != null && cacheMembers.Get().Contains(userEmail.Normalize());
#endif
        }

        protected void StopwatchOperation(string name, Action action)
        {
            var sw = Stopwatch.StartNew();

            action();

            sw.Stop();
            if (sw.ElapsedMilliseconds >= 1000)
                Log.Information("StopwatchOperation {name} {time} s", name, (sw.ElapsedMilliseconds / 1000d).ToString("0.00"));
        }

        protected async Task<T> StopwatchOperation<T>(string name, Func<Task<T>> action)
        {
            var sw = Stopwatch.StartNew();

            var ret = await action();

            sw.Stop();
            if (sw.ElapsedMilliseconds >= 1000)
                Log.Information("StopwatchOperation {name} {time} s", name, (sw.ElapsedMilliseconds / 1000d).ToString("0.00"));

            return ret;
        }

        protected async Task StopwatchOperation(string name, Func<Task> action)
        {
            var sw = Stopwatch.StartNew();

            await action();

            sw.Stop();
            if (sw.ElapsedMilliseconds >= 1000)
                Log.Information("StopwatchOperation {name} {time} s", name, (sw.ElapsedMilliseconds / 1000d).ToString("0.00"));
        }

        protected T StopwatchOperation<T>(string name, Func<T> action)
        {
            var sw = Stopwatch.StartNew();

            var ret = action();

            sw.Stop();
            if (sw.ElapsedMilliseconds >= 1000)
                Log.Information("StopwatchOperation {name} {time} s", name, (sw.ElapsedMilliseconds / 1000d).ToString("0.00"));

            return ret;
        }

        protected async Task<ActionResult<IResponse>> CheckUser()
        {
            userId = Request.Cookies["userId"];
            if (userId == null)
            {
                //Log.Error("CheckUser() failed for user {userId} from {caller}. You must first register.", userId, new StackTrace().GetFrame(1).GetMethod().Name);
                Log.Warning(
                    "CheckUser() called with null user from {caller} [{ip}] - Calling Register",
                    new StackTrace().GetFrame(1).GetMethod().Name,
                    GetRequestIP());
                //return BadRequest(new ErrorResponse($"You must first register."));

                //if (userIdRegistered == null)
                //    throw new Exception("TILT");
                var result = await RegisterBase();

                userId = configUsers.Get(result.UserId).Id;
            }
            else
                SetIsSupporter();

            if (container.GetUserStatus(userId) != UserStatusEnum.InMemory)
            {
                var referer = Request.Headers.ContainsKey("Referer") ? Request.Headers["Referer"].ToString() : null;
                await container.RegisterUser(userId, referer);

                if (container.GetUserStatus(userId) != UserStatusEnum.InMemory)
                {
                    // Bad ending
                    Log.Error("CheckUser() failed for user {userId} from {caller}. User not found.", userId, new StackTrace().GetFrame(1).GetMethod().Name);
                    return NotFound(new ErrorResponse($"User '{userId}' not found."));
                }
            }

            if (User?.Identity.IsAuthenticated == true)
            {
                // Refresh the token
                await HttpContext.SignInAsync(User.Identity.Name, User.Identity.Name, new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddDays(30) });
            }

            // Good ending, userId is valid and available in memory now
            return null;
        }

        internal void SetCookieUserId(string mtgaHelperUserId)
        {
            var hasUserId = Request.Cookies.ContainsKey("userId");
            if (hasUserId == false || Request.Cookies["userId"] != mtgaHelperUserId)
            {
                Response.Cookies.Delete("userId");
                Response.Cookies.Append("userId", mtgaHelperUserId, new CookieOptions { Expires = DateTime.UtcNow.AddYears(2) });
            }
        }

        //protected string GetIPAddress()
        //{
        //    HttpContext.Request.

        //    System.Web.HttpContext context = System.Web.HttpContext.Current;
        //    string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

        //    if (!string.IsNullOrEmpty(ipAddress))
        //    {
        //        string[] addresses = ipAddress.Split(',');
        //        if (addresses.Length != 0)
        //        {
        //            return addresses[0];
        //        }
        //    }

        //    return context.Request.ServerVariables["REMOTE_ADDR"];
        //}

        public string GetRequestIP(bool tryUseXForwardHeader = true)
        {
            T GetHeaderValueAs<T>(string headerName)
            {
                if (/*httpContextAccessor.*/HttpContext?.Request?.Headers?.TryGetValue(headerName, out StringValues values) ?? false)
                {
                    string rawValues = values.ToString();   // writes out as Csv when there are multiple.

                    if (!string.IsNullOrWhiteSpace(rawValues))
                        return (T)Convert.ChangeType(values.ToString(), typeof(T));
                }
                return default(T);
            }

            ICollection<string> SplitCsv(string csvList, bool nullOrWhitespaceInputReturnsNull = false)
            {
                if (string.IsNullOrWhiteSpace(csvList))
                    return nullOrWhitespaceInputReturnsNull ? null : new List<string>();

                return csvList
                    .TrimEnd(',')
                    .Split(',')
                    .AsEnumerable<string>()
                    .Select(s => s.Trim())
                    .ToList();
            }

            string ip = null;

            // todo support new "Forwarded" header (2014) https://en.wikipedia.org/wiki/X-Forwarded-For

            // X-Forwarded-For (csv list):  Using the First entry in the list seems to work
            // for 99% of cases however it has been suggested that a better (although tedious)
            // approach might be to read each IP from right to left and use the first public IP.
            // http://stackoverflow.com/a/43554000/538763
            //
            if (tryUseXForwardHeader)
                ip = SplitCsv(GetHeaderValueAs<string>("X-Forwarded-For")).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(ip))
                ip = GetHeaderValueAs<string>("REMOTE_ADDR");

            // RemoteIpAddress is always null in DNX RC1 Update1 (bug).
            if (string.IsNullOrWhiteSpace(ip) && /*httpContextAccessor.*/HttpContext?.Connection?.RemoteIpAddress != null)
                ip = /*httpContextAccessor.*/HttpContext.Connection.RemoteIpAddress.ToString();

            // _httpContextAccessor.HttpContext?.Request?.Host this is the local host.

            //if (string.IsNullOrWhiteSpace(ip))
            //    throw new Exception("Unable to determine caller's IP.");

            return ip;
        }
    }
}