using System;
using System.Collections.Generic;
using System.Linq;
using Common.Scripts.Simulation;
using TMPro;
using UnityEngine;

namespace Common.Scripts.UI
{
    public class DataSelect : MonoBehaviour, IDataField
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private TMP_Dropdown dropdown;
        private InputParameter _parameter;
        private List<InputValue> _options;

        public void SetData(InputParameter param, string selectedValue = "")
        {
            label.text = param.Name;
            _parameter = param;
            _options = param.DefaultValue ?? new List<InputValue>();
            
            dropdown.ClearOptions();
            if (_options.Count == 0)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(" - "));
                dropdown.interactable = false;
            }
            else
            {
                dropdown.AddOptions(_options.Select(o => o.Value).ToList());
            }

            if (string.IsNullOrEmpty(selectedValue)) return;
            var index = dropdown.options.FindIndex(o => o.text.Equals(selectedValue));
            if (index >= 0)
            {
                dropdown.SetValueWithoutNotify(index);
            }
        }

        public KeyValuePair<string, string> GetValue()
        {
            return new KeyValuePair<string, string>(_parameter.SchemaVar, _options[dropdown.value].Name);
        }
        
        public KeyValuePair<InputParameter, string> GetInputAndValue()
        {
            return new KeyValuePair<InputParameter, string>(_parameter, _options[dropdown.value].Name);
        }
    }
}
