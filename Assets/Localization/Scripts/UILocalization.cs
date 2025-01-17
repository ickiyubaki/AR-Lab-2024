using TMPro;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace Localization.Scripts
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UILocalization : MonoBehaviour, ILocalization
    {
        [SerializeField] 
        private bool updateTextOnEnable;
        
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

        private TextMeshProUGUI _textField;

        private void OnEnable()
        {
            LocalizationManager.LocalizationChange += OnLocalizationChange;

            if (updateTextOnEnable)
            {
                UpdateText();
            }
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

            _textField = GetComponent<TextMeshProUGUI>();
        }

        public void OnLocalizationChange(StringTable stringTable)
        {
            _textField.text = stringTable.GetEntry(key)?.GetLocalizedString() ?? defaultValue;
        }

        public void UpdateText()
        {
            if (_textField == null)
            {
                _textField = GetComponent<TextMeshProUGUI>();
            }

            _textField.text = LocalizationManager.GetStringTableEntryOrDefault(key, defaultValue);
        }
    }
}