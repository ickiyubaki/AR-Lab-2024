using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common.Scripts.Extensions;
using Common.Scripts.UI;
using Common.Scripts.Utils;
using Localization.Scripts;
using Newtonsoft.Json;
using UnityEngine;

namespace Common.Scripts.Simulation
{
    /// <summary>
    /// Pomocné rozhranie na vyhľadávanie simulačného scriptu priradeného k prefabu 3D modelu.
    /// </summary>
    public interface ISimulation
    {
        public Type GetSimulationDataType();
    }

    /// <summary>
    /// Trieda deklaruje štruktúru simulačných dát.
    /// Do objektu tejto triedy sú deserializované simulačné dáta získané z API
    /// </summary>
    public abstract class SimulationData
    {
        [JsonProperty(PropertyName = "time")] 
        public abstract string Time { get; set; }
    }

    public abstract class Simulation<T> : MonoBehaviour, ISimulation where T : SimulationData
    {
        /// <summary>
        /// Ako často má prísť pri sumlácio k aktualizácii grafu
        /// </summary>
        protected const float DrawTimeStepInSeconds = 1f;
        
        /// <summary>
        /// Všetky dáta a parametre experimentu ziskané z API
        /// </summary>
        protected ExperimentData ExperimentData { get; private set; }
        /// <summary>
        /// Používateľom nastavené vstupné parametre simulácie
        /// </summary>
        protected Dictionary<string, string> UserSelectedValues { get; private set; }

        /// <returns> Návratový dátový typ simulačných dát </returns>
        public virtual Type GetSimulationDataType()
        {
            return typeof(List<T>);
        }

        /// <summary>
        /// Funckia spúšťa simuláciu, vykreslovanie grafu a pripravuje export simulačných dát
        /// </summary>
        /// <param name="simulationData"> Simulačné dáta </param>
        /// <param name="experimentData"> Všetky dostupné parametre experimentu </param>
        /// <param name="userSelectedValues"> Používateľom nastavené vstupne parametre simulácie </param>
        public void StartSimulation(List<T> simulationData, ExperimentData experimentData,
            Dictionary<string, string> userSelectedValues)
        {
            StopAllCoroutines();
            ExperimentData = experimentData;
            UserSelectedValues = userSelectedValues;
            var components = FindComponents();

            if (simulationData != null && simulationData.Count > 2)
            {
                var (graphDate, dataStep) = GetGraphData(simulationData);
                
                StartCoroutine(Simulate(simulationData, components));
                StartCoroutine(DrawGraph(graphDate, dataStep));
                SetUpDataShare(userSelectedValues, simulationData);
            }
            else
            {
                Toast.Instance.ShowErrorMessage(
                    LocalizationManager.GetStringTableEntryOrDefault("NO_SIMULATION_DATA", "No simulation data"), 3f);
            }
        }

        /// <returns> Kolekcia všetkých tagov, ktoré majú byť použité na vyhľadanie komponentov na scéne </returns>
        protected abstract IEnumerable<string> GetComponentsTagNames();

        /// <summary>
        /// Funkcia obsahujúca implemetáciu simulácie
        /// </summary>
        /// <param name="simulationData"> Simulačné dáta </param>
        /// <param name="components"> Najdéné kopomenty podľa špecifikovaných tagov </param>
        /// <seealso cref="FindComponents"/>
        /// <seealso cref="GetComponentsTagNames"/>
        protected abstract IEnumerator Simulate(List<T> simulationData,
            IReadOnlyDictionary<string, GameObject[]> components);

        /// <summary>
        /// Funkcia obsahujúca implemetáciu vykreslovania grafu
        /// </summary>
        /// <param name="simulationData"> Simulačné dáta </param>
        /// /// <param name="dataInterval"> Interval údajov - krok získavania dát</param>
        protected abstract IEnumerator DrawGraph(List<T> simulationData, decimal dataInterval);
        
