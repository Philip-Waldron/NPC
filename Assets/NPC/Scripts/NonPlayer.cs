using System.Collections.Generic;
using I_Spy.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NPC.Scripts
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
            WAIT,
            WALK_RANDOM,
            WALK_DIRECTION,
            WALK_LOCATION
        }

        float detectionChance;
        bool detected;
        void Start()
        {
            InvokeRepeating(nameof(CheckPlayers), detectionFrequency, detectionFrequency);
        }

        void Update()
        {
            if (scan)
            {
                Scanned();
            }
        }

        private void CheckPlayers()
        {
            foreach (Player player in playerManager.players)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                if (distance <= detectionRange && player.Disguise <= detectionThreshold)
                {
                    detectionChance = Max - player.Disguise;
                    detected = true;
                }
            }
            switch (detected)
            {
                case true:
                    StopAllCoroutines();
                    Alert(detectionChance < 0 ? 0 : detectionChance);
                    break;
                default:
                    speechBubble.SetActive(false);
                    break;
            }
        }

        private void Alert(float threshold)
        {
            float x = Random.Range(100, threshold);
            if (x >= Max - detectionSensitivity)
            {
                Speak(alerts[Random.Range(0, alerts.Count)], detectionFrequency);
                //Emote(Random.Range(0, emotes.Count), detectionFrequency);
            }
            detectionChance = 0f;
            detected = false;
        }
        
        private void Move()
        {
        
        }
    }
}