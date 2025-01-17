using Common.Scripts.Extensions;
using Common.Scripts.Tweens;
using Common.Scripts.Utils;
using Localization.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Common.Scripts.UI
{
    public class AddMenu : MonoBehaviour
    {
        [SerializeField] 
        private GameObject addMenu;

        [SerializeField] 
        private GameObject addMenuContent;

        [SerializeField] 
        private GameObject modelButtonPrefab;

        [SerializeField] 
        private SwitchPlacementPrefab modelSelector;

        private void Start()
        {
            foreach (var model in Models.Instance.Available3DModels)
            {
                var buttonPrefab = Instantiate(modelButtonPrefab, addMenuContent.transform);
                var localKey = model.LocalizationLabelKey;
                buttonPrefab.GetComponentInChildren<UILocalization>().Key = localKey;
                buttonPrefab.GetComponentInChildren<UILocalization>().DefaultValue = model.LocalizationDefaultValue;
                buttonPrefab.name =  localKey.ToLower();
               
                var button = buttonPrefab.GetComponent<Button>();
                button.onClick.AddListener(() => addMenu.GetComponent<TweenSequencer>().Close());
                button.onClick.AddListener(() => modelSelector.SwapPlacementObject(model.ModelPrefab));

                var image = buttonPrefab.FindComponentInChildWithTag<Image>("icon");
                if (model.ModelIcon != null)
                {
                    image.sprite = model.ModelIcon;
                }
            }
        }
    }
}