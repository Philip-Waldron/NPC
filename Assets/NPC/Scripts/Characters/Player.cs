using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NPC.Scripts.Items;
using NPC.Scripts.Networking;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NPC.Scripts.Characters
{
    public class Player : Character
    {
        [Header("Ammo")]
        [SerializeField, Range(1, 3)] public int AmmoCount = 1;
        [SerializeField, Range(1f, 10f)] public float _shootRange;
        [SerializeField] private GameObject _laserParticleEffect;
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
        private readonly List<ParticleSystem> _laserParticleSystem = new List<ParticleSystem>();
        
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
        [SerializeField] private LayerMask obstacleLayer;

        private Vector2 _moveDirection;
        private bool _moving;

        [Header("Emote")]
        private float _emoteDuration = 2f;
        
        [Header("Other Player Character"), Tooltip("List of GameObjects to be disabled when Other Player")]
        [SerializeField] private List<GameObject> otherPlayerObjects = new List<GameObject>();
        [SerializeField] private PlayerInput playerInput;

        public NetworkCharacterParameters networkedParameters;
        
        private void Start()
        {
            networkedParameters = gameObject.GetComponent<NetworkCharacterParameters>();
            
            AdjustAmmo();
            _thisCollider2D = transform.GetComponent<Collider2D>();
            _startDisguise = _disguiseIntegrity;
            timeScalar = _disguiseDuration / MaxDisguiseIntegrity;
            SetupLineRenderer();
            SetupParticleSystem();
        }

        public void MakeOtherPlayerCharacter()
        {
            playerInput.enabled = false;
            foreach (GameObject otherPlayerObject in otherPlayerObjects)
            {
                otherPlayerObject.SetActive(false);
            }
        }
        
        private void SetupLineRenderer()
        {
            _bulletLine = gameObject.AddComponent<LineRenderer>();
            _bulletLine.positionCount = 2;
            _bulletLine.startWidth = 0.025f;
            _bulletLine.material = new Material(Shader.Find("Unlit/Color")) { color = Color.cyan };
            _bulletLine.alignment = LineAlignment.TransformZ;
        }

        private void SetupParticleSystem()
        {
            _laserParticleEffect = Instantiate(_laserParticleEffect, transform);
            foreach (Transform child in _laserParticleEffect.transform)
            {
                ParticleSystem particle = child.GetComponent<ParticleSystem>();
                if (particle != null)
                {
                    _laserParticleSystem.Add(particle);
                }
            }
        }
        private void Update()
        {
            // Update Disguise.
            _disguiseBar.SetValueWithoutNotify(DisguiseIntegrity / MaxDisguiseIntegrity);
            _elapsedTime += Time.deltaTime / _disguiseDuration;
            _disguiseIntegrity = IsDead? 0f : Mathf.Lerp(_startDisguise, MinDisguiseIntegrity, _elapsedTime);
            
            // Move.
            if (!_moving && _moveDirection != Vector2.zero)
            {
                StartCoroutine(MoveToPosition(transform, transform.position + new Vector3(_moveDirection.x, _moveDirection.y), _timeToMove));
            }

            animationSpeed = _moveDirection.sqrMagnitude;
            
            if (_bulletCharging && _startBulletChargeFrame != Time.frameCount)
            {
                DrawLineRenderer();
                
                _chargingFor += Time.deltaTime;

                if (_chargingFor >= _bulletChargeTime && !_chargeReady)
                {
                    if (_lineRendererAnimation != null)
                    {
                        StopCoroutine(_lineRendererAnimation);
                    }
                
                    _lineRendererAnimation = StartCoroutine(FlashLineRenderer(0.2f, 1, Color.white, Color.cyan));
                    _chargeReady = true;
                    foreach (ParticleSystem particle in _laserParticleSystem)
                    {
                        particle.Play();
                    }
                }
            }
        }
        public void Move(InputAction.CallbackContext context)
        {
            if (context.performed && !IsDead)
            {
                Vector2 moveDirection = context.ReadValue<Vector2>();

                if (moveDirection == Vector2.up || moveDirection == Vector2.down || moveDirection == Vector2.left || moveDirection == Vector2.right)
                {
                    _moveDirection = moveDirection;
                }
            }
            else if (context.ReadValue<Vector2>() == Vector2.zero)
            {
                _moveDirection = Vector2.zero;
            }
        }
        public void ShootCharge(InputAction.CallbackContext context)
        {
            if (context.performed && AmmoCount > 0 && !IsDead)
            {
                if (_lineRendererAnimation != null)
                {
                    StopCoroutine(_lineRendererAnimation);
                    _bulletLine.material.color = Color.cyan;
                }

                DrawLineRenderer();
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
            foreach (ParticleSystem particle in _laserParticleSystem)
            {
                particle.Stop();
            }
        
            _lineRendererAnimation = StartCoroutine(FlashLineRenderer(0.1f, 3, Color.red, Color.cyan, true));

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, 0f));
            Vector3 mousePositionNormalised = new Vector3(mousePosition.x, mousePosition.y, 0f);
            Vector3 position = transform.position;
            Vector3 direction = mousePositionNormalised - position;
            RaycastHit2D[] hits = Physics2D.RaycastAll(position, direction, _shootRange).OrderBy(h => h.distance).ToArray();

            bool hitPlayer = false;
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider != _thisCollider2D)
                {
                    IDamageable target = hit.transform.GetComponent<IDamageable>();
                    if (target != null) 
                    {
                        if (target is Player && !hit.collider.GetComponent<Player>().IsDead)
                        {
                            hitPlayer = true;
                        }
                        target.Damage(direction, hit.point);
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
            else
            {
                AdjustDisguise(MaxDisguiseIntegrity);
            }
        }
        private void DrawLineRenderer()
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, 0f));
            Vector3 mousePositionNormalised = new Vector3(mousePosition.x, mousePosition.y, 0f);
            Vector3 position = transform.position;

            Vector3 direction = (mousePositionNormalised - position).normalized * _shootRange;
            Vector3 endPoint = position + direction;

            _laserParticleEffect.transform.right = direction;
            _bulletLine.SetPositions(new Vector3[] { position, endPoint });
        }
        public void Interact(InputAction.CallbackContext context)
        {
            if (context.action.triggered && !IsDead)
            {
                Array itemColliders = Physics2D.OverlapCircleAll(transform.position, pickupRange, itemMask);
                
                float minDistance = pickupRange;
                Item selectedItem = null;

                foreach (Collider2D itemCollider in itemColliders)
                {
                    Item item = itemCollider.GetComponent<Item>();

                    if (item != null && !item.Accessed && Vector2.Distance(item.transform.position, transform.position) < minDistance)
                    {
                        minDistance = Vector2.Distance(item.transform.position, transform.position);
                        selectedItem = item;
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
            if (context.action.triggered && !IsDead)
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
        /// <summary>
        /// Adjust the disguise of the Player
        /// </summary>
        /// <param name="adjustment"> The change in disguise, if this is a scalar value, this should be between 1 and 0 </param>
        /// <param name="scalar"></param>
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
        public IEnumerator MoveToPosition(Transform targetTransform, Vector3 position, float timeToMove)
        {
            bool valid = ValidMove(targetTransform.position);
            if (!valid)
            {
                animationSpeed = 0;
                _moving = false;
                yield break;
            }
            
            _moving = true;
            Vector3 currentPos = targetTransform.position;
            animationMoveDirection = MovePosition(currentPos, position);
            
            if (networkedParameters != null)
            {
                networkedParameters.GridPosition = position;
            }
            
            float currentTime = 0f;
            while(currentTime < 1)
            {
                currentTime += Time.deltaTime / timeToMove;
                targetTransform.position = Vector3.Lerp(currentPos, position, currentTime);
                DrawLineRenderer();
                yield return null;
            }

            if (_moveDirection != Vector2.zero)
            {
                StartCoroutine(MoveToPosition(targetTransform, targetTransform.position + new Vector3(_moveDirection.x, _moveDirection.y), _timeToMove));
            }
            else
            {
                _moving = false;
            }
        }

        private bool ValidMove(Vector3 position)
        {
            bool valid = !Physics2D.Raycast(position, _moveDirection, _moveDirection.magnitude, obstacleLayer);
            return valid;
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