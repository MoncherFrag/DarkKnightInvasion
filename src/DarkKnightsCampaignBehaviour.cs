using TaleWorlds.CampaignSystem;

namespace DarkKnightsInvasion
{
    /// <summary>
    /// Hooks our logic into the campaign's event system.
    /// </summary>
    public class DarkKnightsCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.CanKingdomBeDiscontinuedEvent.AddNonSerializedListener(
                this, OnCanKingdomBeDiscontinued);
        }

        // The game asks "can this kingdom be removed?" and we get a chance to say no.
        private void OnCanKingdomBeDiscontinued(Kingdom kingdom, ref bool result)
        {
            if (kingdom.RulingClan != null && kingdom.RulingClan.StringId == InvasionStarter.ClanStringId)
            {
                result = false;
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            // Nothing to save yet: the clan and kingdom themselves live in the save file.
        }
    }
}