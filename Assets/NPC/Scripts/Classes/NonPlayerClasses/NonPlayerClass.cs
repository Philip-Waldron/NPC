using System.Collections.Generic;
using UnityEngine;

namespace NPC.Scripts.Classes.NonPlayerClasses
{
    [CreateAssetMenu(fileName = "NonPlayerClass", menuName = "Classes/NonPlayerClass", order = 2)]
    public class NonPlayerClass : CharacterClass
    {
        [Header("Detection")]
        [Range(0f, 20f)] public float detectionRadius = 5f;
        public Vector2 detectionFrequency;
        public Vector2 talkativenessRange;
        public float alertDuration = 2f;
        public List<string> alerts = new List<string>();
        public AnimationCurve detectionChanceCurve;
        public AudioClip alertAudio;

        [Header("Path-finding")]
        public bool usePathfinding = false;
        public Vector2 waitTimeRange = new Vector2(1, 5);
        public Vector2Int walkCloseNodeRange = new Vector2Int(1, 4);
        public Vector2 walkFarDistanceRange = new Vector2(5, 25);

        [Header("Path-finding Chances")]
        public float waitChance = 40;
        public float walkToCloseChance = 20;
        public float walkToFarChance = 10;
        public float walkToRandomChance = 10;
        public float walkInRoomChance = 15;
        public float walkToItemChance = 5;
    }
}
