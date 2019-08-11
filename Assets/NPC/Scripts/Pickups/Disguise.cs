﻿using NPC.Scripts.Characters;
using UnityEngine;

namespace NPC.Scripts.Pickups
{
    public class Disguise : BasePickup
    {
        [SerializeField, Range(1, 10), Space(10)] private float disguiseBuff = 2f;
        
        public override void Pickup(Player player)
        {
            base.Pickup(player);
            player.Sapped(disguiseBuff);
        }
    }
}