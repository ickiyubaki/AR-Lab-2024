using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Scripts.API;
using Common.Scripts.AR;
using Common.Scripts.Settings;
using Common.Scripts.Tweens;
using Common.Scripts.UI;
using Common.Scripts.Utils;
using Localization.Scripts;
using UnityEngine;

namespace Common.Scripts.Simulation
{
    public class SimulationManager : Singleton<SimulationManager>
    {

        [SerializeField] 
        private GameObject loadingIcon;

        [SerializeField] 
        private GameObject graphButton;

        [SerializeField] 
        private TweenSequencer graphWindowsTween;
        
        private ApiClient _apiClient ;
        private ModelSettings _selectedModelSettings;

        private void Start()
        {
            _apiClient = ApiClient.Instance;
            loadingIcon.SetActive(false);
        }

        public async Task<List<Controller>> PickModel(ModelSettings selectedModel)
        {
            _selectedModelSettings = selectedModel;
            
            if (!_selectedModelSettings.Controllers.Any())
            {
                var response = await _apiClient.RequestDataAsync<ControllersResponse>(HttpMethod.Get,
                    _apiClient.APISettings.GetControllersURL(_selectedModelSettings.ModelId));
                var controllers = response.Data;
                
                foreach (var controller in controllers)
                {
                    controller.ExperimentData = await _apiClient.RequestDataAsync<ExperimentData>(HttpMethod.Get,
                        _apiClient.APISettings.GetExperimentParametersURL(controller.ExperimentId));
                }

                if (controllers.Any())
                {
                    _selectedModelSettings.Controllers = controllers; 
                }
            }

            return _selectedModelSettings.Controllers;
        }

        public async void StartSimulation(GameObject model, string experimentId, Dictionary<string, string> parameters, ExperimentData experimentData)
        {
            if (ARPlacementInteractableMultiple.Instantiated3DModelsInScene.Any(m =>
                m.GetInstanceID().Equals(model.GetInstanceID())))
            {
                var simulation = model.GetComponent<ISimulation>();
                if (simulation != null)
                {
                    loadingIcon.SetActive(true);
                    Graph.Instance.ResetGraph();
                    NativeDataShare.Instance.ResetParameters();
                    loadingIcon.GetComponentInChildren<UILocalization>()?.UpdateText();

                    try
                    {
                       
                        Debug.Log($"[SimulationManager] Using experimentId: {experimentId}");
                        Debug.Log($"[SimulationManager] Simulation URL: {ApiClient.Instance.APISettings.GetSimulationURL(experimentId)}");

                        if (string.IsNullOrEmpty(experimentId))
                        {
                            throw new ApiException { StatusCode = 400, Content = "Experiment ID is missing." };
                        }

                        var responseType = typeof(SimulationResponse<>).MakeGenericType(
                            simulation.GetSimulationDataType().GetGenericArguments()[0]);
                        var simulationResponse = await ApiClient.Instance.RequestDataAsync(
                            responseType, HttpMethod.Post,
                            ApiClient.Instance.APISettings.GetSimulationURL(experimentId), parameters);

                        var simulationDataProperty = simulationResponse.GetType().GetProperty("Simulation");
                        var simulationData = simulationDataProperty.GetValue(simulationResponse);

                        // Cast simulationData to the expected type
                        var expectedType = simulation.GetSimulationDataType();
                        if (!expectedType.IsInstanceOfType(simulationData))
                        {
                            Debug.LogError($"Type mismatch: simulationData is {simulationData.GetType()}, expected {expectedType}");
                            loadingIcon.SetActive(false);
                            return;
                        }

                        loadingIcon.SetActive(false);
                        graphButton.SetActive(true);
                        graphWindowsTween.Show();
                        simulation.GetType().GetMethod(nameof(Simulation<SimulationData>.StartSimulation))
                            ?.Invoke(simulation, new[] { simulationData, experimentData, parameters });
                    }
                    catch (ApiException ex)
                    {
                        DisableLoading();
                        Toast.Instance.ShowErrorMessage("ERROR: " + ex.StatusCode, 3f);
                    }
                }
                else
                {
                    Toast.Instance.ShowErrorMessage(
                        LocalizationManager.GetStringTableEntryOrDefault(LocalizationKeyValuePairs.ScriptNotFoundKey,
                            LocalizationKeyValuePairs.ScriptNotFoundDefaultValue), 5f);
                }
            }
            else
            {
                Toast.Instance.ShowErrorMessage(
                    LocalizationManager.GetStringTableEntryOrDefault(LocalizationKeyValuePairs.ModelNotFoundKey,
                        LocalizationKeyValuePairs.ModelNotFoundDefaultValue), 5f);
            }
        }

        public void DisableLoading()
        {
            loadingIcon.SetActive(false);
        }
    }
}