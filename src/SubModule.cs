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
                new InformationMessage("Dark Knights Invasion yuklendi!", Colors.Green));
        }
    }
}