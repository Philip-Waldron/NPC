using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using NPC.Scripts.Classes.PlayerClasses;
using NPC.Scripts.Items;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace NPC.Scripts.Characters
{
    public class Player : Character
    {
        [Header("Player References")]
        [SerializeField] private Slider _chargeBar;
        [SerializeField] private Slider _cooldownBar;
        [SerializeField, Space(10)] private Slider _disguiseBar;
        [SerializeField] private Transform _bulletCharges;
        [SerializeField, Space(10)] public LayerMask _itemMask;
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField, Space(10)] private string _bulletSortLayer = "UnderCharacter";
        
        [Header("Spectating")]
        public Transform _playerCamera;

        [Header("Other Player Character")]
        [Tooltip("List of GameObjects to be disabled when Other Player")]
        [SerializeField] private List<GameObject> otherPlayerObjects = new List<GameObject>();
        [SerializeField] private PlayerInput playerInput;

        [HideInInspector] public UnityEvent onSpectateStart;
        [HideInInspector] public UnityEvent onSpectateEnd;
        
        public PlayerClass PlayerClass { get; private set; }
        
        private Vector2 _moveDirection;
        private Vector3 _selfSpectatePosition;
        
        private int _spectatingIndex;
        private int _startBulletChargeFrame;
        public int AmmoCount { get; set; }
        private int _inventoryIndex = 0;
        
        private float _lastShotTime;
        private float _chargingFor;
        private float _disguiseIntegrity;
        private float _timeScalar;
        private float _elapsedTime;
        private float _startDisguise;
        public float DisguiseIntegrity => _disguiseIntegrity;
        
        private GameObject _laserParticleEffect;
        private GameObject _shootParticleEffect;

        private bool _isOtherPlayer;
        private bool _queuedWeaponCharge;
        private bool _bulletCharging;
        private bool _chargeReady;
        private bool _validItemPickup;
        private bool _moving;
        public bool HoldingPickupButton { get; set; }
        
        private LineRenderer _bulletLine;
        
        private Coroutine _lineRendererAnimation;
        
        private Collider2D _thisCollider2D;
        
        private readonly List<ParticleSystem> _laserParticleSystem = new List<ParticleSystem>();
        private readonly List<Item> _inventory = new List<Item>();

        public bool DisableInput { get; set; } // Used for disabling user input when the menu is open

        private void Start()
        {
            // Cache
            Transform t;
            
            // Setup
            SetupClass();
            AdjustAmmo();
            SetupLineRenderer();
            SetupParticleSystem();
            
            // Assign Values
            _lastShotTime = Time.time - PlayerClass.shootCooldownTime;
            _thisCollider2D = (t = transform).GetComponent<Collider2D>();
            _startDisguise = _disguiseIntegrity;
            _timeScalar = PlayerClass.disguiseDuration / PlayerClass.maxDisguiseIntegrity;
            SpriteRenderer.sortingOrder = -Mathf.CeilToInt(t.position.y);
            _selfSpectatePosition = _playerCamera.localPosition;
            if (networkedParameters.networkObject.IsOwner)
            {
                GameManager.onScreenInterface.Player = this;
                
                // Set Global Effects Volume
                GameManager.onScreenInterface.SFXVolume(PlayerPrefs.GetFloat("SFXVolume"));
                GameManager.onScreenInterface.sfxVolume.value = PlayerPrefs.GetFloat("SFXVolume");
            }

            // Add Listeners
            onDeath.AddListener(ShootRelease);
            onDeath.AddListener(StopAllCoroutines);
            onDeath.AddListener(OnDeathSpectate);
            GameManager.WinState.AddListener(OnWin);
            onSpectateStart.AddListener(SpectateStart);
            onSpectateEnd.AddListener(SpectateEnd);

            // Add to GameManager Lists
            GameManager.LivePlayers.Add(this);
            GameManager.AllPlayers.Add(this);

            // Naming
            networkedParameters.GenerateName();
            SetCharacterName(characterName + "_" + GameManager.LivePlayers.Count);
        }

        private void SetupClass()
        {
            // Assign class
            PlayerClass = GameManager.PlayerClasses[Random.Range(0, GameManager.PlayerClasses.Count)];
            characterClass = PlayerClass;
            
            // Assign local values
            AmmoCount = PlayerClass.ammoCount;
            audioClips = PlayerClass.audioClips.ToArray();
            _disguiseIntegrity = PlayerClass.disguiseIntegrity;
            animatorController = PlayerClass.animatorController;
            Animator.runtimeAnimatorController = animatorController;
        }
        
        //todo: Find better means of updating the player Count
        public void RemoveFromLivePlayersList()
        {
            GameManager.LivePlayers.Remove(this);
        }
        
        private void Update()
        {
            // Cache
            Transform t = transform;
            
            // Update Disguise.
            _disguiseBar.SetValueWithoutNotify(DisguiseIntegrity / PlayerClass.maxDisguiseIntegrity);
            _elapsedTime += Time.deltaTime / PlayerClass.disguiseDuration;
            _disguiseIntegrity = IsDead? 0f : Mathf.Lerp(_startDisguise, PlayerClass.minDisguiseIntegrity, _elapsedTime);
            
            // Update Cooldown and Charge Bars
            _cooldownBar.SetValueWithoutNotify((Time.time - _lastShotTime) / PlayerClass.shootCooldownTime);
            _chargeBar.SetValueWithoutNotify(_chargingFor > PlayerClass.bulletChargeTime ? 1 : _chargingFor / PlayerClass.bulletChargeTime);

            // Move.
            if (!_moving && _moveDirection != Vector2.zero)
            {
                StartCoroutine(MoveToPosition(t, t.position + new Vector3(_moveDirection.x, _moveDirection.y), PlayerClass.timeToMove));
            }

            AnimationSpeed = _moveDirection.sqrMagnitude;
            
            // Shooting Charge
            if (_bulletCharging && _startBulletChargeFrame != Time.frameCount)
            {
                DrawLineRenderer();
                
                _chargingFor += Time.deltaTime;

                if (_chargingFor >= PlayerClass.bulletChargeTime && !_chargeReady)
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

            // Shoot Queueing
            if ((Time.time - _lastShotTime) >= PlayerClass.shootCooldownTime && !_bulletCharging && _queuedWeaponCharge)
            {
                ShootCharge();
            } 
        }
        public void MakeOtherPlayerCharacter()
        {
            _isOtherPlayer = true;
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
            _bulletLine.endWidth = 0f;
            _bulletLine.material = new Material(Shader.Find("Sprites/Default")) { color = Color.cyan };
            _bulletLine.alignment = LineAlignment.TransformZ;
            _bulletLine.enabled = false;
            _bulletLine.sortingLayerName = _bulletSortLayer;
        }
        private void SetupParticleSystem()
        {
            _laserParticleEffect = Instantiate(PlayerClass.laserParticleEffect, transform);
            foreach (Transform child in _laserParticleEffect.transform)
            {
                ParticleSystem particle = child.GetComponent<ParticleSystem>();
                if (particle != null)
                {
                    _laserParticleSystem.Add(particle);
                }
            }
        }
        public void Move(InputAction.CallbackContext context)
        {
            if (DisableInput || IsDead)
            {
                return;
            }
            if (context.performed)
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
            if (DisableInput)
            {
                return;
            }
            if (context.performed && AmmoCount > 0 && !IsDead)
            {
                switch ((Time.time - _lastShotTime) >= PlayerClass.shootCooldownTime)
                {
                    case true: // Weapon has cooled down
                        ShootCharge();
                        break;
                    default: // Weapon has not cooled down
                        _queuedWeaponCharge = true;
                        break;
                }
            }
        }
        private void ShootCharge()
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
        public void ShootRelease(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                ShootRelease();
            }
        }
        private void ShootRelease()
        {
            switch (_bulletCharging)
                {
                    case true: // When the weapon is charging
                        if (_chargingFor >= PlayerClass.bulletChargeTime)
                        {
                            Shoot();
                        }
                        else
                        {
                            _bulletLine.enabled = false;
                        }
                        _queuedWeaponCharge = false;
                        _bulletCharging = false;
                        _chargeReady = false;
                        _chargingFor = 0;
                        break;
                    default: // When the weapon is cooling down
                        _queuedWeaponCharge = false;
                        break;
                }
        }
        private void Shoot()
        {
            if (IsDead || DisableInput)
            {
                return;
            }
            
            _chargingFor = 0;
            
            // A E S T H E T I C S
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
            _shootParticleEffect = Instantiate(PlayerClass.shootParticleEffect);

            // Cooldown
            _lastShotTime = Time.time;
            
            // Audio
            audioSource.clip = PlayerClass.shootAudioClip;
            audioSource.Play();

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, 0f));
            Vector3 mousePositionNormalised = new Vector3(mousePosition.x, mousePosition.y, 0f);
            Vector3 position = transform.position;
            Vector3 direction = mousePositionNormalised - position;
            RaycastHit2D[] hits = Physics2D.RaycastAll(position, direction, PlayerClass.shootRange).OrderBy(h => h.distance).ToArray();

            bool hitPlayer = false;
            bool hitNonPlayer = false;

            _shootParticleEffect.transform.right = direction;
            _shootParticleEffect.transform.position = position;
            
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
                        else if (target is NonPlayerCharacter &&
                                 !hit.collider.GetComponent<NonPlayerCharacter>().IsDead)
                        {
                            hitNonPlayer = true;
                        }
                        
                        target.Damage(direction, hit.point, true);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (hitPlayer) // Reward for killing player
            {
                AdjustDisguise(PlayerClass.maxDisguiseIntegrity);
            }
            if (hitNonPlayer) // Punishment for hitting NPC
            {
                AdjustDisguise(-PlayerClass.shootNonPlayerPenalty, true); // If we make this scalar, we encourage players to play more recklessly when they are nearly out of disguise
                if (!PlayerClass.useAmmo)
                {
                    return;
                }
                AmmoCount--;
                AdjustAmmo();
            }
            if (!hitPlayer && !hitNonPlayer) // Hit nothing
            {
                if (!PlayerClass.useAmmo)
                {
                    return;
                }
                AmmoCount--;
                AdjustAmmo();
            }
        }
        private void DrawLineRenderer()
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, 0f));
            Vector3 mousePositionNormalised = new Vector3(mousePosition.x, mousePosition.y, 0f);
            Vector3 position = transform.position;

            Vector3 direction = (mousePositionNormalised - position).normalized * PlayerClass.shootRange;
            Vector3 endPoint = position + direction;

            _laserParticleEffect.transform.right = direction;
            _bulletLine.SetPositions(new Vector3[] { position, endPoint });
        }
        #region Emotes
        public void Emote1(InputAction.CallbackContext context)
        {
            if (context.action.triggered)
            {
                networkedParameters.BroadcastEmote(0);
                Emote(0, PlayerClass.emoteDuration);
            }
        }
        public void Emote2(InputAction.CallbackContext context)
        {
            if (context.action.triggered)
            {
                networkedParameters.BroadcastEmote(1);
                Emote(1, PlayerClass.emoteDuration);
            }
        }
        public void Emote3(InputAction.CallbackContext context)
        {
            if (context.action.triggered)
            {
                networkedParameters.BroadcastEmote(2);
                Emote(2, PlayerClass.emoteDuration);
            }
        }
        public void Emote4(InputAction.CallbackContext context)
        {
            if (context.action.triggered)
            {
                networkedParameters.BroadcastEmote(3);
                Emote(3, PlayerClass.emoteDuration);
            }
        }
        public void EmoteF(InputAction.CallbackContext context)
        {
            if (context.action.triggered)
            {
                networkedParameters.BroadcastEmote(4);
                Emote(4, PlayerClass.emoteDuration);
            }
        }
        #endregion
        public void OpenMenu(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                GameManager.onScreenInterface.MenuScreen(true);
            }
        }
        public void SetCharacterName(string newName)
        {
            characterName = newName;
            identificationText.SetText(characterName);
            if (networkedParameters.networkObject.IsOwner)
            {
                GameManager.onScreenInterface.playerName.SetText(characterName);
            }
        }
        public void Interact(InputAction.CallbackContext context)
        {
            // Button States
            if (context.started)
            {
                HoldingPickupButton = true;
            }
            if (context.canceled)
            {
                HoldingPickupButton = false;
            }
            
            // Interactions
            if (context.started && !IsDead)
            {
                Vector3 position = transform.position;
                
                Array itemColliders = Physics2D.OverlapCircleAll(position, PlayerClass.pickupRange, _itemMask);
                float minDistance = PlayerClass.pickupRange;
                Item selectedItem = null;

                foreach (Collider2D itemCollider in itemColliders)
                {
                    Vector3 itemPosition = itemCollider.transform.position;
                    float itemDistance = Vector2.Distance(itemPosition, position);;
                    bool obstructed = Physics2D.Raycast(position, itemPosition - position, itemDistance, obstacleLayer);
                    Item item = itemCollider.GetComponent<Item>();

                    if (item != null && !item.Accessed && Vector2.Distance(item.transform.position, position) < minDistance && !obstructed)
                    {
                        minDistance = itemDistance;
                        selectedItem = item;
                    }
                }

                if (selectedItem == null) return;
                
                switch (_inventory.Count > 0 && _inventory[_inventoryIndex] is Trap)
                {
                    case true:
                        selectedItem.SetTrap();
                        _inventory.RemoveAt(0);
                        AdjustInventory();
                        break;
                    default:
                        if (_inventory.Count <= PlayerClass.inventoryCount)
                        {
                            selectedItem.PickupItem(this);
                        }
                        else
                        {
                            Debug.LogWarning("Can't add " + selectedItem.name + " because inventory is full!");
                        }
                        break;
                }
            }
        }
        public void AddInventoryItem(Item item, Sprite pickup)
        {
            _inventory.Add(item);
            AdjustInventory();
        }
        public void CycleInventoryItems(InputAction.CallbackContext context)
        {
            if (DisableInput)
            {
                return;
            }
            if (context.action.triggered && _inventory.Count > 0)
            {
                switch (context.ReadValue<float>() > 0)
                {
                    case true:
                        _inventoryIndex--;
                        _inventoryIndex = _inventoryIndex < 0 ? _inventory.Count - 1 : _inventoryIndex;
                        break;
                    default:
                        _inventoryIndex++;
                        _inventoryIndex = _inventoryIndex > _inventory.Count - 1 ? 0 : _inventoryIndex;
                        break;
                }
                
                AdjustInventory();
            }
        }
        public void UseEquipmentItem(InputAction.CallbackContext context)
        {
            if (IsDead || DisableInput)
            {
                return;
            }
            if (context.action.triggered)
            {
                if (_inventory.Count > 0)
                {
                    if (_inventory[_inventoryIndex] is Trap)
                    {
                        return;
                    }
                    _inventory[_inventoryIndex].Use(this);
                    _inventory.RemoveAt(_inventoryIndex);
                    AdjustInventory();
                }
            }
        }
        private void AdjustInventory()
        {
            // If you used the last item, set the new item to the last item
            _inventoryIndex = _inventoryIndex > _inventory.Count - 1 ? _inventory.Count > 0 ? _inventory.Count - 1 : 0 : _inventoryIndex;
            
            // get rid of all the inventory
            foreach (Transform inventoryBar in GameManager.onScreenInterface.inventoryBar)
            {
                Destroy(inventoryBar.gameObject);
            }
            
            // restock the items
            int index = 0;
            
            foreach (Item item in _inventory)
            { 
                GameObject itemImage = Instantiate(PlayerClass.itemPrefab, GameManager.onScreenInterface.inventoryBar);
                itemImage.GetComponent<Image>().sprite = item.itemSprite;
                if (index == _inventoryIndex)
                {
                    itemImage.transform.GetChild(0).gameObject.SetActive(true);
                }
                index++;
            }
        }
        
        /// <summary>
        /// Adjust the disguise of the Player
        /// </summary>
        /// <param name="adjustment"> The change in disguise, if this is a scalar value, this should be between 1 and 0 </param>
        /// <param name="scalar"></param>
        public void AdjustDisguise(float adjustment, bool scalar = false)
        {
            float adjustedDisguiseIntegrity = PlayerClass.disguiseIntegrity;

            if (scalar)
            {
                adjustedDisguiseIntegrity += (adjustedDisguiseIntegrity * adjustment);
            }
            else
            {
                adjustedDisguiseIntegrity += adjustment;
            }

            _disguiseIntegrity = adjustedDisguiseIntegrity;
            _disguiseIntegrity = _disguiseIntegrity > PlayerClass.maxDisguiseIntegrity ? PlayerClass.maxDisguiseIntegrity : PlayerClass.disguiseIntegrity;
            _disguiseIntegrity = _disguiseIntegrity < PlayerClass.minDisguiseIntegrity ? PlayerClass.minDisguiseIntegrity : PlayerClass.disguiseIntegrity;
            
            _elapsedTime = Mathf.InverseLerp(PlayerClass.maxDisguiseIntegrity, PlayerClass.minDisguiseIntegrity, _disguiseIntegrity);
        }
        public void AdjustAmmo()
        {
            if (!PlayerClass.useAmmo)
            {
                return;
            }
            
            // get rid of all the bullets
            foreach (Transform charge in _bulletCharges)
            {
                Destroy(charge.gameObject);
            }
            
            // add as many new bullets as there is ammo
            AmmoCount = AmmoCount < 0 ? 0 : AmmoCount;
            for (int i = 0; i < AmmoCount; i++)
            {
                Instantiate(PlayerClass.bulletChargeSprite, _bulletCharges);
            }
        }
        public IEnumerator MoveToPosition(Transform targetTransform, Vector3 position, float timeToMove)
        {
            bool valid = ValidMove(targetTransform.position);
            
            if (!valid || IsDead)
            {
                AnimationSpeed = 0;
                _moving = false;
                yield break;
            }
            
            _moving = true;
            Vector3 currentPos = targetTransform.position;
            AnimationMoveDirection = MovePosition(currentPos, position);
            
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
            
            SpriteRenderer.sortingOrder = -Mathf.CeilToInt(transform.position.y);

            if (_moveDirection != Vector2.zero)
            {
                StartCoroutine(MoveToPosition(targetTransform, targetTransform.position + new Vector3(_moveDirection.x, _moveDirection.y), PlayerClass.timeToMove));
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
        public void CyclePlayers(InputAction.CallbackContext context)
        {
            if (context.action.triggered && IsDead)
            {
                switch (context.ReadValue<float>() > 0)
                {
                    case true:
                        _spectatingIndex--;
                        _spectatingIndex = _spectatingIndex < 0 ? GameManager.AllPlayers.Count : _spectatingIndex;
                        break;
                    default:
                        _spectatingIndex++;
                        _spectatingIndex = _spectatingIndex > GameManager.AllPlayers.Count ? 0 : _spectatingIndex;
                        break;
                }
                
                SpectatePlayer(_spectatingIndex);
            }
        }
        private void OnWin()
        {
            if (!IsDead && networkedParameters.networkObject.IsOwner)
            {
                GameManager.onScreenInterface.WinScreen();
            }
        }
        private void OnDeathSpectate()
        {
            if (GameManager.AllPlayers.Count > 1 && networkedParameters.networkObject.IsOwner)
            {
                SpectatePlayer(GameManager.AllPlayers.Count);
            }
        }
        private void SpectatePlayer(int index = 0)
        {
            switch (index == GameManager.AllPlayers.Count)
            {
                case true:
                    _playerCamera.SetParent(transform);
                    _playerCamera.localPosition = _selfSpectatePosition;
                    GameManager.onScreenInterface.SetSpectatingText("yourself,  you  dead  bitch");
                    break;
                default:
                    Player spectatingPlayer = GameManager.AllPlayers[index];
                    if (spectatingPlayer == this)
                    {
                        break;
                    }
                    Vector3 targetCameraPos = spectatingPlayer._playerCamera.transform.localPosition;
                    _playerCamera.SetParent(spectatingPlayer.transform);
                    _playerCamera.localPosition = targetCameraPos;
                    string n = spectatingPlayer.characterName;
                    string s = spectatingPlayer.IsDead ? n + ",  but  they're  dead  lmao" : n;
                    GameManager.onScreenInterface.SetSpectatingText(s);
                    foreach (Player player in GameManager.AllPlayers)
                    {
                        switch (player == spectatingPlayer)
                        {
                            case true:
                                player.onSpectateStart.Invoke();
                                break;
                            default:
                                player.onSpectateEnd.Invoke();
                                break;
                        }
                    }
                    break;
            }
        }
        private void SpectateStart()
        {
            foreach (GameObject otherPlayerObject in otherPlayerObjects)
            {
                otherPlayerObject.SetActive(true);
            }
        }
        private void SpectateEnd()
        {
            foreach (GameObject otherPlayerObject in otherPlayerObjects)
            {
                otherPlayerObject.SetActive(false);
            }
        }
    }
}
