using System;

namespace MtgaDecksPro.Cards.Entity.Exceptions
{
    public class CardPoolTypeUnknownException : Exception
    {
        public CardPoolTypeUnknownException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }

    public class CardTypeNotFoundException : Exception
    {
    }

    public class CardNotFoundException : Exception
    {
        public CardNotFoundException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }

    public class FormatNotFoundException : Exception
    {
        public FormatNotFoundException(string format)
            : base(format)
        {
        }
    }

    public class InvalidColorCharsException : Exception
    {
        public InvalidColorCharsException(string chars)
            : base(chars)
        {
        }
    }

    public class InvalidColorPartException : Exception
    {
        public InvalidColorPartException(string part)
            : base(part)
        {
        }
    }

    public class UnknownColorMatchTypeException : Exception
    {
    }
}