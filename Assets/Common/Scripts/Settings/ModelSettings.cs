using System.Collections.Generic;
using Common.Scripts.Simulation;
using Localization.Scripts;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace Common.Scripts.Settings
{
    [CreateAssetMenu(menuName = "Model Settings")]
    public class ModelSettings : ScriptableObject
    {
        [SerializeField] 
        [Tooltip("The id of model on the API server")]
        private string modelId;

        public string ModelId => modelId;

        [SerializeField] 
        private string localizationLabelKey = "";

        public string LocalizationLabelKey => localizationLabelKey;
        
        [SerializeField] 
        private string localizationDefaultValue = "";

        public string LocalizationDefaultValue => localizationDefaultValue;
        
        [SerializeField] 
        private Sprite modelIcon;
        
        public Sprite ModelIcon => modelIcon;
        
        [SerializeField]
        private GameObject modelPrefab;

        public GameObject ModelPrefab => modelPrefab;

        public string Label { get; private set; }

        public List<Controller> Controllers { get; set; } = new List<Controller>();

        public LocalizationKeyValue GetLocalizationKeyValue()
        {
            return new LocalizationKeyValue(localizationLabelKey, localizationDefaultValue);
        }
        
        private void OnEnable()
        {
            LocalizationManager.LocalizationChange += OnLocalizationChange;
        }

        private void OnDisable()
        {
            LocalizationManager.LocalizationChange -= OnLocalizationChange;
        }

        private void OnLocalizationChange(StringTable stringTable)
        {
            Label = stringTable.GetEntry(localizationLabelKey)?.GetLocalizedString() ?? localizationDefaultValue;
        }
    }
}