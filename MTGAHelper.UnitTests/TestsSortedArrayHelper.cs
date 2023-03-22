using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Lib.CardProviders;
using System.Collections.Generic;

namespace MTGAHelper.UnitTests;

[TestClass]
public class TestsSortedArrayHelper
{
    [TestMethod]
    public void TakesAllSevens_None()
    {
        (int, string)[] arr = {(1, "A"), (2, "B"), (3, "C"), (4, "D"), (4, "E"), (4, "F"), (5, "G"), (8, "H")};

        var found = SortedArrayHelper.BinarySearchContiguousEquals(arr, x => x.Item1, 7, Comparer<int>.Default);

        found.Should().BeEmpty();
    }

    [TestMethod]
    public void TakesAllThrees_One()
    {
        (int, string)[] arr = {(1, "A"), (2, "B"), (3, "C"), (4, "D"), (4, "E"), (4, "F"), (5, "G"), (8, "H")};

        var found = SortedArrayHelper.BinarySearchContiguousEquals(arr, x => x.Item1, 3, Comparer<int>.Default);

        found.Should().HaveCount(1);
    }
    
    [TestMethod]
    public void TakesAllFours_DEF()
    {
        (int i, string s)[] arr = {(1, "A"), (2, "B"), (3, "C"), (4, "D"), (4, "E"), (4, "F"), (5, "G"), (8, "H")};

        var found = SortedArrayHelper.BinarySearchContiguousEquals(arr, x => x.i, 4, Comparer<int>.Default);

        found.Should().Equal((4, "D"), (4, "E"), (4, "F"));
    }
    
    [TestMethod]
    public void TakesAllFours_DE()
    {
        (int i, string s)[] arr = {(1, "A"), (2, "B"), (3, "C"), (4, "D"), (4, "E"), (5, "F"), (5, "G"), (8, "H")};

        var found = SortedArrayHelper.BinarySearchContiguousEquals(arr, x => x.i, 4, Comparer<int>.Default);

        found.Should().Equal((4, "D"), (4, "E"));
    }
    
    [TestMethod]
    public void TakesAllFours_EF()
    {
        (int i, string s)[] arr = {(1, "A"), (2, "B"), (3, "C"), (3, "D"), (4, "E"), (4, "F"), (5, "G"), (8, "H")};

        var found = SortedArrayHelper.BinarySearchContiguousEquals(arr, x => x.i, 4, Comparer<int>.Default);

        found.Should().Equal((4, "E"), (4, "F"));
    }
}