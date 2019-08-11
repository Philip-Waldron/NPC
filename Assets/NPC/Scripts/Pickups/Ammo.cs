using NPC.Scripts.Characters;
using UnityEngine;

namespace NPC.Scripts.Pickups
{
    public class Ammo : BasePickup
    {
        [SerializeField, Range(1, 10), Space(10)] private int ammoCount = 1;
        
        public override void Pickup(Player player)
        {
            base.Pickup(player);
            player.AddAmmo(ammoCount);
        }
    }
}
