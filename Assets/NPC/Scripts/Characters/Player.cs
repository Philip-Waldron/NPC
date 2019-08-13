using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NPC.Scripts.Pickups;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NPC.Scripts.Characters
{
    public class Player : Character
    {
        [Header("Ammo")]
        [SerializeField, Range(1, 3)]
        public int AmmoCount = 1;
        [SerializeField]
        private SpriteRenderer[] _bulletChargeSprites;
        [SerializeField, Range(0f, 3f)]
        private float _bulletChargeTime = 1;

        private int _startBulletChargeFrame;
        private float _chargingFor;
        private bool _bulletCharging;
        private bool _chargeReady;
        private LineRenderer _bulletLine;
        private Coroutine _lineRendererAnimation;
        private Collider2D _thisCollider2D;
        
        [Header("Items")]
        [SerializeField, Space(10f), Range(.1f, 3f)]
        public float PickupRange;
        [SerializeField]
        private SpriteRenderer _inventorySlot;
        
        private readonly List<Item> _inventory = new List<Item>();

        [Header("Disguise")]
        [SerializeField]
        public float MaxDisguiseIntegrity = 100f;
        [SerializeField]
        private float _disguiseIntegrity = 100f;
        public float DisguiseIntegrity => _disguiseIntegrity;

        [SerializeField]
        private float _disguiseDuration = 300f;
        [SerializeField]
        private Slider _disguiseBar;

        private float _elapsedTime;
        private float _startDisguise;

        [Header("Movement")]
        [SerializeField, Range(.1f, 1f)]
        private float _timeToMove;

        private Vector2 _moveDirection;
        private bool _moving;
        
        private void Start()
        {
            for (int i = 0; i < AmmoCount; i++)
            {
                _bulletChargeSprites[i].enabled = true;
            }
            _thisCollider2D = transform.GetComponent<Collider2D>();
            _startDisguise = _disguiseIntegrity;
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
            _disguiseIntegrity = Mathf.Lerp(_startDisguise, 0, _elapsedTime);
            
            // Move.
            if (!_moving && _moveDirection != Vector2.zero)
            {
                StartCoroutine(MoveToPosition(transform, transform.position + new Vector3(_moveDirection.x, _moveDirection.y), _timeToMove));
            }

            if (_bulletCharging && _startBulletChargeFrame != Time.frameCount)
            {
                _bulletLine.SetPositions(new Vector3[] { transform.position, Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y)) });
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

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y));
            Vector3 position = transform.position;
            RaycastHit2D[] hits = Physics2D.RaycastAll(position, mousePosition - new Vector2(position.x, position.y),
                                      Vector2.Distance(mousePosition, position)).OrderBy(h => h.distance).ToArray();
            
            bool hitPlayer = false;
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider != _thisCollider2D)
                {
                    IDamageable target = hit.transform.GetComponent<IDamageable>();
                    if (target != null)
                    {
                        target.Damage();
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
                _bulletChargeSprites[AmmoCount].enabled = false;
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
                }
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
                print("Pickup");
            }
        }
        
        public void AdjustDisguise(bool scalar, float adjustment)
        {
            if (scalar)
            {
                _disguiseIntegrity += adjustment;
            }
            else
            {
                _disguiseIntegrity *= adjustment;
            }

            _disguiseIntegrity = _disguiseIntegrity > MaxDisguiseIntegrity ? MaxDisguiseIntegrity : _disguiseIntegrity;
            _disguiseIntegrity = _disguiseIntegrity < 0 ? 0 : _disguiseIntegrity;
        }

        public void PickupInventoryItem(Item item, Sprite pickup)
        {
            _inventorySlot.sprite = pickup;
            _inventory.Clear();
            _inventory.Add(item);
        }

        public void UseEquipment()
        {
            print("UseEquipmentDEBUG");
            
            if (_inventory.Count > 0)
            {
                _inventory[0].Use(this);
                _inventory.Clear();
                _inventorySlot.sprite = null;
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
                StartCoroutine(MoveToPosition(targetTransform, targetTransform.position + new Vector3(_moveDirection.x, _moveDirection.y), this._timeToMove));
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
    }
}