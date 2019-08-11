using NPC.Scripts.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPC.Scripts.Pickups
{
    public class Sapper : BasePickup
    {
        [SerializeField, Range(1, 10), Space(10)] private float sapperAreaOfEffect = 3f;
        [SerializeField, Range(0f, 1f), Space(10)] private float sapperStrength = .5f;
        
        public override void Pickup(Player player)
        {
            base.Pickup(player);
            player.PickupInventoryItem(this, pickupSprite);
        }
        
        public override void UseEquipment()
        {
            base.UseEquipment();
            foreach (Player player in PlayerManager.players)
            {
                if(Vector2.Distance(player.transform.position, Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y))) <= sapperAreaOfEffect)
                {
                    player.Sapped(sapperStrength);
                }
            }
        }
    }
}