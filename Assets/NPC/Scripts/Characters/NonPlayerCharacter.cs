using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NPC.Scripts.Characters
{
    public class NonPlayerCharacter : Character
    {
        private enum ENpcState
        {
            WalkRandom,
            Wait,
            WalkDirection,
            WalkLocation
        }
        
        [Header("Detection")]
        [SerializeField, Range(0f, 20f)] private float _detectionRadius = 5f;
        [SerializeField, Range(0f, 15f)] private float _detectionFrequency = 5f;
        [SerializeField, Range(0f, 100f)] private float _talkativeness = 25f;
        [SerializeField] private List<string> _alerts = new List<string>();
        
        [SerializeField]
        private LayerMask _playerMask;

        [SerializeField]
        private AnimationCurve _detectionChanceCurve;

        [Header("Pathfinding")]
        [SerializeField]
        private Vector2 waitTimeRange;
        [SerializeField]
        private Vector2 walkRandomTimeRange;
        
        [Header("Emote")]
        private float _emoteDuration = 2f;
        private float _alertDuration = 2f;

        private void Start()
        {
            InvokeRepeating(nameof(DetectPlayersAttempt), _detectionFrequency, _detectionFrequency);
            InvokeRepeating(nameof(RandomSpeech), 10f, 10f);
        }

        private void RandomSpeech()
        {
            if (isDead) return;

            float chance = Random.Range(0, 100);
            
            if (chance < _talkativeness)
            {
                Emote(Random.Range(0, emotes.Count), 3f);
            }
        }
        
        private void DetectPlayersAttempt()
        {
            if (isDead) return;
            
            Collider2D[] playerColliders = Physics2D.OverlapCircleAll(transform.position, _detectionRadius, _playerMask);
            foreach (Collider2D playerCollider in playerColliders)
            {
                Player player = playerCollider.GetComponent<Player>();
                int _detectionChance = Mathf.RoundToInt(_detectionChanceCurve.Evaluate(player.DisguiseIntegrity));

                if (_detectionChance != 0 && IsAlerted(_detectionChance))
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
        
        private void Move()
        {
        
        }
    }
}