using System.Collections;
using System.Collections.Generic;
using NPC.Scripts.Classes;
using NPC.Scripts.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace NPC.Scripts.Characters
{
    public abstract class Character : MonoBehaviour, IDamageable, IScannable, IEmote<int, float>, ISpeak<string, float, int>
    {
        // Character.
        protected SpriteRenderer SpriteRenderer;
        protected CharacterClass characterClass;
        
        [Header("Scan")] 
        [SerializeField] protected GameObject characterIdentifier;
        [SerializeField] public TextMeshPro identificationText;
        [SerializeField] public string characterName = "Character Name";

        [Header("Speech Bubble")]
        [SerializeField] protected GameObject speechBubble;
        [SerializeField] protected TextMeshPro speechTextMesh;
        [SerializeField] protected Image emoteImage;
        [SerializeField] protected List<Sprite> emotes = new List<Sprite>();

        private Coroutine _speechBubbleCoroutine;
        private readonly Color _enabledColor = new Color(1, 1, 1, 1);
        private readonly Color _disabledColor = new Color(1, 1, 1, 0);

        [Header("Audio")]
        [SerializeField] public AudioSource audioSource;
        protected AudioClip[] audioClips;

        [Header("Animation")]
        public Animator Animator;

        protected Vector2 AnimationMoveDirection = Vector2.zero;
        protected float AnimationSpeed;
        protected RuntimeAnimatorController animatorController;

        private static readonly int Horizontal = Animator.StringToHash("Horizontal");
        private static readonly int Vertical = Animator.StringToHash("Vertical");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Dead = Animator.StringToHash("Dead");

        // Death.
        [HideInInspector] public UnityEvent onDeath;
        public bool IsDead { get; set; }

        [HideInInspector] public NetworkCharacterParameters networkedParameters;
        protected GameManager GameManager;

        private void Awake()
        {
            networkedParameters = gameObject.GetComponent<NetworkCharacterParameters>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
            GameManager = FindObjectOfType<GameManager>();
            identificationText.SetText(characterName);
        }

        private void LateUpdate()
        {
            Animator.SetFloat(Horizontal, AnimationMoveDirection.x);
            Animator.SetFloat(Vertical, AnimationMoveDirection.y);
            Animator.SetFloat(Speed, AnimationSpeed);
        }

        protected static Vector2 MovePosition(Vector3 currentPos, Vector3 position)
        {
            return (position - currentPos).normalized;
        }

        public void Emote(int emoteIndex, float duration)
        {
            if (_speechBubbleCoroutine != null)
            {
                StopCoroutine(_speechBubbleCoroutine);
            }
            _speechBubbleCoroutine = StartCoroutine(Emote(emotes[emoteIndex], duration));
        }

        public void SpeakText(string speechText, float duration)
        {
            if (_speechBubbleCoroutine != null)
            {
                StopCoroutine(_speechBubbleCoroutine);
            }

            _speechBubbleCoroutine = StartCoroutine(Speech(speechText, duration));
        }

        public void SpeakAudio(int audioClipIndex)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = audioClips[audioClipIndex];
                audioSource.Play();
            }
        }

        public void Damage(Vector3 target, Vector2 hitPoint, bool shouldBroadcast)
        {
            if (shouldBroadcast)
            {
                networkedParameters.CommunicateShot(target, hitPoint);
            }

            // Death State
            onDeath.Invoke();
            IsDead = true;
            GameManager.CharacterDeath(this);

            // Death Effects
            Instantiate(characterClass.deathPuddleParticleEffect, transform);
            GameObject splatter = Instantiate(characterClass.deathSplatterParticleEffect, transform);
            GameObject bulletHole = Instantiate(characterClass.bulletHole, transform);
            bulletHole.transform.position = hitPoint;
            splatter.transform.position = transform.position;
            splatter.transform.right = target;
            Animator.SetBool(Dead, true);

            // Messaging
            Scan(Mathf.Infinity);
            Emote(Random.Range(2, 4), characterClass.emoteDuration);
            SpeakAudio(Random.Range(0, audioClips.Length));
        }

        public void Scan(float revealDuration)
        {
            StartCoroutine(OnScan(revealDuration));
        }

        IEnumerator OnScan(float scanDuration)
        {
            characterIdentifier.SetActive(true);
            yield return new WaitForSeconds(scanDuration);
            characterIdentifier.SetActive(false);
        }

        private IEnumerator Emote(Sprite emoteSprite, float duration)
        {
            speechBubble.SetActive(true);
            speechTextMesh.SetText("");
            emoteImage.sprite = emoteSprite;
            emoteImage.color = _enabledColor;
            yield return new WaitForSeconds(duration);
            emoteImage.color = _disabledColor;
            speechBubble.SetActive(false);
        }

        private IEnumerator Speech(string text, float duration)
        {
            speechBubble.SetActive(true);
            speechTextMesh.SetText(text);
            emoteImage.color = _disabledColor;
            yield return new WaitForSeconds(duration);
            speechTextMesh.SetText("");
            speechBubble.SetActive(false);
        }
    }
}