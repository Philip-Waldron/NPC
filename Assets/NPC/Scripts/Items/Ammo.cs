using NPC.Scripts.Characters;
using UnityEngine;

namespace NPC.Scripts.Pickups
{
    public class Ammo : Item
    {
        [SerializeField, Range(1, 10), Space(10)]
        private int ammoCount = 1;
        
        public override bool Pickup(Character character)
        {
            if (character is Player)
            {
                Use(character);
                return true;
            }
            
            return false;
        }

        public override void Use(Character character)
        {
            var player = (Player)character;
            player.AmmoCount += ammoCount;
            player.AdjustAmmo();
        }
    }
}
