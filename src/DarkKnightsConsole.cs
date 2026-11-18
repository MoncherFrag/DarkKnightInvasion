using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
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
    }
}