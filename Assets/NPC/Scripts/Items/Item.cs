using NPC.Scripts.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace NPC.Scripts.Pickups
{
    public abstract class Item : MonoBehaviour
    {
        [SerializeField, Range(0f, 30f)]
        public float pickupDuration = 5f;
        [SerializeField, Space(10)]
        protected Slider pickupBar;

        public abstract bool Pickup(Character character);
        
        public abstract void Use(Character character);
    }
}
