using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.UnitTests.TestHelpers;
using System.Linq;

namespace MTGAHelper.UnitTests;

[TestClass]
public class TestsCardRepository
{
    private readonly ICardRepository _sut = new CardRepositoryFromDict(
        new[]
        {
            CardCreator.WithGrpIdAndName(05, "Broko"),
            CardCreator.WithGrpIdAndName(10, "Oko"),
            CardCreator.WithGrpIdAndName(20, "Oko, Breaker of Standard"),
            CardCreator.WithGrpIdAndName(30, "Oko, the Trickster"),
            CardCreator.WithGrpIdAndName(40, "Oko, Thief of Clowns"),
            CardCreator.WithGrpIdAndName(50, "Oko, Thief of Crowns"),
            CardCreator.WithGrpIdAndName(60, "Oko, Thief of Crowns"),
            CardCreator.WithGrpIdAndName(70, "Opt"),
        }.ToDictionary(c => c.GrpId));
    
    [TestMethod]
    public void FindStarting_OkoTh_ShouldFindAllMatches()
    {
        _sut.FindNameStartingWith("Oko, Th").Should().HaveCount(4);
    }
    
    [TestMethod]
    public void FindStarting_Oko_ShouldFindAll5Matches()
    {
        _sut.FindNameStartingWith("Oko, ").Should().HaveCount(5);
    }

    [TestMethod]
    public void FindStarting_Oko_ShouldFindAll6Matches()
    {
        _sut.FindNameStartingWith("Oko").Should().HaveCount(6);
    }

    [TestMethod]
    public void FindStarting_O_ShouldFindAllMatches()
    {
        _sut.FindNameStartingWith("O").Should().HaveCount(7);
    }
}