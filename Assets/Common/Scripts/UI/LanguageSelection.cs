using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Common.Scripts.UI
{
    public class LanguageSelection : MonoBehaviour
    {
        
        [SerializeField]
        private GameObject languageToggle;
        private Transform _radioMenu;
        private Dictionary<string, int> _languages;
        private ToggleGroup _toggleGroup;
        private AsyncOperationHandle _initializeOperation;

        private void Start()
        {
            _languages = new Dictionary<string, int>();
            _radioMenu = gameObject.transform;
            _toggleGroup = GetComponent<ToggleGroup>();
            DestroyChildren(_radioMenu);
            
            _initializeOperation = LocalizationSettings.SelectedLocaleAsync;

            if (_initializeOperation.IsDone)
            {
                InitializeCompleted(_initializeOperation);
            }
            else
            {
                _initializeOperation.Completed += InitializeCompleted;
            }
        }
        
        private void InitializeCompleted(AsyncOperationHandle obj)
        {
            
            var locales = LocalizationSettings.AvailableLocales.Locales;
            for (var i = 0; i < locales.Count; ++i)
            {
                var locale = locales[i];
                var localeCode = locale.Identifier.Code;
                var localeNativeName = locale.Identifier.CultureInfo.NativeName;
                _languages.Add(localeCode, i);
                
                languageToggle.GetComponent<Toggle>().group = _toggleGroup;
                var languageOption = Instantiate(languageToggle, _radioMenu);
                languageOption.name = localeCode;
                languageOption.GetComponent<Toggle>().isOn = LocalizationSettings.SelectedLocale == locale;
                languageOption.GetComponentInChildren<TextMeshProUGUI>().text = char.ToUpper(localeNativeName[0]) + localeNativeName.Substring(1);
                // languageOption.transform.localPosition = new Vector2(0, 0 - 110 * i);
            }
        }

        public void Submit()
        {
            var selectedToggle = _toggleGroup.ActiveToggles().FirstOrDefault();
            if (selectedToggle != null)
            {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[_languages[selectedToggle.name]];
            }
        }
        
        private void DestroyChildren(Transform transformParam)
        {
            for (var i = transformParam.childCount - 1; i >= 0; --i)
            {
                Destroy(transformParam.GetChild(i).gameObject);
            }
        }
    }
}