        /// <summary>
        /// Funkcia vyhľadá všetky komponenty podľa špecifikovaných tagov a výsledky uloží do dictionary
        /// </summary>
        /// <returns> Dictionary kde klúčom je tag a hodnotou je pole komponentov s daným tagom</returns>
        /// <seealso cref="GetComponentsTagNames"/>
        protected virtual Dictionary<string, GameObject[]> FindComponents()
        {
            return GetComponentsTagNames()
                .ToDictionary(componentTag => componentTag,
                    componentTag => gameObject.FindComponentsInChildrenWithTag<Transform>(componentTag)
                        .Select(t => t.gameObject).ToArray());
        }
        
        /// <summary>
        /// Predmet správy, ktorý sa použije pri zdielaní simulačných dát
        /// </summary>
        protected virtual string GetShareSubject() => "Simulation Data";
        /// <summary>
        /// Text správy, ktorý sa použije pri zdielaní simulačných dát
        /// </summary>
        protected virtual string GetShareText() => "";
        /// <summary>
        /// Callback funkcia, ktorá sa zavolá po dokončení zdielania simulačných dát
        /// </summary>
        protected virtual NativeShare.ShareResultCallback GetShareCallback() => (result, target) => { };

        /// <summary>
        ///  Nastavenie parametrov zdieľania
        /// </summary>
        /// <param name="simulationData"> Simulačné dáta </param>
        /// <param name="userSelectedValues"> Používateľom nastavené vstupne parametre simulácie </param>
        private void SetUpDataShare(Dictionary<string, string> userSelectedValues, IEnumerable<T> simulationData)
        {
            NativeDataShare.Instance.SetParameters(GetShareSubject, GetShareText,
                () => GetExportFilePaths(userSelectedValues, simulationData), GetShareCallback());
        }

        // helper methods

        /// <summary>
        /// Funkcia vyexportuje simulačne dáta do CSV suborov a vratí ich umiestnenie
        /// </summary>
        /// <param name="userSelectedValues">Vstupné parametre simulácie</param>
        /// <param name="simulationData">Simulačné dáta</param>
        /// <returns>Umiestnenia CSV súborov</returns>
        private List<string> GetExportFilePaths(Dictionary<string, string> userSelectedValues,
            IEnumerable<T> simulationData)
        {
            var csvExporter = new CsvExporter<T>();
            var exportFilePaths = new List<string>
            {
                csvExporter.Write(
                    userSelectedValues.Where(k => k.Key != SimulationMenu.IDParam)
                        .ToDictionary(p => p.Key, p => p.Value), "simulation_parameters.csv"),
                csvExporter.Write(simulationData, "simulation_data.csv")
            };
            return exportFilePaths;
        }
        
        /// <summary>
        /// Ak je to nutné funkcia zredukuje množstvo dát, ktoré budú použité na vykreslenie grafu.
        /// V prípade, že simulačné dáta maju krok menši ako 0.1s dôjde k ich zredukovaniu.
        /// Vzhľadom na to, že vykreslovanie grafu je pomerne náročná operácie na zdroje a mohlo by dôjsť k spomaleniu
        /// aplikacie alebo jej sekaniu, dáta sú preventívne zredukované
        /// </summary>
        /// <param name="simulationData">Simulačné dáta</param>
        /// <returns>Zredukované simulačné dáta na vykreslenie do grafu a interval</returns>
        private (List<T>, decimal) GetGraphData(List<T> simulationData)
        {
            var timeStep = simulationData
                .Select(d => decimal.Parse(d.Time, 
                    CultureInfo.InvariantCulture.NumberFormat))
                .First(t => t > 0m);
            var graphData = simulationData;

            if (timeStep < 0.1m)
            {
                var time = 0m;
                var nStep = 0;
                while (time < 0.1m)
                {
                    time += timeStep;
                    nStep++;
                }

                timeStep = time; 
                graphData = simulationData.Where((d, i) => i % nStep == 0).ToList();
            }

            return (graphData, timeStep);
        }
    }
}