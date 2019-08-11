using System.Collections;
using NPC.Scripts.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPC.Scripts.Pickups
{
    public class Sapper : BasePickup
    {
        [SerializeField, Range(1, 10), Space(10)] private float sapperAreaOfEffect = 3f;
        [SerializeField, Range(0f, 1f), Space(10)] private float sapperStrength = .5f;
        [SerializeField] private GameObject particleEffect;
        
        public override void Pickup(Player player)
        {
            base.Pickup(player);
            player.PickupInventoryItem(this, pickupSprite);
        }
        
        public override void UseEquipment()
        {
            base.UseEquipment();
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x,
                Mouse.current.position.ReadValue().y));
            StartCoroutine(ParticleEffect(mousePosition));
            foreach (Player player in PlayerManager.players)
            {
                if (Vector2.Distance(player.transform.position, mousePosition) <= sapperAreaOfEffect)
                {
                    player.Sapped(sapperStrength);
                }
            }
        }

        private IEnumerator ParticleEffect(Vector2 position)
        {
            particleEffect = Instantiate(particleEffect);
            var particles = particleEffect.GetComponent<ParticleSystem>();
            particles.Play();
            particleEffect.transform.position = position;
            yield return new WaitForSeconds(particles.main.duration);
            Destroy(particleEffect);
        }
    }
}