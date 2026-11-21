using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Library;

namespace DarkKnightsInvasion
{
    public static class DarkKnightsConsole
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("invade", "dki")]
        public static string Invade(List<string> args)
        {
            string error = "";
            if (!CampaignCheats.CheckCheatUsage(ref error))
            {
                return error;
            }

            string result = InvasionStarter.Start();
            InformationManager.DisplayMessage(new InformationMessage("Dark Knights: " + result, Colors.Red));

            // The console window is narrow: one sentence per line so nothing gets cut off.
            return result.Replace(". ", ".\n");
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("status", "dki")]
        public static string Status(List<string> args)
        {
            string error = "";
            if (!CampaignCheats.CheckCheatUsage(ref error))
            {
                return error;
            }

            Clan clan = Clan.All.FirstOrDefault(c => c.StringId == InvasionStarter.ClanStringId);
            if (clan == null)
            {
                return "The Dark Knights do not exist yet. Run dki.invade first.";
            }

            StringBuilder sb = new StringBuilder();

            // Balance numbers: our strength next to the victim's.
            Kingdom ours = clan.Kingdom;
            Kingdom victim = Kingdom.All.FirstOrDefault(k => k.StringId == InvasionStarter.VictimKingdomId);
            if (ours != null && victim != null)
            {
                sb.AppendLine("Strength: Dark Knights " + ours.CurrentTotalStrength.ToString("0")
                    + " vs " + victim.Name + " " + victim.CurrentTotalStrength.ToString("0"));
            }
            // The invasion target: who owns it now, and how many defenders are inside.
            Settlement target = Settlement.Find(InvasionStarter.TargetSettlementId);
            if (target != null)
            {
                int garrison = target.Town?.GarrisonParty != null ? target.Town.GarrisonParty.MemberRoster.TotalManCount : 0;
                sb.AppendLine("Target: " + target.Name
                    + " | owner: " + target.OwnerClan?.Name
                    + " | garrison: " + garrison
                    + (target.IsUnderSiege ? " | UNDER SIEGE" : ""));
            }
            // Armies of our kingdom (two lines each, so the console does not cut them off).
            if (ours != null)
            {
                sb.AppendLine("Armies: " + ours.Armies.Count);
                foreach (Army army in ours.Armies)
                {
                    sb.AppendLine("* " + army.Name + " | type: " + army.ArmyType
                        + " | leader behavior: " + army.LeaderParty.DefaultBehavior);
                    sb.AppendLine("    parties: " + army.Parties.Count
                        + " | men: " + army.TotalManCount
                        + " | cohesion: " + army.Cohesion.ToString("0")
                        + " | strength: " + army.EstimatedStrength.ToString("0"));
                    sb.AppendLine("    besieging: "
                        + (army.LeaderParty.BesiegedSettlement != null ? army.LeaderParty.BesiegedSettlement.Name.ToString() : "no")
                        + " | AI locked: " + army.LeaderParty.Ai.DoNotMakeNewDecisions);

                    // Siege progress: why is (or is not) the assault starting?
                    BesiegerCamp camp = army.LeaderParty.BesiegerCamp;
                    if (camp != null)
                    {
                        sb.AppendLine("    siege: preparation done: " + camp.IsPreparationComplete
                            + " | engines built: " + camp.SiegeEngines.AllSiegeEngines().Count(e => e.IsConstructed)
                            + " | ready to assault: " + camp.IsReadyToBesiege);
                    }
                }
            }

            // Every war party (three lines each).
            sb.AppendLine("War parties: " + clan.WarPartyComponents.Count);
            foreach (WarPartyComponent component in clan.WarPartyComponents)
            {
                MobileParty party = component.MobileParty;

                string armyState = party.Army == null
                    ? "no army"
                    : (party.Army.LeaderParty == party ? "army leader"
                        : (party.AttachedTo != null ? "army, attached" : "army, not attached yet"));

                sb.AppendLine("- " + party.Name
                    + " | troops: " + party.MemberRoster.TotalManCount + "/" + party.Party.PartySizeLimit
                    + " | morale: " + party.Morale.ToString("0")
                    + " | speed: " + party.Speed.ToString("0.0"));
                sb.AppendLine("    wage/day: " + party.TotalWage
                    + " | gold: " + (party.LeaderHero != null ? party.LeaderHero.Gold : 0)
                    + " | food days: " + party.GetNumDaysForFoodToLast());
                sb.AppendLine("    " + party.DefaultBehavior
                    + " -> " + (party.TargetSettlement != null ? party.TargetSettlement.Name.ToString() : "none")
                    + " | in settlement: " + (party.CurrentSettlement != null ? party.CurrentSettlement.Name.ToString() : "no")
                    + " | " + armyState);
            }

            return sb.ToString();
        }
    }
}