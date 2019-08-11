using System.Collections;
using System.Linq;
using NPC.Scripts.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPC.Scripts.Pickups
{
    public class Scanner : BasePickup
    {
        [SerializeField, Range(1f, 5f), Space(10)] private float scanLineDuration = 3f;
        [SerializeField] private GameObject particleEffect;

        public override void Pickup(Player player)
        {
            base.Pickup(player);
            player.PickupInventoryItem(this, pickupSprite);
            
        }
        
        public override void UseEquipment()
        {
            base.UseEquipment();
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y));
            Vector3 position = AccessingPlayer.transform.position;
            RaycastHit2D[] hits = Physics2D.RaycastAll(position, mousePosition - new Vector2(position.x, position.y), Vector2.Distance(mousePosition, position)).OrderBy(h => h.distance).ToArray();
            Collider2D thisCollider = AccessingPlayer.GetComponent<Collider2D>();
            StartCoroutine(ParticleEffect(AccessingPlayer.transform.position, mousePosition));
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider != thisCollider)
                {
                    Character character = hit.transform.GetComponent<Character>();
                    if (character != null)
                    {
                        character.Scanned();
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private IEnumerator ParticleEffect(Vector2 position, Vector2 target)
        {
            particleEffect = Instantiate(particleEffect);
            var particles = particleEffect.GetComponent<ParticleSystem>();
            particles.Play();
            particleEffect.transform.position = position;
            particleEffect.transform.LookAt(target);
            yield return new WaitForSeconds(particles.main.duration);
            Destroy(particleEffect);
        }
    }
}
