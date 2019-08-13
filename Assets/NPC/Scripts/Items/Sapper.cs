using System;
using System.Collections;
using NPC.Scripts.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPC.Scripts.Pickups
{
    public class Sapper : Item
    {
        [SerializeField]
        private float sapperEffectRadius = 3f;
        [SerializeField]
        private float sapperStrength = 35f;
        [SerializeField]
        private GameObject particleEffect;
        private Sprite _itemSprite;
        
        [SerializeField]
        private LayerMask _playerMask;

        private void Awake()
        {
            _itemSprite = GetComponent<Sprite>();
        }

        public override bool Pickup(Character character)
        {
            if (character is Player player)
            {
                player.PickupInventoryItem(this, _itemSprite);
                return true;
            }
            
            return false;
        }
        
        public override void Use(Character character)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x,
                Mouse.current.position.ReadValue().y));
            StartCoroutine(ParticleEffect(mousePosition));
            
            Collider2D[] playerColliders = Physics2D.OverlapCircleAll(transform.position, sapperEffectRadius, _playerMask);
            foreach (var playerCollider in playerColliders)
            {
                Player player = playerCollider.GetComponent<Player>();
                if (Vector2.Distance(player.transform.position, mousePosition) <= sapperEffectRadius)
                {
                    player.AdjustDisguise(true, sapperStrength);
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