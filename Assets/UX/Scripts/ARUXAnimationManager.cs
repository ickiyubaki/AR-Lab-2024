using System;
using Localization.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace UX.Scripts
{
    public class ARUXAnimationManager : MonoBehaviour
    {
        [SerializeField] 
        private Image instructionPanel;
        
        [SerializeField] 
        private Image instructionBox;
        
        [SerializeField]
        private TMP_Text instructionText;

        [SerializeField]
        [Tooltip("Move device animation")]
        private VideoClip findAPlaneClip;

        [SerializeField]
        [Tooltip("Tap to place animation")]
        private VideoClip tapToPlaceClip;

        [SerializeField]
        [Tooltip("Video player reference")]
        private VideoPlayer videoPlayer;
        
        [SerializeField]
        [Tooltip("Raw image used for video player reference")]
        private RawImage rawImage;
        
        [SerializeField]
        [Tooltip("time the UI takes to fade on")]
        private float fadeOnDuration = 1.0f;
        
        [SerializeField]
        [Tooltip("time the UI takes to fade off")]
        private float fadeOffDuration = 0.5f;

        [SerializeField] 
        private Texture transparent;

        [SerializeField] 
        private ReasonsLocalization reasonsLocalization;
        
        [SerializeField] 
        private bool localizeText = true;
        
        public static event Action OnFadeOffComplete;
        
        private readonly Color _alphaWhite = new Color(1,1,1,0);
        private readonly Color _white = new Color(1,1,1,1);
        
        private readonly Color _alphaBlack = new Color(0,0,0,0);
        private readonly Color _black = new Color(0,0,0,0.4f);

        private RenderTexture _renderTexture;
        private bool _isClipNotNull;
        private Color _targetColor;
        private Color _startColor;
        private Color _lerpingColor;
        
        private Color _targetColorBackground;
        private Color _startColorBackground;
        private Color _lerpingColorBackground;
        
        private bool _fadeOn;
        private bool _fadeOff;
        private bool _tweening;
        private float _tweenTime;
        private float _tweenDuration;

        public bool LocalizeText
        {
            get => localizeText;
            set => localizeText = value;
        }

        private void Start()
        {
            _isClipNotNull = videoPlayer.clip != null;
            _startColor = _alphaWhite;
            _targetColor = _white;
        }

        private void Update()
        {
            if (!videoPlayer.isPrepared)
            {
                return;
            }

            if (_fadeOff || _fadeOn)
            {
                if (_fadeOn)
                {
                    instructionPanel.gameObject.SetActive(true);
                    
                    _startColor = _alphaWhite;
                    _startColorBackground = _alphaBlack;
                    _targetColor = _white;
                    _targetColorBackground = _black;
                    
                    _tweenDuration = fadeOnDuration;
                    _fadeOff = false;
                }
        
                if(_fadeOff)
                {
                    _startColor = _white;
                    _startColorBackground = _black;
                    _targetColor = _alphaWhite;
                    _targetColorBackground = _alphaBlack;
                   
                    _tweenDuration = fadeOffDuration;
                    _fadeOn = false;
                }
            
                if (_tweenTime < 1)
                {
                    _tweenTime += Time.deltaTime / _tweenDuration;
                    _lerpingColor = Color.Lerp(_startColor, _targetColor, _tweenTime);
                    _lerpingColorBackground = Color.Lerp(_startColorBackground, _targetColorBackground, _tweenTime);
                    
                    rawImage.color = _lerpingColor;
                    instructionBox.color = _lerpingColor;
                    instructionText.color = _lerpingColor;
                    instructionPanel.color = _lerpingColorBackground;
                    
                    _tweening = true;
                }
                else
                {
                    _tweenTime = 0;
                    _fadeOff = false;
                    _fadeOn = false;
                    _tweening = false;
 
                    // was it a fade off?
                    if (_targetColor == _alphaWhite && _targetColorBackground == _alphaBlack)
                    {
                        OnFadeOffComplete?.Invoke();

                        // fix issue with render texture showing a single frame of the previous video
                        _renderTexture = videoPlayer.targetTexture;
                        _renderTexture.DiscardContents();
                        _renderTexture.Release();
                        Graphics.Blit(transparent, _renderTexture);
                        instructionPanel.gameObject.SetActive(false);
                    }
                }
            }
        }

        public void ShowTapToPlace()
        {
            videoPlayer.clip = tapToPlaceClip;
            videoPlayer.Play();
            instructionText.text = localizeText
                ? reasonsLocalization.LocalizedTapToPlace
                : LocalizationKeyValuePairs.TapToPlaceDefaultValue;
            _fadeOn = true;
        }

        public void UpdateTapToPlaceText()
        {
            instructionText.text = localizeText
                ? reasonsLocalization.LocalizedTapToPlace
                : LocalizationKeyValuePairs.TapToPlaceDefaultValue;
        }

        public void ShowCrossPlatformFindAPlane()
        {
            videoPlayer.clip = findAPlaneClip;
            videoPlayer.Play();
            instructionText.text = localizeText
                ? reasonsLocalization.LocalizedMoveDevice
                : LocalizationKeyValuePairs.MoveDeviceDefaultValue;
            _fadeOn = true;
        }

        public void UpdateShowCrossPlatformFindAPlaneText()
        {
            instructionText.text = localizeText
                ? reasonsLocalization.LocalizedMoveDevice
                : LocalizationKeyValuePairs.MoveDeviceDefaultValue;
        }

        public void FadeOffCurrentUI()
        {
            if (_isClipNotNull)
            {
                // handle exiting fade out early if currently fading out another Clip
                if (_tweening || _fadeOn)
                {
                    // stop tween immediately
                    _tweenTime = 1.0f;
                    rawImage.color = _alphaWhite;
                    instructionText.color = _alphaWhite;
                    instructionBox.color = _alphaWhite;
                    instructionPanel.color = _alphaBlack;
                    OnFadeOffComplete?.Invoke();
                }

                _fadeOff = true;
            }
        }
    }
}
