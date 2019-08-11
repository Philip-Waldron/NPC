using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace I_Spy.Scripts.UI
{
    public class StartGame : MonoBehaviour
    {
        [SerializeField] Button button;
        float t;
        [SerializeField] CanvasGroup canvasGroup;
        void Awake()
        {
            button.onClick.AddListener(Click);
        }

        void Click()
        {
            canvasGroup.interactable = false;
            canvasGroup.alpha = 0f;
        }
    }
}
