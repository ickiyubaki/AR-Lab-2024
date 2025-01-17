using UnityEngine.Localization.Tables;

namespace Localization.Scripts
{
    public interface ILocalization
    {
        void OnLocalizationChange(StringTable table);
    }
}
