using System;
using System.Collections;
using NPC.Scripts.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace NPC.Scripts.Pickups
{
    public abstract class BasePickup : MonoBehaviour, IInteractable<Player>, IEquipment
    {
        [SerializeField, Range(0f, 30f)] public float pickupDuration = 5f;
        [SerializeField, Space(10)] protected Slider pickupBar;
        [SerializeField, Space(10)] public Sprite pickupSprite;
        
        protected PickupManager PickupManager;
        protected PlayerManager PlayerManager;

        public Player AccessingPlayer { get; set; }

        private float _t;
        private float _p = 100f;
        
        public bool PickupCountdown { get; set; }

        private void Awake()
        {
            PickupManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PickupManager>();
            PlayerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();
            PickupManager.pickups.Add(this);
        }

        public virtual void Pickup(Player player)
        {
            pickupBar.value = 0f;
            gameObject.GetComponent<ParticleSystem>().Play();
            StartCoroutine(DestroyPickup());
        }

        private void Update()
        {
            pickupBar.SetValueWithoutNotify(_p / 100f);
            
            switch (PickupCountdown)
            {
                case true when Math.Abs(_p) > float.Epsilon:
                    _t += Time.deltaTime / pickupDuration;
                    _p = Mathf.Lerp(100f, 0, _t);
                    if (Vector2.Distance(transform.position, AccessingPlayer.transform.position) > AccessingPlayer.pickupRange)
                    {
                        PickupCountdown = false;
                    }
                    break;
                case true:
                    _p = 0;
                    break;
                case false:
                    _t = 0;
                    _p = 100;
                    break;
            }
        }
        
        private IEnumerator DestroyPickup()
        {
            PickupManager.pickups.Remove(this);
            yield return new WaitForSeconds(2f);
            //Destroy(gameObject);
        }

        public virtual void UseEquipment()
        {
            Debug.Log(name + " used.");
        }
    }
}
