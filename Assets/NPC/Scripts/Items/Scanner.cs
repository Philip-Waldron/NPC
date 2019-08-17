using System.Collections;
using System.Linq;
using NPC.Scripts.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPC.Scripts.Items
{
    public class Scanner : Item
    {
        [SerializeField] private float scanLineDuration = 3f;
        [SerializeField] private float revealDuration = 10f;
        [SerializeField] private GameObject particleEffect;

        public override bool Pickup(Character character)
        {
            if (character is Player player)
            {
                player.AddInventoryItem(this, itemSprite);
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
                    Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y));
                    Vector3 position = character.transform.position;
                    Collider2D thisCollider = character.GetComponent<Collider2D>();
            
                    ParticleEffect(position, mousePosition);
            
                    RaycastHit2D[] hits = Physics2D.RaycastAll(position, mousePosition - new Vector2(position.x, position.y), Vector2.Distance(mousePosition, position)).OrderBy(h => h.distance).ToArray();
            
                    foreach (RaycastHit2D hit in hits)
                    {
                        if (hit.collider != null && hit.collider != thisCollider)
                        {
                            Character hitCharacter = hit.transform.GetComponent<Character>();
                            if (hitCharacter != null)
                            {
                                hitCharacter.Scan(revealDuration);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    break;
            }
        }

        public override void UseWhenTrapped(Character character)
        {
            Player player = (Player) character;
            player.Scan(revealDuration);
        }

        private void ParticleEffect(Vector2 position, Vector2 target)
        {
            particleEffect = Instantiate(particleEffect);
            ParticleSystem particles = particleEffect.GetComponent<ParticleSystem>();
            particleEffect.transform.position = position;
            particleEffect.transform.LookAt(target);
        }
    }
}
