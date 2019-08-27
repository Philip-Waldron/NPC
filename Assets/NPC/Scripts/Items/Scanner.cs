using System;
using System.Collections;
using System.Linq;
using NPC.Scripts.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPC.Scripts.Items
{
    public class Scanner : Item
    {
        [Header("Scanner")]
        [SerializeField] private float scanDuration = 3f;
        [SerializeField] private float scanRadius = 5f;
        [SerializeField] private float revealDuration = 10f;
        [SerializeField, Space(10)] private GameObject particleEffect;
        [SerializeField] private LayerMask _playerMask;

        private Vector2 scannerPosition;
        private float scanStartTime;
        private bool scanning;
        
        public override bool Pickup(Character character)
        {
            if (character is Player player)
            {
                player.AddInventoryItem(this, itemSprite);
                return true;
            }
            
            return false;
        }

        private void LateUpdate()
        {
            if (!scanning) return;
            
            Collider2D[] characterColliders = Physics2D.OverlapCircleAll(scannerPosition, scanRadius, _playerMask);
            foreach (Collider2D characterCollider in characterColliders)
            {
                Character character = characterCollider.GetComponent<Character>();
                if (character != null && Vector2.Distance(character.transform.position, scannerPosition) <= scanRadius)
                {
                    character.Scan(revealDuration);
                }
            }
            
            scanning = Time.time - scanStartTime <= scanDuration;
        }

        public override void Use(Character character)
        {
            switch (Trapped)
            {
                case true:
                    UseWhenTrapped(character);
                    break;
                default:
                    scannerPosition = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y));
                    ParticleEffect(scannerPosition);
                    scanning = true;
                    scanStartTime = Time.time;
                    
                    // This is the RayCast Method, which sucks
                    /*
                    Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y));
                    Vector3 position = character.transform.position;
                    Collider2D thisCollider = character.GetComponent<Collider2D>();
                    RaycastHit2D[] hits = Physics2D.RaycastAll(position, mousePosition - new Vector2(position.x, position.y), Vector2.Distance(mousePosition, position)).OrderBy(h => h.distance).ToArray();
                    foreach (RaycastHit2D hit in hits)
                    {
                        if (hit.collider != null && hit.collider != thisCollider)
                        {
                            Character hitCharacter = hit.transform.GetComponent<Character>();
                            if (hitCharacter != null && !hitCharacter.IsDead)
                            {
                                hitCharacter.Scan(revealDuration);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    */
                    break;
            }
        }

        public override void UseWhenTrapped(Character character)
        {
            Player player = (Player) character;
            player.Scan(revealDuration);
        }

        private void ParticleEffect(Vector2 position/*, Vector2 target*/)
        {
            particleEffect = Instantiate(particleEffect);
            ParticleSystem particles = particleEffect.GetComponent<ParticleSystem>();
            particles.Stop();
            ParticleSystem.ShapeModule particlesShape = particles.shape;
            ParticleSystem.MainModule particlesMain = particles.main;
            particlesShape.radius = scanRadius;
            particlesMain.duration = scanDuration;
            particleEffect.transform.position = position;
            particles.Play();
            //particleEffect.transform.LookAt(target);
        }
    }
}
