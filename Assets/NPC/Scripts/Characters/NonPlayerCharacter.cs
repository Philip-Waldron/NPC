using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NPC.Scripts.Classes.NonPlayerClasses;
using Pathfinding;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace NPC.Scripts.Characters
{
    public class NonPlayerCharacter : Character
    {

        [Header("Non Player Setup")]
        [SerializeField] private LayerMask playerMask;

        // A* Path-finding
        private Seeker _seeker;
        private Path _path;
        private int _currentWayPoint;
        public NonPlayerClass NonPlayerClass { get; private set; }
        
        private float _totalChance;
        private float _talkativeness;
        private float _detectionFrequency;

        //TODO: Find not awful way of giving network override to disable AI Pathing
        public bool avoidUpdatingState = false;

        private void Start()
        {
            SetupClass();
            
            // Add to GameManager Lists
            GameManager.NonPlayers.Add(this);
            
            // Add references
            _seeker = GetComponent<Seeker>();
            
            // Invoke methods
            InvokeRepeating(nameof(DetectPlayersAttempt), _detectionFrequency, _detectionFrequency);
            InvokeRepeating(nameof(RandomSpeech), _talkativeness, _talkativeness);
            
            // Add listeners
            onDeath.AddListener(StopAllCoroutines);

//            RollState();
        }
        
        private void SetupClass()
        {
            // Assign class
            NonPlayerClass = GameManager.NonPlayerClasses[Random.Range(0, GameManager.NonPlayerClasses.Count)];
            characterClass = NonPlayerClass;
            
            // Assign local values
            animatorController = NonPlayerClass.animatorController;
            Animator.runtimeAnimatorController = animatorController;
            audioClips = NonPlayerClass.audioClips.ToArray();
            _totalChance = NonPlayerClass.waitChance + NonPlayerClass.walkToCloseChance +
                           NonPlayerClass.walkToFarChance + NonPlayerClass.walkToRandomChance +
                           NonPlayerClass.walkInRoomChance + NonPlayerClass.walkToItemChance;
            _talkativeness = Random.Range(NonPlayerClass.talkativenessRange.x, NonPlayerClass.talkativenessRange.y);
            _detectionFrequency = Random.Range(NonPlayerClass.detectionFrequency.x, NonPlayerClass.detectionFrequency.y);
        }

        public void RollState()
        {
            if (IsDead || !NonPlayerClass.usePathfinding || avoidUpdatingState)
            {
                return;
            }

            float roll = Random.Range(0, _totalChance);
            // Wait.
            if (roll < NonPlayerClass.waitChance)
            {
                float waitTime = Random.Range(NonPlayerClass.waitTimeRange.x, NonPlayerClass.waitTimeRange.y);
                StartCoroutine(Wait(waitTime));
            }
            // Walk to close node.
            else if (roll < NonPlayerClass.waitChance + NonPlayerClass.walkToCloseChance)
            {
                List<Vector3> validPositions = new List<Vector3>();
                int nodeRange = Random.Range(NonPlayerClass.walkCloseNodeRange.x, NonPlayerClass.walkCloseNodeRange.y + 1);
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
            else if (roll < NonPlayerClass.waitChance + NonPlayerClass.walkToCloseChance + NonPlayerClass.walkToFarChance)
            {
                float radius = Random.Range(NonPlayerClass.walkFarDistanceRange.x, NonPlayerClass.walkFarDistanceRange.y);
                Vector3 point = Random.insideUnitSphere * radius;

                point += transform.position;
                WalkToPosition(point);
            }
            // Walk to random position.
            else if (roll < NonPlayerClass.waitChance + NonPlayerClass.walkToCloseChance + NonPlayerClass.walkToFarChance + NonPlayerClass.walkToRandomChance)
            {
                Vector3 targetPosition = GameManager.ValidMovePositions[Random.Range(0, GameManager.ValidMovePositions.Count - 1)];
                WalkToPosition(targetPosition);
            }
            // Walk in room.
            else if (roll < NonPlayerClass.waitChance + NonPlayerClass.walkToCloseChance + NonPlayerClass.walkToFarChance + NonPlayerClass.walkToRandomChance + NonPlayerClass.walkInRoomChance)
            {
                Vector3Int tilePosition = GameManager.Tilemap.WorldToCell(transform.position);
                Tile tile = (Tile)GameManager.Tilemap.GetTile(tilePosition);
                List<Vector3> roomTiles = GameManager.Zones.First(x => x.Tile == tile).TilePositions;
                Vector3 targetPosition = roomTiles[Random.Range(0, roomTiles.Count)];
                WalkToPosition(targetPosition);
            }
            // Walk to item. TODO: implement.
            else // if (roll < NonPlayerClass.waitChance + NonPlayerClass.walkToCloseChance + NonPlayerClass.walkToFarChance + NonPlayerClass.walkToRandomChance + NonPlayerClass.walkInRoomChance + NonPlayerClass.walkToItemChance)
            {
                float waitTime = Random.Range(NonPlayerClass.waitTimeRange.x, NonPlayerClass.waitTimeRange.y);
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
            if (IsDead)
            {
                return;
            }

            Collider2D[] playerColliders = Physics2D.OverlapCircleAll(transform.position, NonPlayerClass.detectionRadius, playerMask);
            foreach (Collider2D playerCollider in playerColliders)
            {
                Player player = playerCollider.GetComponent<Player>();
                if (player.IsDead)
                {
                    return;
                }
                int detectionChance = Mathf.RoundToInt(NonPlayerClass.detectionChanceCurve.Evaluate(player.DisguiseIntegrity));
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
            SpeakText(NonPlayerClass.alerts[Random.Range(0, NonPlayerClass.alerts.Count)], NonPlayerClass.alertDuration);
            audioSource.clip = NonPlayerClass.alertAudio;
            audioSource.Play();
            return true;
        }

        private void WalkToPosition(Vector3 targetPosition)
        {
            _seeker.StartPath(transform.position, targetPosition, OnPathComplete);
        }

        private void OnPathComplete(Path path)
        {
            if (!path.error)
            {
                _path = path;
                _currentWayPoint = 0;

                StartCoroutine(MoveToPosition(transform, _path.vectorPath[_currentWayPoint], NonPlayerClass.timeToMove));
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
            AnimationMoveDirection = movePosition;
            AnimationSpeed = movePosition == Vector2.zero ? 0f : 1f;

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

            if (_path != null && _currentWayPoint + 1 < _path.vectorPath.Count)
            {
                _currentWayPoint++;
                StartCoroutine(MoveToPosition(targetTransform, _path.vectorPath[_currentWayPoint], NonPlayerClass.timeToMove));
            }
            else
            {
                AnimationSpeed = 0f;
                RollState();
            }
        }

        private IEnumerator Wait(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            RollState();
        }
    }
}