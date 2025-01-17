namespace Localization.Scripts
{
    public readonly struct LocalizationKeyValue
    {
        public string Key { get; }
        public string DefaultValue { get; }

        public LocalizationKeyValue(string key, string defaultValue)
        {
            Key = key;
            DefaultValue = defaultValue;
        }

        public string GetLocalizedText() => LocalizationManager.GetStringTableEntryOrDefault(this);
    }
    
    public static class LocalizationKeyValuePairs
    {
        // UI
        public const string LoadingPlaceholderKey = "LOADING";
        public const string LoadingPlaceholderDefaultValue = "Loading...";

        public const string ChoosePlaceholderKey = "CHOOSE_PLACEHOLDER";
        public const string ChoosePlaceholderDefaultValue = "Choose...";
        
        public const string InputPlaceholderKey = "INPUT_PLACEHOLDER";
        public const string InputPlaceholderDefaultValue = "Enter value";

        public const string ScriptNotFoundKey = "SCRIPT_NOT_FOUND";
        public const string ScriptNotFoundDefaultValue = "Simulation script is not attached to 3D model";

        public const string ModelNotFoundKey = "MODEL_NOT_FOUND";
        public const string ModelNotFoundDefaultValue = "Selected model is not in the scene";

        public const string SelectModelKey = "SELECT_MODEL";
        public const string SelectModelDefaultValue = "Select model";

        // UX
        public const string InitializeKey = "INIT";
        public const string InitializeDefaultValue = "Initializing augmented reality.";
        
        public const string MotionKey = "MOTION";
        public const string MotionDefaultValue = "Try moving at a slower pace.";
        
        public const string LightKey = "LIGHT";
        public const string LightDefaultValue = "It’s too dark. Try going to a more well lit area.";
        
        public const string FeaturesKey = "FEATURES";
        public const string FeaturesDefaultValue = "Look for more textures or details in the area.";
        
        public const string UnsupportedKey = "UNSUPPORTED";
        public const string UnsupportedDefaultValue = "AR content is not supported.";
        
        public const string NoneKey = "NONE";
        public const string NoneDefaultValue = "Wait for tracking to begin.";
        
        public const string MoveDeviceKey = "MOVE_DEVICE";
        public const string MoveDeviceDefaultValue = "Move Device Slowly";
        
        public const string TapToPlaceKey = "TAP_TO_PLACE";
        public const string TapToPlaceDefaultValue = "Tap to Place 3D model";
        
        public const string TapToManipulateKey = "TAP_TO_MANIPULATE";
        public const string TapToManipulateDefaultValue = "To start object manipulation tap on a 3D model";
    }
}