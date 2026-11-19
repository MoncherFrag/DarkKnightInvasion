using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
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
            return result;
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("status", "dki")]
        public static string Status(List<string> args)
        {
            string error = "";
            if (!CampaignCheats.CheckCheatUsage(ref error))
            {
                return error;
            }

            Clan clan = null;
            foreach (Clan c in Clan.All)
            {
                if (c.StringId == InvasionStarter.ClanStringId)
                {
                    clan = c;
                    break;
                }
            }

            if (clan == null)
            {
                return "The Dark Knights do not exist yet. Run dki.invade first.";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Dark Knights war parties: " + clan.WarPartyComponents.Count);
            foreach (WarPartyComponent component in clan.WarPartyComponents)
            {
                MobileParty party = component.MobileParty;
                sb.AppendLine(
                    "- " + party.Name
                    + " | troops: " + party.MemberRoster.TotalManCount + "/" + party.Party.PartySizeLimit
                    + " | morale: " + party.Morale.ToString("0")
                    + " | speed: " + party.Speed.ToString("0.0")
                    + " | wage/day: " + party.TotalWage
                    + " | gold: " + (party.LeaderHero != null ? party.LeaderHero.Gold : 0)
                    + " | food days: " + party.GetNumDaysForFoodToLast());
            }
            return sb.ToString();
        }
    }
}