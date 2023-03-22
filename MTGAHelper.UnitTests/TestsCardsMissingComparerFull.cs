using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Entity;
using MTGAHelper.Lib.CollectionDecksCompare;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTGAHelper.UnitTests
{
    [TestClass]
    public class TestsCardsMissingComparerFull : TestsBase
    {
        string ClassName { get { return nameof(TestsCardsMissingComparerFull); } }

        public TestsCardsMissingComparerFull()
        {
        }

        [TestMethod]
        public void Test_FullDeck()
        {
            // Arrange
            LoadTestData(ClassName, nameof(Test_FullDeck));

            // Act
            var res = Compare(GetUserData(), new[] { Decks["DeckA"] });

            // Assert
            AssertFromExpectedFiles(res);
        }

        [TestMethod]
        public void Test_TwoDecks()
        {
            // Arrange
            LoadTestData(ClassName, nameof(Test_TwoDecks));

            // Act
            var res = Compare(GetUserData(), new[] { Decks["DeckA"], Decks["DeckB"] });

            // Assert
            AssertFromExpectedFiles(res);
        }

        [TestMethod]
        public void Test_RareLand()
        {
            // Arrange
            LoadTestData(ClassName, nameof(Test_RareLand));

            // Act
            var res = Compare(GetUserData(), new[] { Decks["DeckA"], Decks["DeckB"] });

            // Assert
            AssertFromExpectedFiles(res);
        }
    }
}
