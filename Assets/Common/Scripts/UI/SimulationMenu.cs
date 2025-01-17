using System.Collections.Generic;
using System.Linq;
using Common.Scripts.API;
using Common.Scripts.AR;
using Common.Scripts.Settings;
using Common.Scripts.Simulation;
using Localization.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;

namespace Common.Scripts.UI
{
    public readonly struct ModelSelectedValues
    {
        public Controller Controller { get; }
        public Dictionary<InputParameter, string> UserSelectedValues { get; }

        public ModelSelectedValues(Controller controller)
        {
            Controller = controller;
            UserSelectedValues = new Dictionary<InputParameter, string>();
        }

        public void AddSelectedValue(KeyValuePair<InputParameter, string> valuePair)
        {
            UserSelectedValues.Add(valuePair.Key, valuePair.Value);
        }
    }

    public class SimulationMenu : MonoBehaviour
    {
        public const string IDParam = "id";

        [SerializeField] private GameObject infoMessage;
        [SerializeField] private GameObject modelInputGo;
        [SerializeField] private TMP_InputField modelInput;

        [SerializeField] private GameObject controllerDropdownGO;
        private TMP_Dropdown _controllerDropdown;

        [SerializeField] private Transform optionsTransform;
        [SerializeField] private GameObject dataInputPrefab;
        [SerializeField] private GameObject dataSelectPrefab;
        [SerializeField] private Button simulationButton;
        [SerializeField] private GameObject documentationButtonGO;

        private ModelSettings _selectedModelSetting;
        private GameObject _selectedModel;
        private Dictionary<int, ModelSelectedValues> _modelValuesMap;

        private List<Controller> _controllers;
        private Dictionary<string, string> _parameters;
        private string _documentationURL;
        private Controller _selectedController;

        private void Start()
        {
            _parameters = new Dictionary<string, string>();
            _modelValuesMap = new Dictionary<int, ModelSelectedValues>();
            _controllerDropdown = controllerDropdownGO.GetComponentInChildren<TMP_Dropdown>();
            _controllerDropdown.onValueChanged.AddListener(OnControllerSelectionChanged);
        }

