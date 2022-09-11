using static SurvivalChaos.UpgradeScriptableData;

namespace SurvivalChaos
{
    public interface IUpgrades 
    {
        void TryUpgrade(StatUpgrade upgradeData);
        void CmdInitiateUpgrade(double upgradeValue);
        void ServerSetUpgradeData(double upgradeValue);
    }
}
