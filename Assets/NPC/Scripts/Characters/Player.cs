using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NPC.Scripts.Networking;
using NPC.Scripts.Pickups;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NPC.Scripts.Characters
{
    public class Player : Character, ISap<float>
    {
        [SerializeField, Space(10), Range(1, 3)] private int startAmmo = 1;
        [SerializeField, Range(0f, 100f)] private float startDisguise = 100f;
        [SerializeField, Range(0f, 600f)] private float disguiseDuration = 300f;
        [SerializeField, Space(10)] private Slider disguiseBar;
        [SerializeField] private TextMeshPro bulletCount;
        [SerializeField, Space(10f), Range(.1f, 1f)] private float timeToMove;
        [SerializeField, Range(.5f, 3f)] private float bulletChargeTime;
        [SerializeField, Space(10f), Range(.1f, 3f)] public float pickupRange;
        [SerializeField] private SpriteRenderer inventorySlot;
        [SerializeField, Space(20)] private NetworkPosition networkPosition;

        private readonly List<BasePickup> _inventory = new List<BasePickup>();
        
        private Vector2 _moveDirection;
        
        private bool _moving;
        private bool _bulletCharging;
        private bool _chargeReady;
        
        private int _startBulletChargeFrame;
        private float _chargingFor;
        private float _t;
        
        private LineRenderer _bulletLine;
        
        private Coroutine _lineRendererAnimation;

        private int AmmoCount { get; set; }
        public float Disguise { get; private set; }

        public void IsNotPlayer()
        {
            Destroy(transform.GetComponent<PlayerInput>());
            disguiseBar.gameObject.SetActive(false);
            bulletCount.enabled = false;
        }

        private void Start()
        {
            PlayerManager.players.Add(this);
            PlayerManager.networkPosition.Add(GetComponent<NetworkPosition>());
            AmmoCount = startAmmo;
            Disguise = startDisguise;
            bulletCount.SetText(AmmoCount.ToString());
            SetupLineRenderer();
        }
        private void SetupLineRenderer()
        {
            _bulletLine = gameObject.AddComponent<LineRenderer>();
            _bulletLine.positionCount = 2;
            _bulletLine.startWidth = 0.1f;
            _bulletLine.material = new Material(Shader.Find("Unlit/Color"));
            _bulletLine.material.color = Color.cyan;
            _bulletLine.alignment = LineAlignment.TransformZ;
        }

        private void Update()
        { 
            disguiseBar.SetValueWithoutNotify(Disguise / Max);
            _t += Time.deltaTime / disguiseDuration;
            Disguise = Mathf.Lerp(startDisguise, Min, _t);
            bulletCount.SetText(AmmoCount.ToString());
            
            if (!_moving && networkPosition.MoveDirection != Vector2.zero)
            {
                StartCoroutine(MoveToPosition(transform, transform.position + new Vector3(_moveDirection.x, _moveDirection.y), timeToMove));
            }

            if (_bulletCharging && _startBulletChargeFrame != Time.frameCount)
            {
                _bulletLine.SetPositions(new Vector3[] { transform.position, Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y)) });
                _chargingFor += Time.deltaTime;

                if (_chargingFor >= bulletChargeTime && !_chargeReady)
                {
                    if (_lineRendererAnimation != null)
                    {
                        StopCoroutine(_lineRendererAnimation);
                    }
                
                    _lineRendererAnimation = StartCoroutine(FlashLineRenderer(0.2f, 1, Color.white, Color.cyan));
                    _chargeReady = true;
                }
            }
        }

        private IEnumerator MoveToPosition(Transform targetTransform, Vector3 position, float timeToMove)
        {
            _moving = true;
            Vector3 currentPos = targetTransform.position;
            float currentTime = 0f;
            while(currentTime < 1)
            {
                currentTime += Time.deltaTime / timeToMove;
                targetTransform.position = Vector3.Lerp(currentPos, position, currentTime);
                yield return null;
            }

            if (_moveDirection != Vector2.zero)
            {
                StartCoroutine(MoveToPosition(targetTransform, targetTransform.position + new Vector3(_moveDirection.x, _moveDirection.y), this.timeToMove));
            }
            else
            {
                _moving = false;
            }
        }

        private IEnumerator FlashLineRenderer(float flashTime, int flashCount, Color flashColor, Color baseColor, bool disableLineRenderer = false)
        {
            int count = 0;
            while (count < flashCount)
            {
                _bulletLine.material.color = flashColor;
                yield return new WaitForSeconds(flashTime);
                _bulletLine.material.color = baseColor;
                yield return new WaitForSeconds(flashTime);
                count++;
            }

            if (disableLineRenderer)
            {
                _bulletLine.enabled = false;
            }
        }
        
        public void ShootCharge(InputAction.CallbackContext context)
        {
            if (context.performed && AmmoCount > 0)
            {
                if (_lineRendererAnimation != null)
                {
                    StopCoroutine(_lineRendererAnimation);
                    _bulletLine.material.color = Color.cyan;
                }
            
                _bulletLine.SetPositions(new Vector3[] { transform.position, Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y)) });
                _bulletLine.enabled = true;
                _bulletCharging = true;
                _startBulletChargeFrame = Time.frameCount;
            }
        }

        public void ShootRelease(InputAction.CallbackContext context)
        {
            if (context.performed && _bulletCharging)
            {
                if (_chargingFor >= bulletChargeTime)
                {
                    Shoot();
                }
                else
                {
                    _bulletLine.enabled = false;
                }

                _bulletCharging = false;
                _chargeReady = false;
                _chargingFor = 0;
            }
        }

        private void Shoot()
        {
            if (_lineRendererAnimation != null)
            {
                StopCoroutine(_lineRendererAnimation);
                _bulletLine.material.color = Color.cyan;
            }
        
            _lineRendererAnimation = StartCoroutine(FlashLineRenderer(0.1f, 3, Color.red, Color.cyan, true));

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y));
            Vector3 position = transform.position;
            RaycastHit2D[] hits = Physics2D.RaycastAll(position, mousePosition - new Vector2(position.x, position.y),
                                      Vector2.Distance(mousePosition, position)).OrderBy(h => h.distance).ToArray();

            Collider2D thisCollider = transform.GetComponent<Collider2D>();

            bool hitPlayer = false;
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider != thisCollider)
                {
                    Character character = hit.transform.GetComponent<Character>();
                    if (character != null)
                    {
                        character.Shot();
                        if (character is Player)
                        {
                            hitPlayer = true;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (!hitPlayer)
            {
                AmmoCount--;
            }
        }

        public void Sapped(float sapFactor)
        {
            startDisguise *= sapFactor;
            startDisguise = startDisguise > 100 ? 100 : startDisguise;
            startDisguise = startDisguise < 0 ? 0 : startDisguise;
        }
        
        public void Move(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Vector2 moveDirection = context.ReadValue<Vector2>();

                if (moveDirection == Vector2.up || moveDirection == Vector2.down ||
                    moveDirection == Vector2.left || moveDirection == Vector2.right)
                {
                    _moveDirection = moveDirection;
                }
                
                networkPosition.UpdatePosition(moveDirection);
            }
            else if (context.ReadValue<Vector2>() == Vector2.zero)
            {
                _moveDirection = Vector2.zero;
            }
        }

        public void Emote1(InputAction.CallbackContext context)
        {
            if (context.action.triggered)
            {
                Emote(0, 2f);
            }
        }
        
        public void Emote2(InputAction.CallbackContext context)
        {
            if (context.action.triggered)
            {
                Emote(1, 2f);
            }
        }
        
        public void Emote3(InputAction.CallbackContext context)
        {
            if (context.action.triggered)
            {
                Emote(2, 2f);
            }
        }
        
        public void Emote4(InputAction.CallbackContext context)
        {
            if (context.action.triggered)
            {
                Emote(3, 2f);
            }
        }
        public void Interact(InputAction.CallbackContext context)
        {
            if (context.action.triggered)
            {
                foreach (BasePickup pickup in PickupManager.pickups)
                {
                    if (Vector2.Distance(pickup.transform.position, transform.position) <= pickupRange)
                    {
                        StartCoroutine(PickupDelay(pickup, pickup.pickupDuration));
                        break;
                    }
                }
            }
        }

        private IEnumerator PickupDelay(BasePickup pickup, float delay)
        {
            pickup.PickupCountdown = true;
            pickup.AccessingPlayer = this;
            yield return new WaitForSecondsRealtime(delay);
            if (Vector2.Distance(pickup.transform.position, transform.position) <= pickupRange && pickup.PickupCountdown)
            {
                pickup.Pickup(this);
            }
        }
        
        public void AddAmmo(int ammo)
        {
            AmmoCount += ammo;
        }
        
        public void AddDisguise(float addAmount)
        {
            startDisguise += addAmount;
            startDisguise = startDisguise > 100 ? 100 : startDisguise;
        }

        public void PickupInventoryItem(BasePickup item, Sprite pickup)
        {
            inventorySlot.sprite = pickup;
            _inventory.Clear();
            _inventory.Add(item);
        }

        public void UseEquipment()
        {
            if (_inventory.Count > 0)
            {
                _inventory[0].UseEquipment();
                _inventory.Clear();
                inventorySlot.sprite = null;
            }
        }
    }
}