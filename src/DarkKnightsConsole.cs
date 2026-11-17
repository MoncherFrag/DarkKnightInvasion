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

            InformationManager.DisplayMessage(
                new InformationMessage("Dark Knights: invasion komutu calisti (test).", Colors.Red));

            return "dki.invade: komut calisti. Henuz istila baslatmiyor (test surumu).";
        }
    }
}