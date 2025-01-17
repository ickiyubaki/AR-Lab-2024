using System;
using System.Collections.Generic;
using Common.Scripts.AR;
using Localization.Scripts;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace UX.Scripts
{
    public readonly struct UXHandle
    {
        public readonly UIManager.InstructionUI InstructionalUI;
        public readonly UIManager.InstructionGoals Goal;

        public UXHandle(UIManager.InstructionUI ui, UIManager.InstructionGoals goal)
        {
            InstructionalUI = ui;
            Goal = goal;
        }
    }

    public class UIManager : MonoBehaviour
    {
        [SerializeField] 
        private bool startWithInstructionalUI = true;

        public bool StartWithInstructionalUI
        {
            get => startWithInstructionalUI;
            set => startWithInstructionalUI = value;
        }

        public enum InstructionUI
        {
            CrossPlatformFindAPlane,
            TapToPlace,
            None
        };

        [SerializeField] 
        private InstructionUI instructionalUI;

        public InstructionUI InstructionalUI
        {
            get => instructionalUI;
            set => instructionalUI = value;
        }

        public enum InstructionGoals
        {
            FoundAPlane,
            FoundMultiplePlanes,
            PlacedAnObject,
            None
        };

        [SerializeField] 
        private InstructionGoals instructionalGoal;
    
        public InstructionGoals InstructionalGoal
        {
            get => instructionalGoal;
            set => instructionalGoal = value;
        }

        [SerializeField] 
        private bool showSecondaryInstructionalUI;
    
        public bool ShowSecondaryInstructionalUI
        {
            get => showSecondaryInstructionalUI;
            set => showSecondaryInstructionalUI = value;
        }

        [SerializeField] 
        private InstructionUI secondaryInstructionUI = InstructionUI.TapToPlace;

        public InstructionUI SecondaryInstructionUI
        {
            get => secondaryInstructionUI;
            set => secondaryInstructionUI = value;
        }

        [SerializeField] 
        private InstructionGoals secondaryGoal = InstructionGoals.PlacedAnObject;

        public InstructionGoals SecondaryGoal
        {
            get => secondaryGoal;
            set => secondaryGoal = value;
        }

        [SerializeField] 
        private GameObject arSessionOrigin;

        public GameObject ARSessionOrigin
        {
            get => arSessionOrigin;
            set => arSessionOrigin = value;
        }

        private Func<bool> _goalReached;
        //private bool _secondaryGoalReached;

        private Queue<UXHandle> _uxOrderedQueue;
        private UXHandle _currentHandle;
        private bool _processingInstructions;
        public bool PlacedObjectProp { get; set; }

        [SerializeField] 
        private ARPlaneManager planeManager;
    
        public ARPlaneManager PlaneManager
        {
            get => planeManager;
            set => planeManager = value;
        }

        [SerializeField] 
        private ARUXAnimationManager animationManager;

        public ARUXAnimationManager AnimationManager
        {
            get => animationManager;
            set => animationManager = value;
        }

        private bool _fadedOff;
    
        [SerializeField] 
        private LocalizationManager localizationManager;

        public LocalizationManager LocalizationManager
        {
            get => localizationManager;
            set => localizationManager = value;
        }

        private void OnEnable()
        {
            ARUXAnimationManager.OnFadeOffComplete += FadeComplete;
            ARPlacementInteractableMultiple.OnObjectPlaced += OnModelPlaced;

            GetManagers();
            _uxOrderedQueue = new Queue<UXHandle>();

            if (startWithInstructionalUI)
            {
                _uxOrderedQueue.Enqueue(new UXHandle(instructionalUI, instructionalGoal));
            }

            if (showSecondaryInstructionalUI)
            {
                _uxOrderedQueue.Enqueue(new UXHandle(secondaryInstructionUI, secondaryGoal));
            }
        }

        private void OnDisable()
        {
            ARUXAnimationManager.OnFadeOffComplete -= FadeComplete;
        }
        
        private void OnModelPlaced(ARPlacementInteractableMultiple interactable, GameObject go)
        {
            PlacedObjectProp = true;
        }

        private void Update()
        {
            if (animationManager.LocalizeText)
            {
                if (!localizationManager.LocalizationComplete)
                {
                    return;
                }
            }

            if (_uxOrderedQueue.Count > 0 && !_processingInstructions)
            {
                // pop off
                _currentHandle = _uxOrderedQueue.Dequeue();
            
                // exit instantly, if the goal is already met it will skip showing the first UI and move to the next in the queue 
                _goalReached = GetGoal(_currentHandle.Goal);
                if (_goalReached.Invoke())
                {
                    return;
                }

                // fade on
                FadeOnInstructionalUI(_currentHandle.InstructionalUI);
                _processingInstructions = true;
                _fadedOff = false;
            }

            if (_processingInstructions)
            {
                // start listening for goal reached
                if (_goalReached.Invoke())
                {
                    // if goal reached, fade off
                    if (!_fadedOff)
                    {
                        _fadedOff = true;
                        animationManager.FadeOffCurrentUI();
                    }
                }
                
                UpdateLocalText(_currentHandle.InstructionalUI);
            }
        }

        private void GetManagers()
        {
            if (arSessionOrigin && arSessionOrigin.TryGetComponent(out ARPlaneManager arPlaneManager))
            {
                planeManager = arPlaneManager;
            }
        }

        private Func<bool> GetGoal(InstructionGoals goal)
        {
            return goal switch
            {
                InstructionGoals.FoundAPlane => PlanesFound,
                InstructionGoals.FoundMultiplePlanes => MultiplePlanesFound,
                InstructionGoals.PlacedAnObject => PlacedObject,
                InstructionGoals.None => () => false,
                _ => () => false
            };
        }

        private void FadeOnInstructionalUI(InstructionUI ui)
        {
            switch (ui)
            {
                case InstructionUI.CrossPlatformFindAPlane:
                    animationManager.ShowCrossPlatformFindAPlane();
                    break;

                case InstructionUI.TapToPlace:
                    animationManager.ShowTapToPlace();
                    break;

                case InstructionUI.None:
                    break;
            }
        }
        
        private void UpdateLocalText(InstructionUI ui)
        {
            switch (ui)
            {
                case InstructionUI.CrossPlatformFindAPlane:
                    animationManager.UpdateShowCrossPlatformFindAPlaneText();
                    break;

                case InstructionUI.TapToPlace:
                    animationManager.UpdateTapToPlaceText();
                    break;

                case InstructionUI.None:
                    break;
            }
        }

        private bool PlanesFound() => planeManager && planeManager.trackables.count > 0;

        private bool MultiplePlanesFound() => planeManager && planeManager.trackables.count > 1;

        private void FadeComplete()
        {
            _processingInstructions = false;
        }

        private bool PlacedObject()
        {
            // reset flag to be used multiple times
            if (PlacedObjectProp)
            {
                PlacedObjectProp = false;
                return true;
            }
            return PlacedObjectProp;
        }

        public void AddToQueue(UXHandle uxHandle)
        {
            _uxOrderedQueue.Enqueue(uxHandle);
        }

        public void TestFlipPlacementBool()
        {
            PlacedObjectProp = true;
        }
    }
}