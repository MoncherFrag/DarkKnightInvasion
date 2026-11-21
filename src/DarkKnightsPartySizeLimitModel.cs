using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace DarkKnightsInvasion
{
    /// <summary>
    /// Makes the Dark Knights immune to the party size limit: no desertion, no morale or speed penalty
    /// for being over the limit. Every other party is left exactly as the game (or another mod) computes it.
    ///
    /// This is a "decorator": it wraps whatever PartySizeLimitModel was registered before it
    /// (BaseModel) and forwards every call. Only GetPartyMemberSizeLimit changes the answer.
    /// We deliberately do NOT inherit DefaultPartySizeLimitModel: that would throw away the War Sails
    /// model (NavalDLC) that sits in the same chain.
    /// </summary>
    public class DarkKnightsPartySizeLimitModel : PartySizeLimitModel
    {
        // Every Dark Knights lord party gets at least this limit. Generous on purpose:
        // the invasion starts at 500 men per party and must have room to grow.
        public const int PartySizeFloor = 1000;

        private static readonly TextObject WarHostText = new TextObject("Dark Knights war host");

        public override ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool isCurrentlyAtSea, bool includeDescriptions = false)
        {
            ExplainedNumber limit = BaseModel.GetPartyMemberSizeLimit(party, isCurrentlyAtSea, includeDescriptions);

            if (party != null && party.IsMobile && DarkKnightsHelper.IsDarkKnightsParty(party.MobileParty)
                && limit.ResultNumber < PartySizeFloor)
            {
                // Top the limit up to the floor; the explanation tooltip shows this as its own line.
                limit.Add(PartySizeFloor - limit.ResultNumber, WarHostText);
            }

            return limit;
        }

        // ---- Everything below is plain forwarding to the wrapped model. ----

        public override int MinimumNumberOfVillagersAtVillagerParty => BaseModel.MinimumNumberOfVillagersAtVillagerParty;

        public override ExplainedNumber GetPartyPrisonerSizeLimit(PartyBase party, bool includeDescriptions = false)
            => BaseModel.GetPartyPrisonerSizeLimit(party, includeDescriptions);

        public override ExplainedNumber CalculateGarrisonPartySizeLimit(Settlement settlement, bool includeDescriptions = false)
            => BaseModel.CalculateGarrisonPartySizeLimit(settlement, includeDescriptions);

        public override int GetClanTierPartySizeEffectForHero(Hero hero)
            => BaseModel.GetClanTierPartySizeEffectForHero(hero);

        public override int GetNextClanTierPartySizeEffectChangeForHero(Hero hero)
            => BaseModel.GetNextClanTierPartySizeEffectChangeForHero(hero);

        public override int GetAssumedPartySizeForLordParty(Hero leaderHero, IFaction partyMapFaction, Clan actualClan)
            => BaseModel.GetAssumedPartySizeForLordParty(leaderHero, partyMapFaction, actualClan);

        public override int GetIdealVillagerPartySize(Village village)
            => BaseModel.GetIdealVillagerPartySize(village);

        public override TroopRoster FindAppropriateInitialRosterForMobileParty(MobileParty party, PartyTemplateObject partyTemplate)
            => BaseModel.FindAppropriateInitialRosterForMobileParty(party, partyTemplate);

        public override List<Ship> FindAppropriateInitialShipsForMobileParty(MobileParty party, PartyTemplateObject partyTemplate)
            => BaseModel.FindAppropriateInitialShipsForMobileParty(party, partyTemplate);
    }
}