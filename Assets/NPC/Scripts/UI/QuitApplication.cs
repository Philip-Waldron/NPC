using System;
using UnityEngine;
using UnityEngine.UI;

namespace NPC.Scripts.UI
{
    public class QuitApplication : MonoBehaviour
    {
        public Button quitButton;

        private void Start()
        {
            quitButton.onClick.AddListener(Quit);
        }

        private static void Quit()
        {
            Application.Quit();
        }
    }
}
