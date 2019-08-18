﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace NPC.Scripts.Characters
{
    public abstract class Character : MonoBehaviour, IDamageable, IScannable, IEmote<int, float>, ISpeak<string, float, int>
    {
        [Header("Scan")]
        [SerializeField] protected SpriteRenderer revealedSprite;
        
        [Header("Speech Bubble")] 
        [SerializeField, Space(10)] protected GameObject speechBubble;
        [SerializeField] protected TextMeshPro speechTextMesh;
        [SerializeField] protected Image emoteImage;
        [SerializeField] protected List<Sprite> emotes = new List<Sprite>();
        
        private readonly Color _enabledColor = new Color(1, 1, 1, 1);
        private readonly Color _disabledColor = new Color(1, 1, 1, 0);
        private Coroutine _speechBubbleCoroutine;
        
        [Header("Audio")]
        [SerializeField, Space(10)] private AudioSource audioSource;
        [SerializeField] protected List<AudioClip> audioClips = new List<AudioClip>();

        [Header("Animation")] 
        [SerializeField, Space(10)] private Animator animator;
        [SerializeField, Space(10)] private GameObject deathPuddleParticleEffect;
        [SerializeField] private GameObject deathSplatterParticleEffect;
        [SerializeField] private GameObject bulletHole;

        [HideInInspector] public UnityEvent onDeath;
        
        protected Vector2 animationMoveDirection = Vector2.zero;
        protected float animationSpeed;
        private static readonly int Horizontal = Animator.StringToHash("Horizontal");
        private static readonly int Vertical = Animator.StringToHash("Vertical");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Dead = Animator.StringToHash("Dead");

        public bool IsDead { get; set; }

        private void LateUpdate()
        {
            animator.SetFloat(Horizontal, animationMoveDirection.x);
            animator.SetFloat(Vertical, animationMoveDirection.y);
            animator.SetFloat(Speed, animationSpeed);
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
        
        public void Damage(Vector3 target, Vector2 hitPoint)
        {
            onDeath.Invoke();
            IsDead = true;
            Instantiate(deathPuddleParticleEffect, transform);
            GameObject splatter = Instantiate(deathSplatterParticleEffect, transform);
            GameObject bulletHole = Instantiate(this.bulletHole, transform);
            bulletHole.transform.position = hitPoint;
            splatter.transform.position = transform.position;
            splatter.transform.right = target;
            animator.SetBool(Dead, true);
            Scan(Mathf.Infinity);
            Emote(Random.Range(2, 3), 3f);
            SpeakAudio(Random.Range(0, audioClips.Count));
        }
        
        public void Scan(float revealDuration)
        {
            StartCoroutine(OnScan(revealDuration));
        }
        
        IEnumerator OnScan(float scanDuration)
        {
            revealedSprite.enabled = true;
            yield return new WaitForSeconds(scanDuration);
            revealedSprite.enabled = false;
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
        
        protected static Vector2 MovePosition(Vector3 currentPos, Vector3 position)
        {
            return (position - currentPos).normalized;
        }
    }
}