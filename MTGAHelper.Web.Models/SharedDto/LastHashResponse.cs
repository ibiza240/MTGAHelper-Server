﻿namespace MTGAHelper.Web.Models.SharedDto
{
    public class LastHashResponse
    {
        public string LastHash { get; set; }

        public LastHashResponse()
        {
        }

        public LastHashResponse(string lastUploadHash)
        {
            LastHash = lastUploadHash;
        }
    }
}