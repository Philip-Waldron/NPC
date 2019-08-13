using System.Collections;
using System.Linq;
using NPC.Scripts.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPC.Scripts.Pickups
{
    public class Scanner : Item
    {
        [SerializeField]
        private float scanLineDuration = 3f;
        [SerializeField]
        private float revealDuration = 10f;
        [SerializeField]
        private GameObject particleEffect;
        private Sprite _itemSprite;

        private void Awake()
        {
            _itemSprite = GetComponent<Sprite>();
        }
        
        public override bool Pickup(Character character)
        {
            if (character is Player player)
            {
                player.AddInventoryItem(this, _itemSprite);
                return true;
            }
            
            return false;
        }
        
        public override void Use(Character character)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y));
            Vector3 position = character.transform.position;
            RaycastHit2D[] hits = Physics2D.RaycastAll(position, mousePosition - new Vector2(position.x, position.y), Vector2.Distance(mousePosition, position)).OrderBy(h => h.distance).ToArray();
            Collider2D thisCollider = character.GetComponent<Collider2D>();
            StartCoroutine(ParticleEffect(character.transform.position, mousePosition));
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
