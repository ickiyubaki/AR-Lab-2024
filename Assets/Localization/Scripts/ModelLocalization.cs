using UnityEngine;
using UnityEngine.Localization.Tables;

namespace Localization.Scripts
{
    public class ModelLocalization : MonoBehaviour, ILocalization
    {
        public string Label { get; private set; }
        
        [SerializeField] 
        private string labelKey = string.Empty;

        private void Start()
        {
            if (labelKey.Length == 0)
            {
                Debug.LogError($"A key reference is empty. ({name})");
            }
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
            Label = stringTable.GetEntry(labelKey)?.GetLocalizedString();
        }
    }
}
