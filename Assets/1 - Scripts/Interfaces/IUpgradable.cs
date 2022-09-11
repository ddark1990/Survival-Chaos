using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    public interface IUpgradable 
    {
        int UpgradeLevel
        {
            get;
            set;
        }
    }
}
