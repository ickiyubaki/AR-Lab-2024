using System;
using Common.Scripts.API;
using Unity.RemoteConfig;
using UnityEngine;

namespace Common.Scripts.Utils
{
    public class RemoteConfigManager : MonoBehaviour
    {
        public struct UserAttributes {}

        public struct AppAttributes{}
        
        private void Awake()
        {
            FetchRemoteConfiguration();
        }

        private void FetchRemoteConfiguration()
        {
            if (gameObject.activeInHierarchy)
            {
                ConfigManager.FetchCompleted += ApplyRemoteSettings;
                ConfigManager.FetchConfigs(new UserAttributes(), new AppAttributes());
            }
        }

        private void ApplyRemoteSettings(ConfigResponse configResponse)
        {
            switch (configResponse.requestOrigin)
            {
                case ConfigOrigin.Default:
                    Debug.Log("No settings loaded this session, using default values");
                    break;
                case ConfigOrigin.Cached:
                    Debug.Log("No settings loaded this session, using cached values from a previous session");
                    break;
                case ConfigOrigin.Remote:
                    Debug.Log("New settings loaded this session");
                    SetApiSettings();
                    // SetModelSettings();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // private void SetModelSettings()
        // {
        //     foreach (var instanceAvailable3DModel in Models.Instance.Available3DModels)
        //     {
        //         instanceAvailable3DModel.SetRemoteConfigurationToSettings();
        //     }
        // }

        private void SetApiSettings()
        {
            var baseUrl = ConfigManager.appConfig.GetString("ApiBaseURL");
            var controllersEndpoint = ConfigManager.appConfig.GetString("ControllersEndpoint");
            var experimentParametersEndpoint = ConfigManager.appConfig.GetString("ExperimentParametersEndpoint");
            var simulationEndpoint = ConfigManager.appConfig.GetString("SimulationEndpoint");

            // Updated: removed apiToken
            ApiClient.Instance.APISettings.SetRemoteConfigurationToSettings(
                baseUrl,
                controllersEndpoint,
                experimentParametersEndpoint,
                simulationEndpoint
            );
        }

        private void OnDestroy()
        {
            ConfigManager.FetchCompleted -= ApplyRemoteSettings;
        }
    }
}
