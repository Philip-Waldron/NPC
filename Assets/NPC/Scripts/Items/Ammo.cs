using NPC.Scripts.Characters;
using UnityEngine;

namespace NPC.Scripts.Items
{
    public class Ammo : Item
    {
        [Header("Ammo")]
        [SerializeField, Range(1, 10)]
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
            Player player = (Player)character;

            switch (Trapped)
            {
                case true:
                    UseWhenTrapped(character);
                    break;
                default:
                    player.AmmoCount += ammoCount;
                    player.AdjustAmmo();
                    break;
            }
        }

        public override void UseWhenTrapped(Character character)
        {
            Player player = (Player)character;
            player.AmmoCount -= ammoCount;
            player.AdjustAmmo();
        }
    }
}
