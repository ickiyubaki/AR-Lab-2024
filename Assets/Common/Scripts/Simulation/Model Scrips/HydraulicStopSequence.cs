using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Common.Scripts.Simulation.Model_Scrips
{
    public class HydraulicStopSequence
    {
        private readonly float _duration;
        private readonly  GameObject[][] _outputValveGroups;
        private readonly  GameObject[][] _betweenValveGroups;
        private readonly  GameObject[][] _tankGroups;
        private readonly (GameObject[] tubes, float duration)[] _shorterTubeGroups;
        private readonly (GameObject[] tubes, float duration)[] _longerTubeGroups;

        public HydraulicStopSequence(
            GameObject[][] outputValveGroups, 
            GameObject[][] betweenValveGroups, 
            GameObject[][] tankGroups,
            (GameObject[] tubes, float duration)[] shorterTubeGroups,
            (GameObject[] tubes, float duration)[] longerTubeGroups,
            float duration)
        {
            _duration = duration;
            _tankGroups = tankGroups;
            _longerTubeGroups = longerTubeGroups;
            _shorterTubeGroups = shorterTubeGroups;
            _outputValveGroups = outputValveGroups;
            _betweenValveGroups = betweenValveGroups;
        }
        
        public void PlayStopSequence()
        {
            var sequence = DOTween.Sequence();

            // drain shorter tube line
            var st = DOTween.Sequence();
            foreach (var (tubes, duration) in _shorterTubeGroups)
            {
                st.Append(DrainWaterFromTubes(tubes, duration));
            }
            
            // drain longer tube line
            var lt = DOTween.Sequence();
            foreach (var (tubes, duration) in _longerTubeGroups)
            {
                lt.Append(DrainWaterFromTubes(tubes, duration));
            }
            
            // close between valves
            var valveSequence = DOTween.Sequence();
            foreach (var valves in _betweenValveGroups)
            {
                RotateValves(valveSequence, valves, 0);
            }

            sequence.Append(valveSequence);
            
            // open output valves
            valveSequence = DOTween.Sequence();
            foreach (var valves in _outputValveGroups)
            {
                RotateValves(valveSequence, valves, 90);
            }

            // drain water
            var waterSequence = DOTween.Sequence().AppendInterval(0.5f);
            foreach (var tanks in _tankGroups)
            {
                DrainWaterFromTanks(waterSequence, tanks);
            }

            sequence.Append(valveSequence.Join(st).Join(lt).Append(waterSequence));

            // close output valves
            valveSequence = DOTween.Sequence().AppendInterval(0.5f);
            foreach (var valves in _outputValveGroups)
            {
                RotateValves(valveSequence, valves, 0);
            }

            sequence.Append(valveSequence).Play();
        }
        
        private void DrainWaterFromTanks(Sequence sequence, IEnumerable<GameObject> tanks)
        {
            foreach (var platform in tanks)
            {
                sequence.Join(platform.transform.DOScale(new Vector3(1, 0, 1), 1f));
            }
        }
        
        private Sequence DrainWaterFromTubes(IEnumerable<GameObject> tubes, float duration)
        {
            var sequence = DOTween.Sequence();          
            foreach (var tube in tubes)
            {
                var drainPoint = tube.transform.Find("DrainPoint");

                if (drainPoint != null)
                {
                    sequence.Join(drainPoint.transform.DOScale(new Vector3(1, 0, 1), duration))
                        .Append(tube.transform.DOScale(new Vector3(1, 0, 1), 0))
                        .Append(drainPoint.transform.DOScale(new Vector3(1, 1, 1), 0));
                }
                else
                {
                    sequence.Join(tube.transform.DOScale(new Vector3(1, 0, 1), duration));
                }
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
