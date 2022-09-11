using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    public class Turret : Selectable
    {
        public int damage = 420;
        public int defense = 420;

        private void Awake()
        {
            CacheGeneralDataComponents();

        }

    }
}
