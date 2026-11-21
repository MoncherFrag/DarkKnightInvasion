using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
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

        public const string VictimKingdomId = "vlandia";
        // The invasion's first target: Galend, a Vlandian town on the west coast (id from settlements.xml).
        public const string TargetSettlementId = "town_V5";

        // Placeholder until we ship our own culture and troop tree.
        private const string PlaceholderCultureId = "empire";

        private const uint ClanColorPrimary = 0xFF1A1A1A;   // near-black
        private const uint ClanColorSecondary = 0xFF8B0000; // dark red

        // Army size knobs. TEST VALUES: final balance comes later. Placeholder troops until our own troop tree exists.
        private const int StartingClanTier = 5;
        private const int LieutenantCount = 6;      // ruler + 6 lieutenants = 7 parties in total
        private const float SpawnRadius = 6f;
        private const int StartingGoldPerLord = 300000;
        private const int FoodPerParty = 1200;
        private const int TroopsPerParty = 500;     // total per party, including the lord himself
        private const float EliteFraction = 0.3f;   // share of the top-up that is elite troops
        private const string EliteTroopId = "imperial_elite_cataphract";
        private const string InfantryTroopId = "imperial_legionary";

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

            Settlement target = Settlement.Find(TargetSettlementId);
            if (target == null || target.MapFaction != vlandia)
            {
                return "Error: target settlement '" + TargetSettlementId + "' was not found or is no longer Vlandian.";
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

            // Tier is derived from renown. A higher tier lets the clan field several parties.
            float renownForTier = Campaign.Current.Models.ClanTierModel.GetRequiredRenownForTier(StartingClanTier);
            clan.AddRenown(renownForTier - clan.Renown, false);

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

            // 5) Put troops on the map: the ruler plus lieutenants, each leading a party.
            MobileParty leaderParty = SpawnWarband(leader, target);
            List<MobileParty> followers = new List<MobileParty>();
            for (int i = 0; i < LieutenantCount; i++)
            {
                CharacterObject lieutenantTemplate =
                    culture.LordTemplates[MBRandom.RandomInt(culture.LordTemplates.Count)];
                Hero lieutenant = HeroCreator.CreateSpecialHero(
                    lieutenantTemplate, null, null, null, MBRandom.RandomInt(25, 45));
                lieutenant.Clan = clan;
                followers.Add(SpawnWarband(lieutenant, target));
            }

            // 6) Merge everyone into one besieging army, led by the ruler.
            kingdom.CreateArmy(leader, target, Army.ArmyTypes.Besieger, new MBReadOnlyList<MobileParty>(followers));

            string armyReport = leaderParty.Army != null
                ? "Army formed with " + leaderParty.Army.Parties.Count + " parties."
                : "WARNING: the army was not formed.";

            return "Created kingdom '" + kingdom.Name + "' ruled by '" + leader.Name
                + "'. At war with " + vlandia.Name + ". Target: " + target.Name
                + ". Spawned " + (LieutenantCount + 1) + " war parties. " + armyReport;
        }

        // Turns a hero into the leader of a fresh, supplied party standing near the target.
        private static MobileParty SpawnWarband(Hero lord, Settlement target)
        {
            // A hero must be Active before the game will let him lead a party.
            lord.ChangeState(Hero.CharacterStates.Active);
            lord.ChangeHeroGold(StartingGoldPerLord);

            // The game fills the party from the clan's default party template (our culture's troops).
            MobileParty party = MobilePartyHelper.SpawnLordParty(lord, target.GatePosition, SpawnRadius);

            // Top the party up to exactly TroopsPerParty: some elite, the rest line infantry.
            int missing = TroopsPerParty - party.MemberRoster.TotalManCount;
            if (missing > 0)
            {
                int elite = (int)(missing * EliteFraction);
                AddTroops(party, EliteTroopId, elite);
                AddTroops(party, InfantryTroopId, missing - elite);
            }

            // Without food the army starves long before the siege ends.
            party.ItemRoster.AddToCounts(DefaultItems.Grain, FoodPerParty);
            return party;
        }

        private static void AddTroops(MobileParty party, string troopId, int count)
        {
            CharacterObject troop = MBObjectManager.Instance.GetObject<CharacterObject>(troopId);
            if (troop != null)
            {
                party.AddElementToMemberRoster(troop, count);
            }
        }
    }
}