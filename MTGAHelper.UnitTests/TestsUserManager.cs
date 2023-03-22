using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Lib;
using MTGAHelper.Lib.Config.Users;
using System.Threading.Tasks;

namespace MTGAHelper.UnitTests
{
    [TestClass]
    public class TestsUserManager : TestsBase
    {
        //[TestMethod]
        //public void TestUserManagerPersistData()
        //{
        //    // Arrange
        //    var userId = "test";
        //    var reader = provider.GetRequiredService<ReaderMtgaOutputLog>();
        //    var userManager = provider.GetRequiredService<UserManager>();
        //    var configUsers = provider.GetRequiredService<IConfigManagerUsers>();

        //    configUsers.Set(new IImmutableUser(userId));

        //    // Act
        //    using (var s = new FileStream(Path.Combine(folderData, "output_log25.1.txt"), FileMode.Open))
        //    {
        //        (var result, var errorId) = reader.LoadFileContent(userId, s);
        //        userManager.SaveNewInfo(userId, result, result);
        //    }
        //    using (var s = new FileStream(Path.Combine(folderData, "output_log26.1.txt"), FileMode.Open))
        //    {
        //        (var result, var errorId) = reader.LoadFileContent(userId, s);
        //        userManager.SaveNewInfo(userId, result, result);
        //    }
        //    using (var s = new FileStream(Path.Combine(folderData, "output_log27.1.txt"), FileMode.Open))
        //    {
        //        (var result, var errorId) = reader.LoadFileContent(userId, s);
        //        userManager.SaveNewInfo(userId, result, result);
        //    }
        //}

        [TestMethod, Ignore("was failing")]
        public async Task TestUserManagerReloadData()
        {
            var userId = "test";

            var userManager = provider.GetRequiredService<UserManager>();
            var configUsers = provider.GetRequiredService<IConfigManagerUsers>();

            await userManager.LoadUser(userId);

            var data = configUsers.Get(userId);
        }
    }
}