using UnityEngine;
using UnityEngine.Localization.Tables;

namespace Localization.Scripts
{
    public class ReasonsLocalization : MonoBehaviour, ILocalization
    {
        public string LocalizedInit { get; private set; }
        public string LocalizedMotion { get; private set; }
        public string LocalizedLight { get; private set; }
        public string LocalizedFeatures { get; private set; }
        public string LocalizedUnsupported { get; private set; }
        public string LocalizedNone { get; private set; }
        public string LocalizedMoveDevice { get; private set; }
        public string LocalizedTapToPlace { get; private set; }

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
            LocalizedInit = stringTable.GetEntry(LocalizationKeyValuePairs.InitializeKey).GetLocalizedString();
            LocalizedMotion = stringTable.GetEntry(LocalizationKeyValuePairs.MotionKey).GetLocalizedString();
            LocalizedLight = stringTable.GetEntry(LocalizationKeyValuePairs.LightKey).GetLocalizedString();
            LocalizedFeatures = stringTable.GetEntry(LocalizationKeyValuePairs.FeaturesKey).GetLocalizedString();
            LocalizedUnsupported = stringTable.GetEntry(LocalizationKeyValuePairs.UnsupportedKey).GetLocalizedString();
            LocalizedNone = stringTable.GetEntry(LocalizationKeyValuePairs.NoneKey).GetLocalizedString();
            LocalizedMoveDevice = stringTable.GetEntry(LocalizationKeyValuePairs.MoveDeviceKey).GetLocalizedString();
            LocalizedTapToPlace = stringTable.GetEntry(LocalizationKeyValuePairs.TapToPlaceKey).GetLocalizedString();
        }
    }
}
