using System;
using System.Collections.Generic;
using System.Text;

namespace MTGAHelper.Entity.Services
{
    public class BasicLandIdentifier
    {
        public bool IsBasicLand(Card card)
        {
            return card.Type.StartsWith("Basic Land") || card.Type.StartsWith("Basic Snow Land");
        }
    }
}
