using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

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
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, OnHourlyTick);
        }

        // The game asks "can this kingdom be removed?" and we get a chance to say no.
        private void OnCanKingdomBeDiscontinued(Kingdom kingdom, ref bool result)
        {
            if (kingdom.RulingClan != null && kingdom.RulingClan.StringId == InvasionStarter.ClanStringId)
            {
                result = false;
            }
        }

        // Once per game hour. Left alone, the AI sends army leaders wherever it scores best: raiding
        // villages, or lifting a siege it thinks it cannot win. So we keep the leader locked onto the
        // target until the town is taken.
        private void OnHourlyTick()
        {
            Clan clan = Clan.All.FirstOrDefault(c => c.StringId == InvasionStarter.ClanStringId);
            MobileParty leaderParty = clan?.Leader?.PartyBelongedTo;
            if (leaderParty == null)
            {
                return; // not invaded yet, or the leader is dead / captured / without a party
            }

            Settlement target = Settlement.Find(InvasionStarter.TargetSettlementId);
            bool targetStillEnemy = target != null && target.MapFaction != leaderParty.MapFaction;
            if (!targetStillEnemy)
            {
                // Target taken: hand control back to the normal AI.
                leaderParty.Ai.SetDoNotMakeNewDecisions(false);
                return;
            }

            // TEST AID: the army must not fall apart while the siege drags on (cohesion normally decays daily).
            Army army = leaderParty.Army;
            if (army != null && army.Cohesion < 100f)
            {
                army.Cohesion = 100f;
            }

            // Do not interfere while the army is still gathering or is fighting a battle.
            if (leaderParty.MapEvent != null)
            {
                return;
            }

            if (army != null && army.IsWaitingForArmyMembers())
            {
                return;
            }

            // Lock the AI so it cannot change its mind, then order the siege. Once the siege camp is
            // ready the game itself turns "besiege" into an assault, so the lock can stay on.
            leaderParty.Ai.SetDoNotMakeNewDecisions(true);
            if (leaderParty.DefaultBehavior != AiBehavior.BesiegeSettlement || leaderParty.TargetSettlement != target)
            {
                leaderParty.SetMoveBesiegeSettlement(target, MobileParty.NavigationType.Default);
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            // Nothing to save yet: the clan and kingdom themselves live in the save file.
        }
    }
}