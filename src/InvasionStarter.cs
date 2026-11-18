using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
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
        public const string KingdomStringId = "dki_dark_knights_kingdom";

        private const string VictimKingdomId = "vlandia";

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

            // Validate everything first, then create objects. A half-built invasion is worse than none.
            Kingdom vlandia = Kingdom.All.FirstOrDefault(k => k.StringId == VictimKingdomId);
            if (vlandia == null || vlandia.IsEliminated)
            {
                return "Error: kingdom '" + VictimKingdomId + "' was not found or is eliminated.";
            }

            Settlement target = PickTargetCastle(vlandia);
            if (target == null)
            {
                return "Error: Vlandia owns no castle to attack.";
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

            // A clan without fiefs falls back to its initial home settlement, so give it one.
            clan.SetInitialHomeSettlement(target);

            // SetLeader also moves the hero into the clan.
            clan.SetLeader(leader);

            // Tell the rest of the game a new clan exists.
            CampaignEventDispatcher.Instance.OnClanCreated(clan, false);

            // 3) The kingdom, ruled by our clan. Name, banner and colors come from the clan.
            TextObject kingdomName = new TextObject("{=dki_kingdom_name}Dark Knights");
            Kingdom kingdom = Campaign.Current.KingdomManager.CreateKingdom(
                kingdomName, kingdomName, culture, clan, stringId: KingdomStringId);

            // 4) They arrive already at war with Vlandia.
            DeclareWarAction.ApplyByDefault(kingdom, vlandia);

            return "Created kingdom '" + kingdom.Name + "' ruled by '" + leader.Name
                + "'. At war with " + vlandia.Name + ". Target: " + target.Name + ".";
        }

        // Prefer a castle with a port (for the future sea landing), otherwise any castle.
        private static Settlement PickTargetCastle(Kingdom kingdom)
        {
            List<Settlement> castles = kingdom.Settlements.Where(s => s.IsCastle).ToList();
            if (castles.Count == 0)
            {
                return null;
            }

            List<Settlement> coastal = castles.Where(s => s.HasPort).ToList();
            List<Settlement> pool = coastal.Count > 0 ? coastal : castles;
            return pool[MBRandom.RandomInt(pool.Count)];
        }
    }
}