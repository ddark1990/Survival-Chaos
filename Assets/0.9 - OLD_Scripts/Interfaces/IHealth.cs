using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    public interface IHealth
    {
        void CmdApplyDamage(int damageToApply);
        void TryDie();
    }
}