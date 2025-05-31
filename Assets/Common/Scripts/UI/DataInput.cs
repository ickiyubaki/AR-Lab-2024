using System;
using System.Collections.Generic;
using Common.Scripts.Simulation;
using Localization.Scripts;
using TMPro;
using UnityEngine;

namespace Common.Scripts.UI
{
    public class DataInput : MonoBehaviour, IDataField
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private TMP_InputField value;
        [SerializeField] private TMP_Text placeholder;
        private InputParameter _parameter;

        public void SetData(InputParameter param, string inputValue = "")
        {
            _parameter = param;
            label.text = param.Name;
            if (string.IsNullOrEmpty(inputValue))
            {
                value.text = (param.DefaultValue != null && param.DefaultValue.Count > 0)
                    ? param.DefaultValue[0]?.Value // <-- use Value, not Name
                    : "";
            }
            else
            {
                value.text = inputValue;
            }
            placeholder.text = LocalizationManager.GetStringTableEntryOrDefault(
                LocalizationKeyValuePairs.InputPlaceholderKey, LocalizationKeyValuePairs.InputPlaceholderDefaultValue);
        }

        public KeyValuePair<string, string> GetValue()
        {
            return new KeyValuePair<string, string>(_parameter.SchemaVar, value.text);
        }

        public KeyValuePair<InputParameter, string> GetInputAndValue()
        {
            return new KeyValuePair<InputParameter, string>(_parameter, value.text);
        }
    }
}
