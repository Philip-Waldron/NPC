using TMPro;
using UnityEngine;

namespace NPC.Scripts.UI
{
    public class CharacterNamePlayerPref : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameField;
        
        private const string CHARACTER_NAME_PREF = "CharacterName";

        private void Awake()
        {
            nameField.onEndEdit.AddListener(UpdateName);

            Debug.Log(PlayerPrefs.GetString(CHARACTER_NAME_PREF));
            nameField.text = PlayerPrefs.GetString(CHARACTER_NAME_PREF);
        }

        private static void UpdateName(string s)
        {
            
            PlayerPrefs.SetString(CHARACTER_NAME_PREF, s);
        }
    }
}
