using System.Collections;
using System.Collections.Generic;
using Common.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Common.Scripts.UI
{
    public readonly struct ToastMessage
    {
        public readonly Sprite Icon;
        public readonly string Message;
        public readonly float Duration;

        public ToastMessage(Sprite icon, string message, float duration)
        {
            Icon = icon;
            Message = message;
            Duration = duration;
        }
    }

    public class Toast : Singleton<Toast>
    {
        [SerializeField] 
        private Sprite infoIcon;
        [SerializeField] 
        private Sprite errorIcon;
        [SerializeField] 
        private Image messageIcon;
        [SerializeField] 
        private TMP_Text messageText;
        [SerializeField] 
        private GameObject messageBoxPrefab;
        
        private readonly Queue<ToastMessage> _messages = new Queue<ToastMessage>();

        private void Update()
        {
            if (_messages.Count != 0 && !messageBoxPrefab.activeSelf)
            {
                var toastMessage = _messages.Dequeue();
                messageText.text = toastMessage.Message;
                messageIcon.sprite = toastMessage.Icon;
                messageBoxPrefab.SetActive(true);
                StartCoroutine(DeactivateAfterSeconds(toastMessage.Duration));
            }
        }

        public void ShowCustomMessage(Sprite icon, string msg, float duration)
        {
            _messages.Enqueue(new ToastMessage(icon, msg, duration));
        }

        public void ShowInfoMessage(string msg, float duration)
        {
            _messages.Enqueue(new ToastMessage(infoIcon, msg, duration));
        }

        public void ShowErrorMessage(string msg, float duration)
        {
            _messages.Enqueue(new ToastMessage(errorIcon, msg, duration));
        }

        private IEnumerator DeactivateAfterSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            messageBoxPrefab.SetActive(false);
        }
    }
}
