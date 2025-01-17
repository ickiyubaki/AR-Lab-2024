using Localization.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UX.Scripts
{
    public class ARUXReasonsManager : MonoBehaviour
    {
        [SerializeField] 
        private bool showNotTrackingReasons = true;
    
        public bool ShowNotTrackingReasons
        {
            get => showNotTrackingReasons;
            set => showNotTrackingReasons = value;
        }

        [SerializeField] 
        private TMP_Text reasonDisplayText;

        public TMP_Text ReasonDisplayText
        {
            get => reasonDisplayText;
            set => reasonDisplayText = value;
        }

        [SerializeField] 
        private GameObject reasonParent;
    
        public GameObject ReasonParent
        {
            get => reasonParent;
            set => reasonParent = value;
        }

        [SerializeField] 
        private Image reasonIcon;
    
        public Image ReasonIcon
        {
            get => reasonIcon;
            set => reasonIcon = value;
        }

        [SerializeField] 
        private Sprite initRelocalSprite;

        public Sprite InitRelocalSprite
        {
            get => initRelocalSprite;
            set => initRelocalSprite = value;
        }

        [SerializeField] 
        private Sprite motionSprite;

        public Sprite MotionSprite
        {
            get => motionSprite;
            set => motionSprite = value;
        }

        [SerializeField] private Sprite lightSprite;

        public Sprite LightSprite
        {
            get => lightSprite;
            set => lightSprite = value;
        }

        [SerializeField] 
        private Sprite featuresSprite;

        public Sprite FeaturesSprite
        {
            get => featuresSprite;
            set => featuresSprite = value;
        }

        [SerializeField] 
        private Sprite unsupportedSprite;

        public Sprite UnsupportedSprite
        {
            get => unsupportedSprite;
            set => unsupportedSprite = value;
        }

        [SerializeField] 
        private Sprite noneSprite;

        public Sprite NoneSprite
        {
            get => noneSprite;
            set => noneSprite = value;
        }

        [SerializeField] 
        private ReasonsLocalization reasonsLocalization;
    
        public ReasonsLocalization ReasonsLocalization
        {
            get => reasonsLocalization;
            set => reasonsLocalization = value;
        }

        [SerializeField] 
        private bool localizeText = true;

        public bool LocalizeText
        {
            get => localizeText;
            set => localizeText = value;
        }

        private NotTrackingReason _currentReason;
        private bool _sessionTracking;

        private void OnEnable()
        {
            ARSession.stateChanged += ARSessionOnStateChanged;
            if (!showNotTrackingReasons)
            {
                reasonParent.SetActive(false);
            }
        }

        private void OnDisable()
        {
            ARSession.stateChanged -= ARSessionOnStateChanged;
        }

        private void Update()
        {
            if (showNotTrackingReasons)
            {
                if (!_sessionTracking)
                {
                    _currentReason = ARSession.notTrackingReason;
                    ShowReason();
                }
                else
                {
                    if (reasonDisplayText.gameObject.activeSelf)
                    {
                        reasonParent.SetActive(false);
                    }
                }
            }
        }

        private void ARSessionOnStateChanged(ARSessionStateChangedEventArgs obj)
        {
            _sessionTracking = obj.state == ARSessionState.SessionTracking;
        }

        private void ShowReason()
        {
            reasonParent.SetActive(true);
            SetReason();
        }

        private void SetReason()
        {
            switch (_currentReason)
            {
                case NotTrackingReason.Initializing:
                case NotTrackingReason.Relocalizing:
                    reasonDisplayText.text = localizeText
                        ? reasonsLocalization.LocalizedInit
                        : LocalizationKeyValuePairs.InitializeDefaultValue;
                    reasonIcon.sprite = initRelocalSprite;
                    break;
                case NotTrackingReason.ExcessiveMotion:
                    reasonDisplayText.text = localizeText
                        ? reasonsLocalization.LocalizedMotion
                        : LocalizationKeyValuePairs.MotionDefaultValue;
                    reasonIcon.sprite = motionSprite;
                    break;
                case NotTrackingReason.InsufficientLight:
                    reasonDisplayText.text = localizeText
                        ? reasonsLocalization.LocalizedLight
                        : LocalizationKeyValuePairs.LightDefaultValue;
                    reasonIcon.sprite = lightSprite;
                    break;
                case NotTrackingReason.InsufficientFeatures:
                    reasonDisplayText.text = localizeText
                        ? reasonsLocalization.LocalizedFeatures
                        : LocalizationKeyValuePairs.FeaturesDefaultValue;
                    reasonIcon.sprite = featuresSprite;
                    break;
                case NotTrackingReason.Unsupported:
                    reasonDisplayText.text = localizeText
                        ? reasonsLocalization.LocalizedUnsupported
                        : LocalizationKeyValuePairs.UnsupportedDefaultValue;
                    reasonIcon.sprite = unsupportedSprite;
                    break;
                case NotTrackingReason.None:
                    reasonDisplayText.text = localizeText
                        ? reasonsLocalization.LocalizedNone
                        : LocalizationKeyValuePairs.NoneDefaultValue;
                    reasonIcon.sprite = noneSprite;
                    break;
            }
        }

        public void TestForceShowReason(NotTrackingReason reason)
        {
            _currentReason = reason;
            reasonParent.SetActive(true);
            SetReason();
        }
    }
}
