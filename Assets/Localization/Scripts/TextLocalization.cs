using TMPro;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace Localization.Scripts
{
    [RequireComponent(typeof(TextMeshPro))]
    public class TextLocalization : MonoBehaviour, ILocalization
    {
        [SerializeField] 
        private string key = "";

        public string Key
        {
            set
            {
                if (string.IsNullOrEmpty(key))
                {
                    key = value;
                }
            }
            get => key;
        }

        [SerializeField] 
        private string defaultValue = "";

        public string DefaultValue
        {
            set
            {
                if (string.IsNullOrEmpty(defaultValue))
                {
                    defaultValue = value;
                }
            }
            get => defaultValue;
        }

        private TextMeshPro _textField;

        private void OnEnable()
        {
            LocalizationManager.LocalizationChange += OnLocalizationChange;
        }

        private void OnDisable()
        {
            LocalizationManager.LocalizationChange -= OnLocalizationChange;
        }

        private void Start()
        {
            if (key.Length == 0)
            {
                Debug.LogError($"A key reference is empty. ({name})");
            }

            _textField = GetComponent<TextMeshPro>();
            _textField.text = LocalizationManager.GetStringTableEntryOrDefault(key, defaultValue);
        }

        public void OnLocalizationChange(StringTable stringTable)
        {
            _textField.text = stringTable.GetEntry(key)?.GetLocalizedString() ?? defaultValue;
        }
    }
}
