using NPC.Scripts.Characters;

namespace NPC.Scripts.Items
{
    public class Trap : Item
    {
        public override bool Pickup(Character character)
        {
            if (character is Player player)
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
                    player.AddInventoryItem(this, itemSprite);
                    player.trapItem = true;
                    break;
            }
        }
        
        public override void UseWhenTrapped(Character character)
        {
            
        }
    }
}
