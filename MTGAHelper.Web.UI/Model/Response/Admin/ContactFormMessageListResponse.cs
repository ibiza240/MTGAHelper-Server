using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MTGAHelper.Web.UI.Model.Response.Admin
{
    public class ContactFormMessageListResponse
    {
        public ICollection<ContactFormMessageListDto> Messages { get; set; }

        public ContactFormMessageListResponse(ICollection<(string filename, string message)> rawMessages)
        {
            Messages = rawMessages
                .Select(i => new { i.message, parts = i.filename.Split('-') })
                .Where(i => i.parts.Length == 2 && DateTime.TryParseExact(i.parts[0], "yyyyMMdd_HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                .Select(i => 
                {
                    return new ContactFormMessageListDto
                    {
                        DateSent = DateTime.ParseExact(i.parts[0].Replace(".txt", ""), "yyyyMMdd_HHmmss", CultureInfo.InvariantCulture),
                        UserId = i.parts[1],
                        Message = i.message
                    };
                })
                .ToArray();
        }
    }

    public class ContactFormMessageListDto
    {
        public string UserId { get; set; }
        public DateTime DateSent { get; set; }
        public string Message { get; set; }
    }
}
