using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;

namespace Common.Scripts.Utils
{
    /// <summary>
    /// A component that can be used to access the most
    /// recently received light estimation information
    /// for the physical environment as observed by an
    /// AR device.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class LightEstimation : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The ARCameraManager which will produce frame events containing light estimation information.")]
        private ARCameraManager cameraManager;
    
        /// <summary>
        /// Get or set the <c>ARCameraManager</c>.
        /// </summary>
        public ARCameraManager CameraManager
        {
            get => cameraManager;
            set
            {
                if (cameraManager == value)
                    return;

                if (cameraManager != null)
                    cameraManager.frameReceived -= FrameChanged;

                cameraManager = value;

                if (cameraManager != null & enabled)
                    cameraManager.frameReceived += FrameChanged;
            }
        }

        /// <summary>
        /// The estimated brightness of the physical environment, if available.
        /// </summary>
        public float? Brightness { get; private set; }

        /// <summary>
        /// The estimated color temperature of the physical environment, if available.
        /// </summary>
        public float? ColorTemperature { get; private set; }

        /// <summary>
        /// The estimated color correction value of the physical environment, if available.
        /// </summary>
        public Color? ColorCorrection { get; private set; }

        /// <summary>
        /// The estimated direction of the main light of the physical environment, if available.
        /// </summary>
        public Vector3? MainLightDirection { get; private set; }

        /// <summary>
        /// The estimated color of the main light of the physical environment, if available.
        /// </summary>
        public Color? MainLightColor { get; private set; }

        /// <summary>
        /// The estimated intensity in lumens of main light of the physical environment, if available.
        /// </summary>
        public float? MainLightIntensityLumens { get; private set; }

        /// <summary>
        /// The estimated spherical harmonics coefficients of the physical environment, if available.
        /// </summary>
        public SphericalHarmonicsL2? SphericalHarmonics { get; private set; }
    
        private Light _light;
        
        [SerializeField]
        private float brightnessMod = 2.0f;

        private void Awake ()
        {
            _light = GetComponent<Light>();
        }

        private void OnEnable()
        {
            if (cameraManager != null)
                cameraManager.frameReceived += FrameChanged;
        }

        private void OnDisable()
        {
            if (cameraManager != null)
                cameraManager.frameReceived -= FrameChanged;
        }

        private void FrameChanged(ARCameraFrameEventArgs args)
        {
            if (args.lightEstimation.averageBrightness.HasValue)
            {
                Brightness = args.lightEstimation.averageBrightness.Value;
                _light.intensity = Brightness.Value * brightnessMod;
            }

            if (args.lightEstimation.averageColorTemperature.HasValue)
            {
                ColorTemperature = args.lightEstimation.averageColorTemperature.Value;
                _light.colorTemperature = ColorTemperature.Value;
            }
        
            if (args.lightEstimation.colorCorrection.HasValue)
            {
                ColorCorrection = args.lightEstimation.colorCorrection.Value;
                _light.color = ColorCorrection.Value;
            }

            if (args.lightEstimation.mainLightDirection.HasValue)
            {
                MainLightDirection = args.lightEstimation.mainLightDirection;
                _light.transform.rotation = Quaternion.LookRotation(MainLightDirection.Value);
            }

            if (args.lightEstimation.mainLightColor.HasValue)
            {
                MainLightColor = args.lightEstimation.mainLightColor;
                _light.color = MainLightColor.Value;
            }

            if (args.lightEstimation.mainLightIntensityLumens.HasValue)
            {
                MainLightIntensityLumens = args.lightEstimation.mainLightIntensityLumens;
                if (args.lightEstimation.averageMainLightBrightness != null)
                    _light.intensity = args.lightEstimation.averageMainLightBrightness.Value;
            }

            if (!args.lightEstimation.ambientSphericalHarmonics.HasValue) return;
            SphericalHarmonics = args.lightEstimation.ambientSphericalHarmonics;
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientProbe = SphericalHarmonics.Value;
        }
    }
}
