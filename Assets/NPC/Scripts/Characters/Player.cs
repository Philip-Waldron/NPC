using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NPC.Scripts.Items;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NPC.Scripts.Characters
{
    public class Player : Character
    {
        [Header("Ammo")]
        [SerializeField, Range(1, 3)] public int AmmoCount = 1;
        [SerializeField] private GameObject _bulletChargeSprite;
        [SerializeField] private Transform _bulletCharges;
        [SerializeField, Range(0f, 3f)] private float _bulletChargeTime = 1;

        private int _startBulletChargeFrame;
        private float _chargingFor;
        private bool _bulletCharging;
        private bool _chargeReady;
        private LineRenderer _bulletLine;
        private Coroutine _lineRendererAnimation;
        private Collider2D _thisCollider2D;
        
        [Header("Items")]
        [SerializeField, Space(10f), Range(.1f, 3f)]
        public float pickupRange;
        [SerializeField] public LayerMask itemMask;
        [SerializeField] private SpriteRenderer _inventorySlot;
        
        public bool trapItem { get; set; }
        private readonly List<Item> _inventory = new List<Item>();

        [Header("Disguise")]
        [SerializeField] public float MaxDisguiseIntegrity = 100f;
        [SerializeField] public float MinDisguiseIntegrity = 0f;
        [SerializeField] private float _disguiseIntegrity = 100f;
        public float DisguiseIntegrity => _disguiseIntegrity;
        private float timeScalar;

        [SerializeField] private float _disguiseDuration = 300f;
        [SerializeField] private Slider _disguiseBar;

        [SerializeField] private float _elapsedTime;
        private float _startDisguise;

        [Header("Movement")]
        [SerializeField, Range(.1f, 1f)] private float _timeToMove;

        private Vector2 _moveDirection;
        private bool _moving;

        [Header("Emote")]
        private float _emoteDuration = 2f;
        
        private void Start()
        {
            AdjustAmmo();
            _thisCollider2D = transform.GetComponent<Collider2D>();
            _startDisguise = _disguiseIntegrity;
            timeScalar = _disguiseDuration / MaxDisguiseIntegrity;
            SetupLineRenderer();
        }
        private void SetupLineRenderer()
        {
            _bulletLine = gameObject.AddComponent<LineRenderer>();
            _bulletLine.positionCount = 2;
            _bulletLine.startWidth = 0.05f;
            _bulletLine.material = new Material(Shader.Find("Unlit/Color")) { color = Color.cyan };
            _bulletLine.alignment = LineAlignment.TransformZ;
        }
        private void Update()
        {
            // Update Disguise.
            _disguiseBar.SetValueWithoutNotify(DisguiseIntegrity / MaxDisguiseIntegrity);
            _elapsedTime += Time.deltaTime / _disguiseDuration;
            _disguiseIntegrity = Mathf.Lerp(_startDisguise, MinDisguiseIntegrity, _elapsedTime);
            
            // Move.
            if (!_moving && _moveDirection != Vector2.zero)
            {
                StartCoroutine(MoveToPosition(transform, transform.position + new Vector3(_moveDirection.x, _moveDirection.y), _timeToMove));
            }

            if (_bulletCharging && _startBulletChargeFrame != Time.frameCount)
            {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, 0f));
                Vector3 mousePositionNormalised = new Vector3(mousePosition.x, mousePosition.y, 0f);
                Vector3 position = transform.position;
                
                _bulletLine.SetPositions(new Vector3[] { position, mousePositionNormalised });
                _chargingFor += Time.deltaTime;

                if (_chargingFor >= _bulletChargeTime && !_chargeReady)
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
        public void Move(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Vector2 moveDirection = context.ReadValue<Vector2>();

                if (moveDirection == Vector2.up || moveDirection == Vector2.down ||
                    moveDirection == Vector2.left || moveDirection == Vector2.right)
                {
                    _moveDirection = moveDirection;
                    animationMoveDirection = _moveDirection;
                }
            }
            else if (context.ReadValue<Vector2>() == Vector2.zero)
            {
                _moveDirection = Vector2.zero;
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
                if (_chargingFor >= _bulletChargeTime)
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

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, 0f));
            Vector3 mousePositionNormalised = new Vector3(mousePosition.x, mousePosition.y, 0f);
            Vector3 position = transform.position;
            Vector3 direction = mousePositionNormalised - position;
            RaycastHit2D[] hits = Physics2D.RaycastAll(position, direction, Vector2.Distance(mousePosition, position)).OrderBy(h => h.distance).ToArray();
            
            bool hitPlayer = false;
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider != _thisCollider2D)
                {
                    IDamageable target = hit.transform.GetComponent<IDamageable>();
                    if (target != null) 
                    {
                        target.Damage(direction);
                        if (target is Player)
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
                AdjustAmmo();
            }
        }
        public void Interact(InputAction.CallbackContext context)
        {
            if (context.action.triggered)
            {
                Array itemColliders = Physics2D.OverlapCircleAll(transform.position, pickupRange, itemMask);
                
                float minDistance = pickupRange;
                Item selectedItem = null;

                foreach (Collider2D itemCollider in itemColliders)
                {
                    Item item = itemCollider.GetComponent<Item>();
                    
                    Debug.Log(item.name + ": " + Vector2.Distance(item.transform.position, transform.position) + " < " + minDistance);
                    
                    if (item != null && !item.Accessed && Vector2.Distance(item.transform.position, transform.position) < minDistance)
                    {
                        minDistance = Vector2.Distance(item.transform.position, transform.position);
                        selectedItem = item;
                        
                        Debug.Log(selectedItem.name);
                    }
                }

                if (selectedItem == null) return;
                
                switch (trapItem)
                {
                    case true:
                        selectedItem.SetTrap();
                        trapItem = false;
                        ClearEquipment();
                        break;
                    default:
                        selectedItem.PickupItem(this);
                        break;
                }
            }
        }
        public void AddInventoryItem(Item item, Sprite pickup)
        {
            _inventorySlot.sprite = pickup;
            _inventory.Clear();
            _inventory.Add(item);
        }
        public void UseEquipmentItem(InputAction.CallbackContext context)
        {
            if (context.action.triggered)
            {
                if (_inventory.Count > 0)
                {
                    _inventory[0].Use(this);
                    ClearEquipment();
                }
            }
        }
        private void ClearEquipment()
        {
            _inventory.Clear();
            _inventorySlot.sprite = null;
        }
        public void AdjustDisguise(float adjustment, bool scalar = false)
        {
            float adjustedDisguiseIntegrity = _disguiseIntegrity;

            if (scalar)
            {
                adjustedDisguiseIntegrity *= adjustment;
            }
            else
            {
                adjustedDisguiseIntegrity += adjustment;
            }

            _disguiseIntegrity = adjustedDisguiseIntegrity;
            _disguiseIntegrity = _disguiseIntegrity > MaxDisguiseIntegrity ? MaxDisguiseIntegrity : _disguiseIntegrity;
            _disguiseIntegrity = _disguiseIntegrity < MinDisguiseIntegrity ? MinDisguiseIntegrity : _disguiseIntegrity;
            
            _elapsedTime = Mathf.InverseLerp(MaxDisguiseIntegrity, MinDisguiseIntegrity, _disguiseIntegrity);
        }
        public void AdjustAmmo()
        {
            // get rid of all the bullets
            foreach (Transform charge in _bulletCharges)
            {
                Destroy(charge.gameObject);
            }
            // add as many new bullets as there is ammo

            AmmoCount = AmmoCount < 0 ? 0 : AmmoCount;
            
            for (int i = 0; i < AmmoCount; i++)
            {
                Instantiate(_bulletChargeSprite, _bulletCharges);
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
                _bulletLine.SetPositions(new Vector3[] { transform.position, Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y)) });
                yield return null;
            }

            if (_moveDirection != Vector2.zero)
            {
                animationSpeed = 1;
                StartCoroutine(MoveToPosition(targetTransform, targetTransform.position + new Vector3(_moveDirection.x, _moveDirection.y), this._timeToMove));
            }
            else
            {
                animationSpeed = 0;
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
        
        #region Emotes

        public void Emote1(InputAction.CallbackContext context)
        {
            if (context.action.triggered)
            {
                Emote(0, _emoteDuration);
            }
        }
        public void Emote2(InputAction.CallbackContext context)
        {
            if (context.action.triggered)
            {
                Emote(1, _emoteDuration);
            }
        }
        public void Emote3(InputAction.CallbackContext context)
        {
            if (context.action.triggered)
            {
                Emote(2, _emoteDuration);
            }
        }
        public void Emote4(InputAction.CallbackContext context)
        {
            if (context.action.triggered)
            {
                Emote(3, _emoteDuration);
            }
        }

        #endregion
    }
}