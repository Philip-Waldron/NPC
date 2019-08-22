using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NPC.Scripts.Networking;
using Pathfinding;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace NPC.Scripts.Characters
{
    public class NonPlayerCharacter : Character
    {
        [Header("Detection")]
        [SerializeField, Range(0f, 20f)]
        private float _detectionRadius = 5f;
        [SerializeField]
        private Vector2 _detectionFrequency;
        [SerializeField]
        private List<string> _alerts = new List<string>();
        [SerializeField]
        private LayerMask _playerMask;
        [SerializeField]
        private AnimationCurve _detectionChanceCurve;

        [Header("Pathfinding")]
        [SerializeField]
        private Vector2 _waitTimeRange = new Vector2(1, 5);
        [SerializeField]
        private Vector2Int _walkCloseNodeRange = new Vector2Int(1, 4);
        [SerializeField]
        private Vector2 _walkFarDistanceRange = new Vector2(5, 25);
        [SerializeField, Range(.1f, 1f)]
        public float _timeToMove;

        // A* Pathfinding
        private Seeker _seeker;
        private Path _path;
        private int _currentWaypoint;
        
        [Header("Pathfinding Chances")]
        [SerializeField]
        private float _waitChance = 40;
        [SerializeField]
        private float _walkToCloseChance = 20;
        [SerializeField]
        private float _walkToFarChance = 10;
        [SerializeField]
        private float _walkToRandomChance = 10;
        [SerializeField]
        private float _walkInRoomChance = 15;
        [SerializeField]
        private float _walkToItemChance = 5;
        
        private float _totalChance;

        [Header("Emote")]
        [SerializeField]
        private Vector2 _talkativenessRange;
        private float _talkativeness;
        
        private float _emoteDuration = 2f;
        private float _alertDuration = 2f;
        
        public NetworkCharacterParameters networkedParameters;
        public bool pathingEnabled = true;

        private void Start()
        {
            networkedParameters = gameObject.GetComponent<NetworkCharacterParameters>();
            
            _seeker = GetComponent<Seeker>();
            float detectionFrequency = Random.Range(_detectionFrequency.x, _detectionFrequency.y);
            InvokeRepeating(nameof(DetectPlayersAttempt), detectionFrequency, detectionFrequency);
            _talkativeness = Random.Range(_talkativenessRange.x, _talkativenessRange.y);
            InvokeRepeating(nameof(RandomSpeech), _talkativeness, _talkativeness);
            onDeath.AddListener(StopMovement);

            _totalChance = _waitChance + _walkToCloseChance +
                           _walkToFarChance + _walkToRandomChance +
                           _walkInRoomChance + _walkToItemChance;
            
            if (pathingEnabled)
            {
                RollState();
            }
        }

        private void RollState()
        {
            if (IsDead)
            {
                return;
            }
            
            float roll = Random.Range(0, _totalChance);
            // Wait.
            if (roll < _waitChance)
            {
                float waitTime = Random.Range(_waitTimeRange.x, _waitTimeRange.y);
                StartCoroutine(Wait(waitTime));
            }
            // Walk to close node.
            else if (roll < _waitChance + _walkToCloseChance)
            {
                List<Vector3> validPositions = new List<Vector3>();
                int nodeRange = Random.Range(_walkCloseNodeRange.x, _walkCloseNodeRange.y + 1);
                List<GraphNode> nodesToCheck = new List<GraphNode>() { AstarPath.active.GetNearest(transform.position, NNConstraint.Default).node };
                List<GraphNode> newNodesToCheck = new List<GraphNode>();
                
                for (int i = 0; i < nodeRange; i++)
                {
                    foreach (var node in nodesToCheck)
                    {
                        node.GetConnections(otherNode =>
                        {
                            newNodesToCheck.Add(otherNode);
                            validPositions.Add((Vector3)otherNode.position);
                        });
                    }

                    nodesToCheck = newNodesToCheck;
                    newNodesToCheck.Clear();
                }

                Vector3 targetPosition = validPositions[Random.Range(0, validPositions.Count)];
                WalkToPosition(targetPosition);
            }
            // Walk to far position.
            else if (roll < _waitChance + _walkToCloseChance + _walkToFarChance)
            {
                float radius = Random.Range(_walkFarDistanceRange.x, _walkFarDistanceRange.y);
                Vector3 point = Random.insideUnitSphere * radius;
                
                point += transform.position;
                WalkToPosition(point);
            }
            // Walk to random position.
            else if (roll < _waitChance + _walkToCloseChance + _walkToFarChance + _walkToRandomChance)
            {
                Vector3 targetPosition = _gameManager.ValidMovePositions[Random.Range(0, _gameManager.ValidMovePositions.Count - 1)];
                WalkToPosition(targetPosition);
            }
            // Walk in room.
            else if (roll < _waitChance + _walkToCloseChance + _walkToFarChance + _walkToRandomChance + _walkInRoomChance)
            {
                Vector3Int tilePosition = _gameManager.Tilemap.WorldToCell(transform.position);
                Tile tile = (Tile)_gameManager.Tilemap.GetTile(tilePosition);
                List<Vector3> roomTiles = _gameManager.Zones.First(x => x.Tile == tile).TilePositions;
                Vector3 targetPosition = roomTiles[Random.Range(0, roomTiles.Count)];
                WalkToPosition(targetPosition);
            }
            // Walk to item. TODO: implement.
            else // if (roll < _waitChance + _walkToCloseChance + _walkToFarChance + _walkToRandomChance + _walkInRoomChance + _walkToItemChance)
            {
                float waitTime = Random.Range(_waitTimeRange.x, _waitTimeRange.y);
                StartCoroutine(Wait(waitTime));
            }
        }

        private void RandomSpeech()
        {
            if (IsDead)
            {
                return;
            }

            float chance = Random.Range(0, 100);
            if (chance < _talkativeness)
            {
                Emote(Random.Range(0, emotes.Count), 3f);
            }
        }
        
        private void DetectPlayersAttempt()
        {
            if (IsDead) return;
            
            Collider2D[] playerColliders = Physics2D.OverlapCircleAll(transform.position, _detectionRadius, _playerMask);
            foreach (Collider2D playerCollider in playerColliders)
            {
                Player player = playerCollider.GetComponent<Player>();

                int detectionChance = Mathf.RoundToInt(_detectionChanceCurve.Evaluate(player.DisguiseIntegrity));

                if (detectionChance != 0 && IsAlerted(detectionChance))
                {
                    break;
                }
            }
        }

        private bool IsAlerted(float detectionChance)
        {
            float roll = Random.Range(1, 101);
            if (roll > detectionChance)
            {
                return false;
            }

            SpeakText(_alerts[Random.Range(0, _alerts.Count)], _alertDuration);
            return true;
        }

        private void WalkToPosition(Vector3 targetPosition)
        {
            if (pathingEnabled)
            {
                _seeker.StartPath(transform.position, targetPosition, OnPathComplete);
            }
        }

        private void OnPathComplete(Path path)
        {
            if (!path.error)
            {
                _path = path;
                _currentWaypoint = 0;
                
                if (pathingEnabled)
                {
                    StartCoroutine(MoveToPosition(transform, _path.vectorPath[_currentWaypoint], _timeToMove));
                }
            }
            else
            {
                Debug.LogError("Path failed with error: " + path.error);
            }
        }

        public IEnumerator MoveToPosition(Transform targetTransform, Vector3 position, float timeToMove)
        {
            Vector3 currentPos = targetTransform.position;

            Vector2 movePosition = MovePosition(currentPos, position);
            animationMoveDirection = movePosition;
            animationSpeed = movePosition == Vector2.zero ? 0f : 1f;
            
            if (networkedParameters != null)
            {
                networkedParameters.GridPosition = position;
            }
            
            float currentTime = 0f;
            while(currentTime < 1)
            {
                currentTime += Time.deltaTime / timeToMove;
                targetTransform.position = Vector3.Lerp(currentPos, position, currentTime);
                yield return null;
            }

            if (!pathingEnabled)
                yield return null;

            if (_path != null && _currentWaypoint + 1 < _path.vectorPath.Count)
            {
                _currentWaypoint++;
                StartCoroutine(MoveToPosition(targetTransform, _path.vectorPath[_currentWaypoint], _timeToMove));
            }
            else
            {
                animationSpeed = 0f;
                RollState();
            }
        }

        private void StopMovement()
        {
            StopAllCoroutines();
        }
        
        private IEnumerator Wait(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            RollState();
        }
    }
}