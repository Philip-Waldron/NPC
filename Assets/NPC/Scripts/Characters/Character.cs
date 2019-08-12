using System;
using System.Collections;
using System.Collections.Generic;
using NPC.Scripts.Pickups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace NPC.Scripts.Characters
{
    public abstract class Character : MonoBehaviour, IScan, IEmote<int, float>, ISpeak<string, float, int>, IShoot
    {
        [SerializeField] protected Sprite defaultSprite;
        [SerializeField] protected Sprite revealedSprite;
        [SerializeField, Range(1f, 15f)] protected float scanDuration;
        [SerializeField, Space(10)] protected GameObject speechBubble;
        [SerializeField] protected TextMeshPro speech;
        [SerializeField] protected Image emoteImage;
        [SerializeField, Space(10)] protected List<Sprite> emotes = new List<Sprite>();
        [SerializeField, Space(10)] protected List<AudioClip> audioClips = new List<AudioClip>();

        const string Reset = "";
        protected PlayerManager PlayerManager;
        protected PickupManager PickupManager;
        SpriteRenderer _spriteRenderer;
        AudioSource _audioSource;

        protected const float Min = 0f;
        protected const float Max = 100f;

        readonly Color _enabledColor = new Color(1, 1, 1, 1);
        readonly Color _disabledColor = new Color(1, 1, 1, 0);
        
        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _audioSource = GetComponent<AudioSource>();
            _spriteRenderer.sprite = defaultSprite;
            PlayerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();
            PickupManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PickupManager>();
            speech.SetText(Reset);
            emoteImage.color = _disabledColor;
        }

        public void Scanned()
        {
            StartCoroutine(OnScan());
        }

        IEnumerator OnScan()
        {
            _spriteRenderer.sprite = revealedSprite;
            yield return new WaitForSeconds(scanDuration);
            _spriteRenderer.sprite = defaultSprite;
        }

        public void Emote(int emoteIndex, float duration)
        {
            StopCoroutine($"Emote");
            StartCoroutine(Emote(emotes[emoteIndex], duration));
        }
        
        private IEnumerator Emote(Sprite emoteSprite, float duration)
        {
            speechBubble.SetActive(true);
            speech.SetText(Reset);
            emoteImage.sprite = emoteSprite;
            emoteImage.color = _enabledColor;
            yield return new WaitForSeconds(duration);
            emoteImage.color = _disabledColor;
            speechBubble.SetActive(false);
        }

        public void SpeakText(string speechText, float duration)
        {
            StopCoroutine($"Speech");
            StartCoroutine(Speech(speechText, duration));
        }

        public void SpeakAudio(int audioClipIndex)
        {
            if (!_audioSource.isPlaying)
            {
                _audioSource.clip = audioClips[audioClipIndex];
                _audioSource.Play();
            }
        }

        private IEnumerator Speech(string text, float duration)
        {
            speechBubble.SetActive(true);
            speech.SetText(text);
            emoteImage.color = _disabledColor;
            yield return new WaitForSeconds(duration);
            speech.SetText(Reset);
            speechBubble.SetActive(false);
        }

        public void Shot()
        {
            gameObject.GetComponent<ParticleSystem>().Play();
            Scanned();
            Emote(Random.Range(2,3), 3f);
            SpeakAudio(Random.Range(0,audioClips.Count));
        }
    }
}