using AutoMapper;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Services;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.Config.Users;
using System.Collections.Generic;
using System.Linq;

namespace MTGAHelper.Lib
{
    public class LandsPreferenceManager
    {
        private readonly IMapper mapper;
        private readonly BasicLandIdentifier basicLandIdentifier;
        private readonly ICardRepository cardRepo;
        private readonly IConfigManagerUsers configUsers;

        public LandsPreferenceManager(
            ICardRepository cardRepo,
            IConfigManagerUsers configUsers,
            IMapper mapper,
            BasicLandIdentifier basicLandIdentifier
            )
        {
            this.cardRepo = cardRepo;
            this.configUsers = configUsers;
            this.mapper = mapper;
            this.basicLandIdentifier = basicLandIdentifier;
        }

        public ICollection<CardLandPreferenceDto> GetLandsList(string userId)
        {
            var order = new Dictionary<string, int>()
            {
                { "Plains", 0 },
                { "Island", 1 },
                { "Swamp", 2 },
                { "Mountain", 3 },
                { "Forest", 4 },
                { "Snow-Covered Plains", 0 },
                { "Snow-Covered Island", 1 },
                { "Snow-Covered Swamp", 2 },
                { "Snow-Covered Mountain", 3 },
                { "Snow-Covered Forest", 4 },
            };

            var lands = cardRepo.Values
                .Where(i => basicLandIdentifier.IsBasicLand(i))
                //.Where(i => configApp.StandardSets.Contains(i.set))
                .OrderBy(i => order[i.Name])
                .ToArray();

            var result = mapper.Map<ICollection<CardLandPreferenceDto>>(lands);

            var configUser = configUsers.Get(userId);
            if (configUser?.LandsPreference != null)
            {
                //foreach (var l in configUser.LandsPreference)
                //    result.Single(i => i.GrpId == l).IsSelected = true;
                foreach (var r in result)
                    r.IsSelected = configUser.LandsPreference.Contains(r.GrpId);
            }

            return result;
        }
    }
}