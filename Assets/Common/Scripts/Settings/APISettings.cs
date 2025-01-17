using UnityEngine;

namespace Common.Scripts.Settings
{
    [CreateAssetMenu(menuName = "API Settings")]
    public class APISettings : ScriptableObject
    {
        [SerializeField]
        private string baseURL;

        public string BaseURL => baseURL;
        
        [SerializeField] 
        private string controllersEndpoint;

        [SerializeField] 
        private string experimentParametersEndpoint;

        [SerializeField] 
        private string simulationEndpoint;

        [SerializeField] 
        private string apiToken;

        public string ApiToken => apiToken;
        
        public string GetControllersURL(string modelId)
        {
            return baseURL + controllersEndpoint + modelId;
        }
        
        public string GetExperimentParametersURL(string experimentId)
        {
            return baseURL + experimentParametersEndpoint + experimentId;
        }
        
        public string GetSimulationURL()
        {
            return baseURL + simulationEndpoint;
        }

        public void SetRemoteConfigurationToSettings(string baseUrlParam, string controllersEndpointParam,
            string experimentParametersEndpointParam, string simulationEndpointParam, string apiTokenParam)
        {
            baseURL = baseUrlParam;
            controllersEndpoint = controllersEndpointParam;
            experimentParametersEndpoint = experimentParametersEndpointParam;
            simulationEndpoint = simulationEndpointParam;
            apiToken = apiTokenParam;
        }
    }
}