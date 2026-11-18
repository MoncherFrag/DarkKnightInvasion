using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace DarkKnightsInvasion
{
    /// <summary>
    /// Single entry point that starts the invasion. The console command calls it now;
    /// the assassination quest will call it later.
    /// </summary>
    public static class InvasionStarter
    {
        public const string ClanStringId = "dki_dark_knights";

        // Placeholder until we ship our own culture and troop tree.
        private const string PlaceholderCultureId = "empire";

        private const uint ClanColorPrimary = 0xFF1A1A1A;   // near-black
        private const uint ClanColorSecondary = 0xFF8B0000; // dark red

        public static string Start()
        {
            // Idempotency guard: the clan itself is our "invasion already started" flag.
            Clan existing = Clan.All.FirstOrDefault(c => c.StringId == ClanStringId);
            if (existing != null)
            {
                return "The Dark Knights already exist: clan '" + existing.Name
                    + "', leader '" + existing.Leader?.Name + "'.";
            }

            CultureObject culture = MBObjectManager.Instance.GetObject<CultureObject>(PlaceholderCultureId);
            if (culture == null || culture.LordTemplates.Count == 0)
            {
                return "Error: culture '" + PlaceholderCultureId + "' or its lord templates were not found.";
            }

            // 1) Leader hero, built from a random lord template of the culture.
            CharacterObject template = culture.LordTemplates[MBRandom.RandomInt(culture.LordTemplates.Count)];
            Hero leader = HeroCreator.CreateSpecialHero(template, null, null, null, 38);
            leader.SetName(
                new TextObject("{=dki_leader_full_name}Malakar the Black"),
                new TextObject("{=dki_leader_first_name}Malakar"));

            // 2) The clan.
            Clan clan = Clan.CreateClan(ClanStringId);
            TextObject clanName = new TextObject("{=dki_clan_name}Dark Knights");
            clan.ChangeClanName(clanName, clanName);
            clan.Culture = culture;

            int iconId = culture.PossibleClanBannerIconsIDs.FirstOrDefault();
            clan.Banner = Banner.CreateOneColoredBannerWithOneIcon(ClanColorPrimary, ClanColorSecondary, iconId);
            clan.Color = ClanColorPrimary;
            clan.Color2 = ClanColorSecondary;
            clan.IsNoble = true;

            // 3) Wire hero and clan together (same order the game uses for rebel clans).
            clan.SetLeader(leader);
            leader.Clan = clan;

            // 4) Tell the rest of the game a new clan exists.
            CampaignEventDispatcher.Instance.OnClanCreated(clan, false);

            return "Created clan '" + clan.Name + "' led by '" + leader.Name + "'.";
        }
    }
}