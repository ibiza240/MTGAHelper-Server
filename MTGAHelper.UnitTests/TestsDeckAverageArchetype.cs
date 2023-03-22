using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MTGAHelper.UnitTests
{
    [TestClass]
    public class TestsDeckAverageArchetype : TestsBase
    {
        string ClassName { get { return nameof(TestsDeckAverageArchetype); } }

        [TestMethod, Ignore("was failing")]
        public void Test_DeckAverageArchetype()
        {
            // Arrange
            LoadTestData(ClassName, nameof(Test_DeckAverageArchetype));

            // Act
            var res = Compare(GetUserData(), new[] { Decks["DeckAverageArchetypeA"] });

            // Assert
            AssertFromExpectedFiles(res);
        }
    }
}
