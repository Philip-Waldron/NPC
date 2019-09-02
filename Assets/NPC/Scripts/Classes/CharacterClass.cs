using System.Collections.Generic;
using UnityEngine;

namespace NPC.Scripts.Classes
{
    public class CharacterClass : ScriptableObject
    {
        [Header("Movement")]
        public float timeToMove;
        
        [Header("Animation")]
        public RuntimeAnimatorController animatorController;
        public GameObject deathPuddleParticleEffect;
        public GameObject deathSplatterParticleEffect;
        public GameObject bulletHole;
        
        [Header("Audio")]
        public List<AudioClip> audioClips = new List<AudioClip>();
        
        [Header("Emote")]
        public float emoteDuration = 2f;
    }
}
