using TMPro;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace Localization.Scripts
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UILocalizationEditable : MonoBehaviour, ILocalization
    {
        public string Key { get; private set; } = "";
        public string DefaultValue { get; private set; } = "";

        private TextMeshProUGUI _textField;

        private void Awake()
        {
            _textField = GetComponent<TextMeshProUGUI>();
        }
        
        public void SetKeyAndValue(LocalizationKeyValue localization)
        {
            Key = localization.Key;
            DefaultValue = localization.DefaultValue;
            
            if (_textField == null)
            {
                _textField = GetComponent<TextMeshProUGUI>();
            }
            
            _textField.text = localization.GetLocalizedText();
        }

        private void OnEnable()
        {
            LocalizationManager.LocalizationChange += OnLocalizationChange;
        }

        private void OnDisable()
        {
            LocalizationManager.LocalizationChange -= OnLocalizationChange;
        }
        
        public void OnLocalizationChange(StringTable stringTable)
        {
            _textField.text = stringTable.GetEntry(Key)?.GetLocalizedString() ?? DefaultValue;
        }
    }
}