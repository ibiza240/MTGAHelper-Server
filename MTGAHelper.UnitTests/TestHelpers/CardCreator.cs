using MTGAHelper.Entity;
using System;

namespace MTGAHelper.UnitTests.TestHelpers;

internal static class CardCreator
{
    public static Card WithGrpIdAndName(int grpId, string name)
    {
        return new Card(
            grpId,
            name,
            string.Empty,
            string.Empty,
            Array.Empty<string>(),
            Array.Empty<string>(),
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            true,
            string.Empty,
            0,
            enumLinkedFace.None,
            0,
            false,
            true,
            string.Empty);
    }
}