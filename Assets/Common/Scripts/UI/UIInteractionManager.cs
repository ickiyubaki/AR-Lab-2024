using UnityEngine;

namespace Common.Scripts.UI
{
    public class UIInteractionManager : MonoBehaviour
    {
        [SerializeField] 
        private GameObject graphMenuButton;

        public void HideGraphButton()
        {
            graphMenuButton.SetActive(false);
        }
    }
}