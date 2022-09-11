using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    public interface ICombat 
    {
        void TryAttack(Selectable target);
    }
}