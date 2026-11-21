using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Siege;

namespace DarkKnightsInvasion
{
    /// <summary>
    /// The one question every game model of ours keeps asking: "is this one of the Dark Knights?"
    /// Keeping the answer in a single place means the rule can change in one line later.
    /// </summary>
    internal static class DarkKnightsHelper
    {
        /// <summary>True for lord parties that belong to the Dark Knights clan.</summary>
        public static bool IsDarkKnightsParty(MobileParty party)
        {
            return party != null
                && party.IsLordParty
                && party.ActualClan != null
                && party.ActualClan.StringId == InvasionStarter.ClanStringId;
        }

        /// <summary>True when the besieging camp is led by a Dark Knights party.</summary>
        public static bool IsDarkKnightsSiegeCamp(BesiegerCamp camp)
        {
            return camp != null && IsDarkKnightsParty(camp.LeaderParty);
        }
    }
}