using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;
using System.IO;
using System.Linq;
using UnityEditor.Localization.Plugins.CSV;
using UnityEditor.Localization.Reporting;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Localization.Editor
{
    [CreateAssetMenu(fileName = "New Importer", menuName = "Localization/Importer")]
    public class LocalizationImporter : ScriptableObject
    {
        private const string FlagsKey = "FLAG";

        [Header("String Tables")] 
        public SharedTableData stringSharedTable;
        [Header("Asset Tables")] 
        public SharedTableData assetSharedTable;
        [Header("Data Source Files (Comma Delimited)")]
        public TextAsset[] commaSeparatedFiles;
        [Header("Locale flags")] 
        public List<Texture2D> flags;

        public void Import()
        {
            ValidateReferences();
            var availableLocaleIdentifiers =
                LocalizationSettings.AvailableLocales.Locales.Select(l => l.Identifier).ToList();
            ImportAssetTableCollection(availableLocaleIdentifiers);
            ImportStringTableCollection(availableLocaleIdentifiers);
        }

        private void ImportAssetTableCollection(IEnumerable<LocaleIdentifier> availableLocaleIdentifiers)
        {
            var stc = LocalizationEditorSettings.GetAssetTableCollection(assetSharedTable.TableCollectionName);
            // Checks if a table with the given LocaleIdentifier exists in the collection if not creates a new one
            foreach (var localeIdentifier in availableLocaleIdentifiers.Where(li => !stc.ContainsTable(li)))
            {
                stc.AddNewTable(localeIdentifier);
            }

            var entry = assetSharedTable.Contains(FlagsKey)
                ? assetSharedTable.GetEntry(FlagsKey)
                : assetSharedTable.AddKey(FlagsKey);

            var flagLocales = flags.ToDictionary(x => x.name, x => x);
            foreach (var table in stc.AssetTables)
            {
                stc.AddAssetToTable(table, entry.Id,
                    flagLocales[$"{table.LocaleIdentifier.CultureInfo.EnglishName}({table.LocaleIdentifier.Code})"]);
            }
        }

        private void ImportStringTableCollection(IEnumerable<LocaleIdentifier> availableLocaleIdentifiers)
        {
            var stc = LocalizationEditorSettings.GetStringTableCollection(stringSharedTable.TableCollectionName);
            // Checks if a table with the given LocaleIdentifier exists in the collection if not creates a new one
            foreach (var localeIdentifier in availableLocaleIdentifiers.Where(li => !stc.ContainsTable(li)))
            {
                stc.AddNewTable(localeIdentifier);
            }

            foreach (var commaSeparatedFile in commaSeparatedFiles)
            {
                using var stream = new StreamReader(AssetDatabase.GetAssetPath(commaSeparatedFile));
                Csv.ImportInto(stream,
                    LocalizationEditorSettings.GetStringTableCollection(stringSharedTable.TableCollectionName),
                    reporter: new ProgressBarReporter());
            }
        }

        private void ValidateReferences()
        {
            if (stringSharedTable == null)
            {
                Debug.LogError("A StringSharedTableData reference is required.");
                return;
            }

            if (assetSharedTable == null)
            {
                Debug.LogError("A AssetSharedTableData reference is required.");
                return;
            }

            if (commaSeparatedFiles == null)
            {
                Debug.LogError("A comma delimited file reference is required.");
            }
        }
    }

    [CustomEditor(typeof(LocalizationImporter))]
    public class TestScriptableEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = (LocalizationImporter)target;

            if (GUILayout.Button("Import Language Settings", GUILayout.Height(40)))
            {
                script.Import();
            }
        }
    }
}
