using DG.Tweening;
using UnityEngine;

namespace Common.Scripts.Tweens
{
    public class TweenSequencer : MonoBehaviour
    {
        [SerializeField] 
        private TweenData[] tweenDataArray;
        private Sequence _sequence;
        private bool _sequenceTriggered;

        private void Awake()
        {
            _sequence = DOTween.Sequence();
            BuildSequence();
        }

        private void BuildSequence()
        {
            _sequence.Pause();
            _sequence.SetAutoKill(false);

            foreach (var t in tweenDataArray)
            {
                if (t.join)
                {
                    _sequence.Join(t.GetTween());
                }
                else
                {
                    _sequence.Append(t.GetTween());
                }
            }
        }

        public void StartSequence()
        {
            if (!_sequenceTriggered)
            {
                _sequence.PlayForward();
            }
            else
            {
                _sequence.PlayBackwards();
            }

            _sequenceTriggered = !_sequenceTriggered;
        }

        public void Close()
        {
            if (_sequenceTriggered)
            {
                _sequence.PlayBackwards();
                _sequenceTriggered = !_sequenceTriggered;
            }
        }
        
        public void Show()
        {
            if (!_sequenceTriggered)
            {
                _sequence.PlayForward();
                _sequenceTriggered = !_sequenceTriggered;
            }
        }
    }
}
