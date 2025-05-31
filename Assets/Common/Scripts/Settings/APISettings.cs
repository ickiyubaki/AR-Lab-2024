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

        public string GetControllersURL(string modelId)
        {
            return baseURL + controllersEndpoint + modelId + "/experiments";
        }
        
        public string GetExperimentParametersURL(string experimentId)
        {
            return baseURL + experimentParametersEndpoint + experimentId + "/unity";
        }
        
        public string GetSimulationURL()
        {
            return baseURL + simulationEndpoint;
        }

        public void SetRemoteConfigurationToSettings(string baseUrlParam, string controllersEndpointParam,
            string experimentParametersEndpointParam, string simulationEndpointParam)
        {
            baseURL = baseUrlParam;
            controllersEndpoint = controllersEndpointParam;
            experimentParametersEndpoint = experimentParametersEndpointParam;
            simulationEndpoint = simulationEndpointParam;
        }
    }
}
