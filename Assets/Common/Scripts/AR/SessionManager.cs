using System;
using Common.Scripts.UI;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Common.Scripts.AR
{
    public class SessionManager : MonoBehaviour
    {

        [SerializeField] 
        private ARSession arSession;
        
        private Graph _graph;
        private Models _models;
        private SimulationMenu _simulationMenu;
        private ARPlacementInteractableMultiple _arPlacementInteractable;

        private void Start()
        {
            _graph = FindObjectOfType<Graph>();
            _models = FindObjectOfType<Models>();
            _simulationMenu = FindObjectOfType<SimulationMenu>();
            _arPlacementInteractable = FindObjectOfType<ARPlacementInteractableMultiple>();

        }

        public void ResetScene()
        {
            _graph.ResetGraph();
            _models.ResetSelectedModel();
            _simulationMenu.ResetSimulationMenu();
            _arPlacementInteractable.RemoveAllObject();
            arSession.Reset();
        }
    }
}
