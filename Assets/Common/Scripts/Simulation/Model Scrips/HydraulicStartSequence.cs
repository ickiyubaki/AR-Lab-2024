using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Common.Scripts.Simulation.Model_Scrips
{
    public class HydraulicStartSequence
    {
        private readonly float _duration;
        private readonly (GameObject[] valves, float rotation)[] _valveGroups;
        private readonly (GameObject[] tubes, float duration)[] _shorterTubeGroups;
        private readonly (GameObject[] tubes, float duration)[] _longerTubeGroups;

        public HydraulicStartSequence((GameObject[] valves, float rotation)[] valveGroups,
            (GameObject[] tubes, float duration)[] shorterTubeGroups,
            (GameObject[] tubes, float duration)[] longerTubeGroups,
            float duration)
        {
            _duration = duration;
            _valveGroups = valveGroups;
            _shorterTubeGroups = shorterTubeGroups;
            _longerTubeGroups = longerTubeGroups;
        }

        public void PlayStartSequence()
        {
            var sequence = DOTween.Sequence();

            // fill shorter tube line
            var st = DOTween.Sequence();
            foreach (var (tubes, duration) in _shorterTubeGroups)
            {
                st.Append(FillTubesWithWater(tubes, duration));
            }

            // fill longer tube line
            var lt = DOTween.Sequence();
            foreach (var (tubes, duration) in _longerTubeGroups)
            {
                lt.Append(FillTubesWithWater(tubes, duration));
            }

            sequence.Join(st).Join(lt);

            // open valves
            foreach (var (valves, rotation) in _valveGroups)
            {
                RotateValves(sequence, valves, rotation);
            }

            sequence.Play();
        }

        private Sequence FillTubesWithWater(IEnumerable<GameObject> tubes, float duration)
        {
            var sequence = DOTween.Sequence();
            foreach (var tube in tubes)
            {
                sequence.Join(tube.transform.DOScale(new Vector3(1, 1, 1), duration));
            }
            return sequence;
        }

        private void RotateValves(Sequence sequence, IEnumerable<GameObject> valves, float rotation)
        {
            foreach (var valve in valves)
            {
                sequence.Join(valve.transform.DOLocalRotate(new Vector3(0, 0, rotation), _duration));
            }
        }
    }
}