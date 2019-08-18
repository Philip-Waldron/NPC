using NPC.Scripts.Characters;
using UnityEngine;

namespace NPC.Scripts.Items
{
    public class Disguise : Item
    {
        [Header("Disguise")]
        [SerializeField, Range(1, 100)] private float disguiseBuff = 20f;
        
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
            switch (Trapped)
            {
                case true:
                    UseWhenTrapped(character);
                    break;
                default:
                    Player player = (Player)character;
                    player.AdjustDisguise(disguiseBuff);
                    break;
            }
        }

        public override void UseWhenTrapped(Character character)
        {
            Player player = (Player)character;
            player.AdjustDisguise(-disguiseBuff);
        }
    }
}