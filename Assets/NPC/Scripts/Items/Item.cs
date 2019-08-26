using System;
using System.Collections;
using NPC.Scripts.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace NPC.Scripts.Items
{
    public abstract class Item : MonoBehaviour, IDamageable
    {
        [Header("Pickup")]
        [SerializeField, Range(0f, 30f)] public float pickupDuration = 5f;
        [SerializeField] private AudioClip pickupAudio;
        [Header("References")]
        [SerializeField, Space(10)] protected Slider pickupBar;
        [SerializeField] protected Image pickupBarImage;
        [SerializeField] protected Sprite pickupBarSprite;
        [SerializeField] protected Sprite trapBarSprite;
        [SerializeField, Space(10)]protected GameObject downloadParticleEffect;
        [SerializeField] protected GameObject downloadTrapParticleEffect;
        [SerializeField] private AudioSource audioSource;
        [Header("Item Characteristics")]
        public ItemRarity Rarity;
        public enum ItemRarity
        {
            Common = 60,
            Uncommon = 25,
            Rare = 10,
            UltraRare = 5
        }
        public bool Accessed { get; private set; }
        public bool Trapped { get; set; }

        [HideInInspector]
        public Sprite itemSprite;
        
        private const float SliderMax = 100f;
        private const float SliderMin = 0f;
        private float _t;
        private float _p = SliderMax;
        public bool PickupValid { get; set; }
        private Player accessingPlayer;
        private void Awake()
        {
            itemSprite = GetComponent<SpriteRenderer>().sprite;
            pickupBarImage.sprite = pickupBarSprite;
        }
        public abstract bool Pickup(Character character);
        
        public abstract void Use(Character character);
        
        public abstract void UseWhenTrapped(Character character);

        private void Update()
        {
            if (Accessed) return;
            
            pickupBar.SetValueWithoutNotify(_p / SliderMax);
            switch (PickupValid)
            {
                case true when Math.Abs(_p) > float.Epsilon && accessingPlayer != null:
                    _t += Time.deltaTime / pickupDuration;
                    _p = Mathf.Lerp(SliderMax, SliderMin, _t);
                    PickupValid = VerifyPickup(accessingPlayer);
                    break;
                case true:
                    _t = SliderMin;
                    _p = SliderMin;
                    break;
                default:
                    _t = SliderMin;
                    _p = SliderMax;
                    break;
            }
        }

        private bool VerifyPickup(Player player)
        {
            bool valid = Vector2.Distance(player.transform.position, transform.position) <= player.pickupRange && 
                         player.HoldingPickupButton;
            
            if (!valid) // makes sure you have to stay within range for the whole countdown
            {
                StopAllCoroutines();
            }
            return valid;
        }

        public void PickupItem(Player player)
        {
            PickupValid = true;
            accessingPlayer = player;
            StartCoroutine(PickupDelay(player));
        }

        public void SetTrap()
        {
            Trapped = true;
            pickupBarImage.sprite = trapBarSprite;
        }

        private IEnumerator PickupDelay(Player player)
        {
            yield return new WaitForSeconds(pickupDuration);
            if (!PickupValid)
            {
                yield break;
            }
            // Pickup the item
            Pickup(player);
            
            // Pickup Consequences
            Accessed = true;
            switch (Trapped)
            {
                case true:
                    Instantiate(downloadTrapParticleEffect, transform);
                    break;
                default:
                    Instantiate(downloadParticleEffect, transform);
                    break;
            }
            
            // Messaging
            audioSource.clip = pickupAudio;
            audioSource.Play();
        }

        public void Damage(Vector3 target, Vector2 hitPoint, bool shouldBroadcast)
        {
            pickupBar.SetValueWithoutNotify(SliderMin);
            Accessed = true;
        }
    }
}
