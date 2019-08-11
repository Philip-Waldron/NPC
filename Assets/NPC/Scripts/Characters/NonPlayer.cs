using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NPC.Scripts.Characters
{
    public class NonPlayer : Character
    {
        [SerializeField, Space(10), Range(0f, 100f)] private float detectionRange = 20f;
        [SerializeField, Range(0f, 15f)] private float detectionFrequency = 5f;
        [SerializeField, Range(0f, 100f)] private float detectionThreshold = 50f;
        [SerializeField, Range(0f, 50f)] private float detectionSensitivity = 5f;
        [SerializeField] private List<string> alerts = new List<string>();
        
        [SerializeField] private Vector2 waitTimeRange;
        [SerializeField] private Vector2 walkRandomTimeRange;
    
        private enum ENonPlayerState
        {
            WalkRandom,
            Wait,
            WalkDirection,
            WalkLocation
        }

        private float _detectionChance;
        private bool _detected;
        
        private void Start()
        {
            InvokeRepeating(nameof(CheckPlayers), detectionFrequency, detectionFrequency);
        }


        private void CheckPlayers()
        {
            foreach (Player player in PlayerManager.players)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                if (distance <= detectionRange && player.Disguise <= detectionThreshold)
                {
                    _detectionChance = Max - player.Disguise;
                    Alert(_detectionChance < 0 ? 0 : _detectionChance);
                    _detected = true;
                }
            }
            /*
            switch (_detected)
            {
                case true:
                    //StopAllCoroutines();
                    Alert(_detectionChance < 0 ? 0 : _detectionChance);
                    break;
                default:
                    speechBubble.SetActive(false);
                    break;
            }
            */
        }

        private void Alert(float threshold)
        {
            float x = Random.Range(100, threshold);
            if (x >= Max - detectionSensitivity)
            {
                SpeakText(alerts[Random.Range(0, alerts.Count)], detectionFrequency);
            }
            _detectionChance = 0f;
            _detected = false;
        }
        
        private void Move()
        {
        
        }
    }
}