using System.Collections;
using NPC.Scripts.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPC.Scripts.Items
{
    public class Sapper : Item
    {
        [SerializeField] private float sapperEffectRadius = 3f;
        [SerializeField] private float sapperEffectDuration = 3f;
        [SerializeField, Range(0, 1)] private float sapperStrength = .5f;
        [SerializeField] private GameObject particleEffect;
        [SerializeField] private LayerMask _playerMask;

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
                    ParticleEffect(mousePosition);
            
                    Collider2D[] playerColliders = Physics2D.OverlapCircleAll(transform.position, sapperEffectRadius, _playerMask);
                    foreach (Collider2D playerCollider in playerColliders)
                    {
                        Player player = playerCollider.GetComponent<Player>();
                        if (Vector2.Distance(player.transform.position, mousePosition) <= sapperEffectRadius)
                        {
                            player.AdjustDisguise(sapperStrength, true);
                        }
                    }
                    break;
            }
        }

        public override void UseWhenTrapped(Character character)
        {
            Player player = (Player) character;
            player.AdjustDisguise(sapperStrength, true);
        }

        private void ParticleEffect(Vector2 position)
        {
            particleEffect = Instantiate(particleEffect);
            ParticleSystem particles = particleEffect.GetComponent<ParticleSystem>();
            ParticleSystem.ShapeModule particlesShape = particles.shape;
            particlesShape.radius = sapperEffectRadius;
            particleEffect.transform.position = position;
        }
    }
}