        private void PopulateDropdown(TMP_Dropdown dropdown, List<string> options, int selectedOptionIndex = -1)
        {
            dropdown.ClearOptions();
            if (options.Count == 0)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(" - "));
                dropdown.interactable = false;
            }
            else
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(
                    LocalizationManager.GetStringTableEntryOrDefault(
                        LocalizationKeyValuePairs.ChoosePlaceholderKey,
                        LocalizationKeyValuePairs.ChoosePlaceholderDefaultValue)));
                dropdown.interactable = true;
            }

            dropdown.AddOptions(options);

            if (selectedOptionIndex >= 0 && dropdown.options.Count >= selectedOptionIndex + 2)
            {
                dropdown.SetValueWithoutNotify(selectedOptionIndex + 1);
            }
        }

        private void PopulateDataFields(IEnumerable<InputParameter> inputParameters,
            IReadOnlyDictionary<InputParameter, string> values = null)
        {
            foreach (var group in inputParameters.OrderBy(p => p.Group).ThenBy(p => p.Order).ToLookup(p => p.Group))
            {
                foreach (var param in group)
                {
                    if (!param.Group.Equals(DataGroup.Hidden))
                    {
                        var dataField = param.Type switch
                        {
                            DataType.Text => Instantiate(dataInputPrefab, optionsTransform),
                            DataType.Select => Instantiate(dataSelectPrefab, optionsTransform),
                            _ => null
                        };

                        if (dataField is null) continue;
                        if (values != null && values.TryGetValue(param, out var value))
                        {
                            dataField.GetComponent<IDataField>().SetData(param, value);
                        }
                        else
                        {
                            dataField.GetComponent<IDataField>().SetData(param);
                        }

                        dataField.name = param.SchemaVar;
                    }
                }
            }
        }

        public void OpenDocumentation()
        {
            if (!string.IsNullOrEmpty(_documentationURL))
            {
                Application.OpenURL(_documentationURL);
            }
        }

        // action methods

        private void OnEnable()
        {
            Models.OnModelSelected += OnModelSelected;
            Models.OnModelDeselected += OnModelDeselected;
            LocalizationManager.LocalizationChange += OnLocalizationChange;
            ARPlacementInteractableMultiple.OnObjectRemoved += OnModelRemoved;
        }

        private void OnDisable()
        {
            Models.OnModelSelected -= OnModelSelected;
            Models.OnModelDeselected -= OnModelDeselected;
            LocalizationManager.LocalizationChange -= OnLocalizationChange;
            ARPlacementInteractableMultiple.OnObjectRemoved -= OnModelRemoved;
        }

        private void OnControllerSelectionChanged(int index)
        {
            DestroyChildren(optionsTransform);
            
            if (index > 0)
            {
                simulationButton.interactable = true;
                _selectedController = _controllers[index - 1];
                _parameters.Clear();
                AddValueToParams(new KeyValuePair<string, string>(IDParam, _selectedController.ExperimentId));

                if (_modelValuesMap.TryGetValue(_selectedModel.GetInstanceID(), out var selectedValues))
                {
                    PopulateDataFields(_selectedController.ExperimentData.InputParameters,
                        selectedValues.UserSelectedValues);
                }
                else
                {
                    PopulateDataFields(_selectedController.ExperimentData.InputParameters);
                }

                var file = _selectedController.ExperimentData.Files
                    .FirstOrDefault(f => f.FileInfo.FileType == FileType.PDF);
                if (file != null)
                {
                    _documentationURL = ApiClient.Instance.APISettings.BaseURL + file.PublicURL;
                    documentationButtonGO.SetActive(true);
                    var documentationLabel = documentationButtonGO.GetComponentInChildren<TextMeshProUGUI>();
                    var localizationScript = documentationLabel.gameObject.GetComponent<UILocalization>();
                    documentationLabel.text = LocalizationManager.GetStringTableEntryOrDefault(
                        localizationScript.Key, localizationScript.DefaultValue);
                }
            }
            else
            {
                simulationButton.interactable = false;
                documentationButtonGO.SetActive(false);
                _documentationURL = "";
                _selectedController = null;
            }
        }

        public void OnSimulationSubmit()
        {
            var modelSelectedValues = new ModelSelectedValues(_selectedController);

            foreach (var dataField in optionsTransform.gameObject.GetComponentsInChildren<IDataField>())
            {
                modelSelectedValues.AddSelectedValue(dataField.GetInputAndValue());
                AddValueToParams(dataField.GetValue());
            }

            _modelValuesMap[_selectedModel.GetInstanceID()] = modelSelectedValues;

            SimulationManager.Instance.StartSimulation(_selectedModel, _parameters,
                _selectedController?.ExperimentData);
            if (_parameters.Count > 0)
            {
                _parameters = new Dictionary<string, string> { { IDParam, _parameters[IDParam] } };
            }
        }

        private async void OnModelSelected(GameObject model, ModelSettings modelSettings)
        {
            if (model == null && modelSettings == null)
            {
                ResetSimulationMenu();
            }
            else
            {
                UpdateInfoMessage(false);
                DestroyChildren(optionsTransform);

                _selectedModel = model;
                _selectedModelSetting = modelSettings;
                UpdateSelectedModelInput();

                _parameters.Clear();
                simulationButton.interactable = false;
                _controllerDropdown.ClearOptions();

                documentationButtonGO.SetActive(false);
                controllerDropdownGO.SetActive(true);

                var controllerLabel = controllerDropdownGO.GetComponentInChildren<TextMeshProUGUI>();
                var localizationScript = controllerLabel.gameObject.GetComponent<UILocalization>();

                controllerLabel.text = LocalizationManager.GetStringTableEntryOrDefault(
                    localizationScript.Key, localizationScript.DefaultValue);

                _controllerDropdown.options.Add(
                    new TMP_Dropdown.OptionData(
                        LocalizationManager.GetStringTableEntryOrDefault(
                            LocalizationKeyValuePairs.LoadingPlaceholderKey,
                            LocalizationKeyValuePairs.LoadingPlaceholderDefaultValue)));
                _controllerDropdown.interactable = false;
                _controllers = await SimulationManager.Instance.PickModel(_selectedModelSetting);

                var options = _controllers.Select(c => c.Name).ToList();

                if (_modelValuesMap.TryGetValue(model.GetInstanceID(), out var selectedValues))
                {
                    var optionIndex = options.IndexOf(selectedValues.Controller.Name);
                    PopulateDropdown(_controllerDropdown, options, optionIndex);
                    OnControllerSelectionChanged(optionIndex + 1);
                }
                else
                {
                    PopulateDropdown(_controllerDropdown, options);
                }
            }
        }

        private void OnModelDeselected(GameObject obj)
        {
            ResetSimulationMenu();
        }

        private void OnLocalizationChange(StringTable stringTable)
        {
            if (_selectedModel != null && _selectedModelSetting != null)
            {
                OnModelSelected(_selectedModel, _selectedModelSetting);
            }
            else
            {
                modelInput.text = LocalizationManager.GetStringTableEntryOrDefault(
                    LocalizationKeyValuePairs.SelectModelKey,
                    LocalizationKeyValuePairs.SelectModelDefaultValue);
            }
        }

        /// <summary>
        /// Vyresetovanie simulačného menu do stavu kedy sa na obrazovke zobrazuje
        /// len select na výber modelu
        /// </summary>
        public void ResetSimulationMenu()
        {
            DestroyChildren(optionsTransform);
            _selectedModel = null;
            _selectedModelSetting = null;
            modelInput.text = LocalizationManager.GetStringTableEntryOrDefault(
                LocalizationKeyValuePairs.SelectModelKey,
                LocalizationKeyValuePairs.SelectModelDefaultValue);
            simulationButton.interactable = false;
            UpdateInfoMessage(true);
            controllerDropdownGO.SetActive(false);
            documentationButtonGO.SetActive(false);
            _controllerDropdown.SetValueWithoutNotify(0);

            if (_controllerDropdown.options.Count > 1)
            {
                _controllerDropdown.options.RemoveRange(1, _controllerDropdown.options.Count - 1);
            }

            if (_parameters.Count > 0)
            {
                _parameters = new Dictionary<string, string> { { IDParam, _parameters[IDParam] } };
            }
        }
        
        private void OnModelRemoved(GameObject go)
        {
            _modelValuesMap.Remove(go.GetInstanceID());
        }

        // helper methods

        private void UpdateSelectedModelInput()
        {
            modelInput.text = _selectedModelSetting.Label;
        }

        private void UpdateInfoMessage(bool show)
        {
            modelInputGo.SetActive(!show);
            infoMessage.SetActive(show);
        }

        private void AddValueToParams(KeyValuePair<string, string> valuePair)
        {
            if (!string.IsNullOrEmpty(valuePair.Value))
            {
                _parameters.Add(valuePair.Key, valuePair.Value);
            }
        }

        /// <summary>
        /// Pomocná metóda, ktorá odstráni všetky vnorené objekty
        /// </summary>
        /// <param name="transformParam"> objekt, ktorého vnorené objekty budú odstránené</param>
        private void DestroyChildren(Transform transformParam)
        {
            for (var i = transformParam.childCount - 1; i >= 0; --i)
            {
                Destroy(transformParam.GetChild(i).gameObject);
            }
        }
    }
}