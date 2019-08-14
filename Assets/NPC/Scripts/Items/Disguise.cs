using NPC.Scripts.Characters;
using UnityEngine;

namespace NPC.Scripts.Items
{
    public class Disguise : Item
    {
        [SerializeField, Range(1, 10)]
        private float disguiseBuff = 2f;
        
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
            player.AdjustDisguise(true, disguiseBuff);
        }
    }
}