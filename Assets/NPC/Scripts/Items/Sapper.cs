using System.Collections;
using NPC.Scripts.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPC.Scripts.Items
{
    public class Sapper : Item
    {
        [SerializeField]
        private float sapperEffectRadius = 3f;
        [SerializeField]
        private float sapperStrength = 35f;
        [SerializeField]
        private GameObject particleEffect;

        [SerializeField]
        private LayerMask _playerMask;

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
            ParticleSystem particles = particleEffect.GetComponent<ParticleSystem>();
            particles.Play();
            particleEffect.transform.position = position;
            yield return new WaitForSeconds(particles.main.duration);
            Destroy(particleEffect);
        }
    }
}