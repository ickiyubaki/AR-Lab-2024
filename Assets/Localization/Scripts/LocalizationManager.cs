using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UX.Scripts;

namespace Localization.Scripts
{    
    public class LocalizationManager : MonoBehaviour
    {
        public static event Action<StringTable> LocalizationChange;

        private static StringTable activeLocalizationStringTable;

        public bool LocalizationComplete { get; private set; }

        [SerializeField] 
        private SharedTableData stringSharedTableData;

        [SerializeField]
        private ARUXAnimationManager animationManager;
        
        [SerializeField]
        private ARUXReasonsManager reasonsManager;
        
        private IEnumerator Start()
        {
            var localizeAnimation = false;
            var localizeReasons = false;

            
            if (animationManager)
            {
                localizeAnimation = animationManager.LocalizeText;
            }

            if (reasonsManager)
            {
                localizeReasons = reasonsManager.LocalizeText;
            }

            if (localizeAnimation || localizeReasons)
            {
                yield return LocalizationSettings.InitializationOperation;
                LocalizationSettings.SelectedLocaleChanged += ChangeLocalization;
                ChangeLocalization(LocalizationSettings.SelectedLocale);
            }
        }
        
        private void ChangeLocalization(Locale locale)
        {            
            LocalizationSettings.SelectedLocale = locale;  
            LocalizationSettings.StringDatabase.GetTableAsync(stringSharedTableData.TableCollectionName).Completed +=
                OnCompletedLocalizationChange;
        }

        private void OnCompletedLocalizationChange(AsyncOperationHandle<StringTable> obj)
        {
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                activeLocalizationStringTable = obj.Result;
                LocalizationChange?.Invoke(obj.Result);
                LocalizationComplete = true;
            }
        }

        public static string GetStringTableEntryOrDefault(string key, string defaultValue)
        {
            return key == null || activeLocalizationStringTable == null ||
                   activeLocalizationStringTable.GetEntry(key) == null
                ? defaultValue
                : activeLocalizationStringTable.GetEntry(key).Value;
        }
        
        public static string GetStringTableEntryOrDefault(LocalizationKeyValue localization)
        {
            return localization.Key == null || activeLocalizationStringTable == null ||
                   activeLocalizationStringTable.GetEntry(localization.Key) == null
                ? localization.DefaultValue
                : activeLocalizationStringTable.GetEntry(localization.Key).Value;
        }
    }
}
