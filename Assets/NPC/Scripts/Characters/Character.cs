using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace NPC.Scripts.Characters
{
    public abstract class Character : MonoBehaviour, IDamageable, IScannable, IEmote<int, float>, ISpeak<string, float, int>
    {
        [Header("Scan")]
        [SerializeField]
        protected SpriteRenderer revealedSprite;
        
        [Header("Speech Bubble")]
        [SerializeField]
        protected GameObject speechBubble;
        [SerializeField]
        protected TextMeshPro speechTextMesh;
        [SerializeField]
        protected Image emoteImage;
        [SerializeField]
        protected List<Sprite> emotes = new List<Sprite>();
        
        private readonly Color _enabledColor = new Color(1, 1, 1, 1);
        private readonly Color _disabledColor = new Color(1, 1, 1, 0);
        private Coroutine _speechBubbleCoroutine;
        
        [Header("Audio")]
        [SerializeField]
        private AudioSource _audioSource;
        [SerializeField]
        protected List<AudioClip> audioClips = new List<AudioClip>();

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
            if (!_audioSource.isPlaying)
            {
                _audioSource.clip = audioClips[audioClipIndex];
                _audioSource.Play();
            }
        }
        
        public void Damage()
        {
            gameObject.GetComponent<ParticleSystem>().Play();
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
    }
}