using System;
using System.Collections.Generic;
using System.Linq;
using Common.Scripts.AR;
using Common.Scripts.Settings;
using Common.Scripts.Utils;
using UnityEngine;

namespace Common.Scripts
{
    public class Models : Singleton<Models>
    {
        public static event Action<GameObject, ModelSettings> OnModelSelected;
        public static event Action<GameObject> OnModelDeselected;

        [SerializeField] private ModelSettings[] modelSettings;

        public List<ModelSettings> Available3DModels => modelSettings.ToList();
        public ModelSettings SelectedModelSettings { get; private set; }

        private GameObject _selectedModel;

        public GameObject SelectedModel
        {
            get => _selectedModel;
            set
            {
                if (_selectedModel != null && value == null)
                {
                    OnModelDeselected?.Invoke(_selectedModel);
                    _selectedModel = null;
                    SelectedModelSettings = null;
                }
                else if ( _selectedModel == null || value.GetInstanceID() != _selectedModel.GetInstanceID())
                {
                    _selectedModel = value;
                    SetSelectedModelSettings();
                    OnModelSelected?.Invoke(_selectedModel, SelectedModelSettings);
                }
            }
        }

        private void SetSelectedModelSettings()
        {
            SelectedModelSettings = _selectedModel != null
                ? Available3DModels.First(ms => ms.ModelPrefab.CompareTag(SelectedModel.tag))
                : null;
        }

        public void ResetSelectedModel()
        {
            SelectedModel = null;
        }

        private void OnModelRemoved(GameObject removedObject)
        {
            if (_selectedModel != null && _selectedModel.GetInstanceID() == removedObject.GetInstanceID())
            {
                ResetSelectedModel();
            }
        }
        
        private void OnEnable()
        {
            ARPlacementInteractableMultiple.OnObjectRemoved += OnModelRemoved;
        }

        private void OnDisable()
        {
            ARPlacementInteractableMultiple.OnObjectRemoved -= OnModelRemoved;
        }
    }
}