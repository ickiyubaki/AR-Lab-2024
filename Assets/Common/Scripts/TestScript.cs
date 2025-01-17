using System;
using Common.Scripts.AR;
using Common.Scripts.Settings;
using Common.Scripts.UI;
using Common.Scripts.Utils;
using DG.Tweening;
using UnityEngine;

namespace Common.Scripts
{
    public class TestScript : MonoBehaviour
    {
        public ARPlacementInteractableMultiple arip;
        
        // public GameObject scaleGO;
        public GameObject[] go;

        private void Start()
        {
            var sequence = DOTween.Sequence();

            // Debug.Log(scaleGO.transform.position);
            // Debug.Log(scaleGO.transform.localPosition);
            //
            // sequence.Join(scaleGO.transform.DOScaleY(0.1f, 5f));
            // sequence.Join(scaleGO.transform.DOLocalMoveY( scaleGO.transform.position.y * 1.5f, 5f));
            
            // Debug.Log(scaleGO.GetComponent<Renderer>());
        }

        public void SpawnPrefab(int index)
        {
            var obj = Instantiate(go[index]);
            obj.transform.position = new Vector3(0, -0.412f, 2.01f);
            
            ARPlacementInteractableMultiple.Instantiated3DModelsInScene.Add(obj);
            
            Models.Instance.SelectedModel = obj;
        }
        
        public void DeleteAll()
        {
            ARPlacementInteractableMultiple.Instantiated3DModelsInScene.Clear(); 
        }
        
        public static void InvokeSetModel()
        {
            Models.Instance.SelectedModel = ARPlacementInteractableMultiple.Instantiated3DModelsInScene[0];
        }

        public static void SetPreviousModel()
        {
           Models.Instance.SelectedModel = ARPlacementInteractableMultiple.Instantiated3DModelsInScene[
                ARPlacementInteractableMultiple.Instantiated3DModelsInScene.Count - 2];
        }
    }
}
