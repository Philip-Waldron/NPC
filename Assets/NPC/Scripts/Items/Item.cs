using System;
using System.Collections;
using NPC.Scripts.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace NPC.Scripts.Items
{
    public abstract class Item : MonoBehaviour
    {
        [SerializeField, Range(0f, 30f)] public float pickupDuration = 5f;
        [SerializeField, Space(10)] protected Slider pickupBar;
        [SerializeField] protected GameObject downloadParticleEffect;
        
        protected Sprite itemSprite;
        private const float SliderMax = 100f;
        private const float SliderMin = 0f;
        private float _t;
        private float _p = SliderMax;
        public bool PickupValid { get; set; }
        private Player accessingPlayer;
        private void Awake()
        {
            itemSprite = GetComponent<SpriteRenderer>().sprite;
        }
        public abstract bool Pickup(Character character);
        
        public abstract void Use(Character character);

        private void Update()
        {
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
            bool valid = Vector2.Distance(player.transform.position, transform.position) <= player.pickupRange;
            if (!valid) // makes sure you have to stay within range for the whole countdown
            {
                StopCoroutine(PickupDelay(player));
            }
            return valid;
        }

        public void PickupItem(Player player)
        {
            PickupValid = true;
            accessingPlayer = player;
            StartCoroutine(PickupDelay(player));
        }

        private IEnumerator PickupDelay(Player player)
        {
            yield return new WaitForSeconds(pickupDuration);
            Pickup(player);
            Instantiate(downloadParticleEffect, transform);
        }
    }
}
