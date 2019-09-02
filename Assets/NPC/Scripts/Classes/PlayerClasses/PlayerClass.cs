using System.Collections.Generic;
using NPC.Scripts.Items;
using UnityEngine;

namespace NPC.Scripts.Classes.PlayerClasses
{
    [CreateAssetMenu(fileName = "PlayerClass", menuName = "Classes/PlayerClass", order = 1)]
    public class PlayerClass : CharacterClass
    {
        [Header("Shooting")]
        public bool useAmmo = true;
        public int ammoCount = 1;
        public float shootRange;
        public GameObject laserParticleEffect;
        public GameObject shootParticleEffect;
        public GameObject itemPrefab;
        public GameObject bulletChargeSprite;
        public float bulletChargeTime = 1;
        public AudioClip shootAudioClip;
        public float shootNonPlayerPenalty = .2f;

        [Header("Cooldown")] 
        public float shootCooldownTime;

        [Header("Items")]
        public float pickupRange;
        public int inventoryCount = 3;
        
        [Header("Disguise")]
        public float maxDisguiseIntegrity = 100f;
        public float minDisguiseIntegrity = 0f;
        public float disguiseIntegrity = 100f;
        public float disguiseDuration = 300f;
    }
}
