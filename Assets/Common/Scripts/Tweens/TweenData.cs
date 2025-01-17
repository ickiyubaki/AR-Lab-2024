using System;
using DG.Tweening;
using UnityEngine;

namespace Common.Scripts.Tweens
{
    [Serializable]
    public class TweenData
    {
        public enum TweenType
        {
            Move,
            Rotate,
            Scale
        }

        public TweenType tweenType;
        public RectTransform objectToTween;
        public Vector2 positionOffset;
        public float duration = 1f;
        public Vector3 targetRotation;
        public float targetScale = 1f;
        public bool join;

        public Tween GetTween()
        {
            Tween tween;

            switch (tweenType)
            {
                case TweenType.Move:
                    var targetPosition =
                        objectToTween.anchoredPosition + new Vector2(positionOffset.x, positionOffset.y);
                    tween = objectToTween.DOAnchorPos(targetPosition, duration);
                    break;
                case TweenType.Rotate:
                    tween = objectToTween.DORotate(targetRotation, duration);
                    break;
                case TweenType.Scale:
                    tween = objectToTween.DOScale(targetScale, duration);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return tween;
        }
    }
}
