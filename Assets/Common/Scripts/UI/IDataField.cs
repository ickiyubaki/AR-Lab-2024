using System.Collections.Generic;
using Common.Scripts.Simulation;

namespace Common.Scripts.UI
{
    public interface IDataField
    {
        void SetData(InputParameter param, string value = "");

        KeyValuePair<string, string> GetValue();

        KeyValuePair<InputParameter, string> GetInputAndValue();
    }
}
