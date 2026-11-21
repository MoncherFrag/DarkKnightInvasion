using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace DarkKnightsInvasion
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            InformationManager.DisplayMessage(
                new InformationMessage("Dark Knights Invasion loaded!", Colors.Green));
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (game.GameType is Campaign && gameStarterObject is CampaignGameStarter starter)
            {
                starter.AddBehavior(new DarkKnightsCampaignBehavior());

                // Game models: each AddModel wraps whatever model of that type was registered before it.
                starter.AddModel(new DarkKnightsPartySizeLimitModel());
                starter.AddModel(new DarkKnightsSiegeEventModel());
            }
        }
    }
}