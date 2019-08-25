﻿using System.Collections;
using System.Collections.Generic;
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

        [Header("Scan")]
        [SerializeField] protected SpriteRenderer identificationSprite;
        [SerializeField] protected TextMeshPro identificationText;
        [SerializeField] protected string characterName = "Character Name";

        [Header("Speech Bubble")]
        [SerializeField] protected GameObject speechBubble;
        [SerializeField] protected TextMeshPro speechTextMesh;
        [SerializeField] protected Image emoteImage;
        [SerializeField] protected List<Sprite> emotes = new List<Sprite>();

        private Coroutine _speechBubbleCoroutine;
        private readonly Color _enabledColor = new Color(1, 1, 1, 1);
        private readonly Color _disabledColor = new Color(1, 1, 1, 0);

        [Header("Audio")]
        [SerializeField] protected AudioSource audioSource;
        [SerializeField] protected List<AudioClip> audioClips = new List<AudioClip>();

        [Header("Animation")]
        public Animator Animator;
        [SerializeField, Space(10)] private GameObject deathPuddleParticleEffect;
        [SerializeField] private GameObject deathSplatterParticleEffect;
        [SerializeField] private GameObject bulletHole;

        protected Vector2 AnimationMoveDirection = Vector2.zero;
        protected float AnimationSpeed;
        private RuntimeAnimatorController _animatorController;

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
            _animatorController = GameManager.AnimatorControllers[Random.Range(0, GameManager.AnimatorControllers.Count)];
            Animator.runtimeAnimatorController = _animatorController;
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
            Instantiate(deathPuddleParticleEffect, transform);
            GameObject splatter = Instantiate(deathSplatterParticleEffect, transform);
            GameObject bulletHole = Instantiate(this.bulletHole, transform);
            bulletHole.transform.position = hitPoint;
            splatter.transform.position = transform.position;
            splatter.transform.right = target;
            Animator.SetBool(Dead, true);

            // Messaging
            Scan(Mathf.Infinity);
            Emote(Random.Range(2, 4), 3f);
            SpeakAudio(Random.Range(0, audioClips.Count));
        }

        public void Scan(float revealDuration)
        {
            StartCoroutine(OnScan(revealDuration));
        }

        IEnumerator OnScan(float scanDuration)
        {
            identificationSprite.enabled = true;
            identificationText.renderer.enabled = true;
            yield return new WaitForSeconds(scanDuration);
            identificationSprite.enabled = false;
            identificationText.renderer.enabled = false;
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