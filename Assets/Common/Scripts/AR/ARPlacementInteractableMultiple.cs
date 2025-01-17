using System;
using System.Collections.Generic;
using Common.Scripts.Extensions;
using Common.Scripts.UI;
using Localization.Scripts;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.AR;

namespace Common.Scripts.AR
{
    public sealed class ARPlacementInteractableMultiple : ARBaseGestureInteractable
    {
        public static readonly List<GameObject> Instantiated3DModelsInScene = new List<GameObject>();

        private static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();

        private bool _showHelpMessage = true;
        private bool _readyToPlace;
        public bool ReadyToRemove { get; set; }

        /// <summary>
        /// A <see cref="GameObject"/> to place when a raycast from a user touch hits a plane.
        /// </summary>
        [Tooltip("A GameObject to place when a raycast from a user touch hits a plane.")]
        private GameObject  _placementPrefab;

        public GameObject PlacementPrefab
        {
            get => _placementPrefab;
            set
            {
                _placementPrefab = value;
                _readyToPlace = true;
            }
        }

        /// <summary>
        /// Gets or sets the event that is called when this Interactable places a new <see cref="GameObject"/> in the world.
        /// </summary>
        public static event Action<ARPlacementInteractableMultiple, GameObject> OnObjectPlaced;

        /// <summary>
        /// Gets or sets the event that is called when <see cref="GameObject"/> is removed.
        /// </summary>
        public static event Action<GameObject> OnObjectRemoved;

         /// <summary>
        /// Gets the pose for the object to be placed from a raycast hit triggered by a <see cref="TapGesture"/>.
        /// </summary>
        /// <param name="gesture">The tap gesture that triggers the raycast.</param>
        /// <param name="pose">When this method returns, contains the pose of the placement object based on the raycast hit.</param>
        /// <returns>Returns <see langword="true"/> if there is a valid raycast hit that hit the front of a plane.
        /// Otherwise, returns <see langword="false"/>.</returns>
         private bool TryGetPlacementPose(TapGesture gesture, out Pose pose)
        {
            // Raycast against the location the player touched to search for planes.
            if (GestureTransformationUtility.Raycast(gesture.startPosition, Hits, arSessionOrigin, TrackableType.PlaneWithinPolygon))
            {
                pose = Hits[0].pose;

                // Use hit pose and camera pose to check if hit test is from the
                // back of the plane, if it is, no need to create the anchor.
                // ReSharper disable once LocalVariableHidesMember -- hide deprecated camera property
                var camera = arSessionOrigin != null ? arSessionOrigin.camera : Camera.main;
                if (camera == null)
                    return false;

                return Vector3.Dot(camera.transform.position - pose.position, pose.rotation * Vector3.up) >= 0f;
            }

            pose = default;
            return false;
        }

        /// <summary>
        /// Instantiates the placement object and positions it at the desired pose.
        /// </summary>
        /// <param name="pose">The pose at which the placement object will be instantiated.</param>
        /// <returns>Returns the instantiated placement object at the input pose.</returns>
        /// <seealso cref="_placementPrefab"/>
        private GameObject PlaceObject(Pose pose)
        {
            ReadyToRemove = false;
            var placementObject = Instantiate(_placementPrefab, pose.position, pose.rotation);

            // Create anchor to track reference point and set it as the parent of placementObject.
            var anchor = new GameObject("PlacementAnchor").transform;
            anchor.position = pose.position;
            anchor.rotation = pose.rotation;
            placementObject.transform.parent = anchor;

            // Use Trackables object in scene to use as parent
            if (arSessionOrigin != null && arSessionOrigin.trackablesParent != null)
                anchor.parent = arSessionOrigin.trackablesParent;
            
            Instantiated3DModelsInScene.Add(placementObject);
            return placementObject;
        }

        /// <inheritdoc />
        protected override bool CanStartManipulationForGesture(TapGesture gesture)
        {
            if (!ReadyToRemove || gesture.targetObject == null)
            {
                return !gesture.startPosition.IsPointerOverUIObject() && gesture.targetObject == null;
            }

            // If ReadyToRemove is True destroy selected GameObject
            
            // GameObject je potrebne deaktivovat pretoze v pripade ak mame model selectnuty a chceme ho odmazat
            // po samotnom odmazani sa este na koniec vyvola opatovny Select co sposoji nastavenie simulacneho
            // menu pre objekt, ktory uz, ale neexistuje. Nasledujuci riadok teda zabranuje tomuto bugu.
            gesture.targetObject.SetActive(false);
            
            Instantiated3DModelsInScene.Remove(gesture.targetObject);
            OnObjectRemoved?.Invoke(gesture.targetObject);
            Destroy(gesture.targetObject);
            ReadyToRemove = false;
            return false;
        }
        
        /// <inheritdoc />
        protected override void OnEndManipulation(TapGesture gesture)
        {
            base.OnEndManipulation(gesture);

            if (gesture.isCanceled)
                return;

            if (arSessionOrigin == null)
                return;

            if (_readyToPlace && TryGetPlacementPose(gesture, out var pose))
            {
                var placementObject = PlaceObject(pose);

                if (_showHelpMessage)
                {
                    Toast.Instance.ShowInfoMessage(LocalizationManager.GetStringTableEntryOrDefault(LocalizationKeyValuePairs.TapToManipulateKey,
                        LocalizationKeyValuePairs.TapToManipulateDefaultValue), 5f);
                    _showHelpMessage = false;
                }
                
                OnObjectPlaced?.Invoke(this, placementObject);
                _readyToPlace = false;
            }
        }

        public void RemoveAllObject()
        {
            foreach (var model in Instantiated3DModelsInScene)
            {
                OnObjectRemoved?.Invoke(model);
                Destroy(model);
            }

            _readyToPlace = false;
            ReadyToRemove = false;
            Instantiated3DModelsInScene.Clear();
        }
    }
}