using System;
using System.Collections;
using System.Collections.Generic;
using I_Spy.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NPC.Scripts
{
    public abstract class Character : MonoBehaviour, IScan, IEmote<int, float>, ISpeak<string, float>
    {
        [SerializeField] protected Sprite defaultSprite;
        [SerializeField] protected Sprite revealedSprite;
        [SerializeField, Range(1f, 15f)] protected float scanDuration;
        [SerializeField, Space(10)] protected GameObject speechBubble;
        [SerializeField] protected TextMeshPro speech;
        [SerializeField] protected Image emoteImage;
        [SerializeField, Space(10)] protected List<Sprite> emotes = new List<Sprite>();

        const string Reset = "";
        protected PlayerManager playerManager;
        SpriteRenderer spriteRenderer;
        public bool scan;
        protected const float Min = 0f;
        protected const float Max = 100f;

        readonly Color enabledColor = new Color(1, 1, 1, 1);
        readonly Color disabledColor = new Color(1, 1, 1, 0);
        
        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = defaultSprite;
            playerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();
            speech.SetText(Reset);
            emoteImage.color = disabledColor;
        }

        public void Scan()
        {
            throw new NotImplementedException();
        }

        public void Scanned()
        {
            scan = false;
            StartCoroutine(OnScan());
        }

        IEnumerator OnScan()
        {
            spriteRenderer.sprite = revealedSprite;
            yield return new WaitForSeconds(scanDuration);
            spriteRenderer.sprite = defaultSprite;
        }

        public void Emote(int emoteIndex, float duration)
        {
            StartCoroutine(Emote(emotes[emoteIndex], duration));
        }
        
        private IEnumerator Emote(Sprite emoteSprite, float duration)
        {
            speechBubble.SetActive(true);
            speech.SetText(Reset);
            emoteImage.sprite = emoteSprite;
            emoteImage.color = enabledColor;
            yield return new WaitForSeconds(duration);
            emoteImage.color = disabledColor;
            speechBubble.SetActive(false);
        }

        public void Speak(string speechText, float duration)
        {
            StartCoroutine(Speech(speechText, duration));
        }

        private IEnumerator Speech(string text, float duration)
        {
            speechBubble.SetActive(true);
            speech.SetText(text);
            emoteImage.color = disabledColor;
            yield return new WaitForSeconds(duration);
            speech.SetText(Reset);
            speechBubble.SetActive(false);
        }
    }
}
