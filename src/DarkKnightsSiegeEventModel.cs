using System.Collections.Generic;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;

namespace DarkKnightsInvasion
{
    /// <summary>
    /// Gives the Dark Knights a head start at a siege so Galend falls before Vlandia's relief armies arrive:
    ///  1) their camp starts with ready-made siege engines (no waiting for carpenters), and
    ///  2) whatever still has to be built is built much faster.
    /// Sieges led by anyone else are untouched.
    /// </summary>
    public class DarkKnightsSiegeEventModel : DefaultSiegeEventModel
    {
        // TEST VALUE: how many times faster the Dark Knights build siege equipment and finish siege preparations.
        private const float ConstructionSpeedMultiplier = 20f;

        public override IEnumerable<SiegeEngineType> GetPrebuiltSiegeEnginesOfSiegeCamp(BesiegerCamp camp)
        {
            // Start from the game's own list (it may already hold e.g. a Ballista from a perk).
            List<SiegeEngineType> engines = new List<SiegeEngineType>(base.GetPrebuiltSiegeEnginesOfSiegeCamp(camp));

            if (DarkKnightsHelper.IsDarkKnightsSiegeCamp(camp))
            {
                // One of each category the siege AI looks at before it dares to assault:
                // melee (ram + tower) and ranged (trebuchet + onager + ballista).
                AddOnce(engines, DefaultSiegeEngineTypes.Ram);
                AddOnce(engines, DefaultSiegeEngineTypes.SiegeTower);
                AddOnce(engines, DefaultSiegeEngineTypes.Trebuchet);
                AddOnce(engines, DefaultSiegeEngineTypes.Onager);
                AddOnce(engines, DefaultSiegeEngineTypes.Ballista);
            }

            return engines;
        }

        public override float GetConstructionProgressPerHour(SiegeEngineType type, SiegeEvent siegeEvent, ISiegeEventSide side)
        {
            float progress = base.GetConstructionProgressPerHour(type, siegeEvent, side);

            if (side.BattleSide == BattleSideEnum.Attacker
                && siegeEvent != null
                && DarkKnightsHelper.IsDarkKnightsSiegeCamp(siegeEvent.BesiegerCamp))
            {
                progress *= ConstructionSpeedMultiplier;
            }

            return progress;
        }

        private static void AddOnce(List<SiegeEngineType> list, SiegeEngineType type)
        {
            if (type != null && !list.Contains(type))
            {
                list.Add(type);
            }
        }
    }
}