﻿using Assets.Scripts.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Synthesis.FSM
{
    public class StateMachine : MonoBehaviour
    {
        private static StateMachine sceneGlobal;

        private Stack<State> activeStates;
        private readonly ObjectStateLinker<Behaviour> stateBehaviours;
        private readonly ObjectStateLinker<GameObject> stateGameObjects;

        /// <summary>
        /// (Defined from the editor)
        /// If true, this instance can be accessed globally from the <see cref="SceneGlobal"/> property.
        /// </summary>
        public bool sceneGlobalInstance;
        
        /// <summary>
        /// The global StateMachine instance.
        /// </summary>
        public static StateMachine SceneGlobal
        {
            get
            {
                if (sceneGlobal == null)
                    Debug.LogWarning("No global StateMachine instance has been defined!");

                return sceneGlobal;
            }
            private set
            {
                sceneGlobal = value;
            }
        }

        /// <summary>
        /// The current state in the StateMachine.
        /// </summary>
        public State CurrentState { get; private set; }

        /// <summary>
        /// Initializes the StateMachine.
        /// </summary>
        private StateMachine()
        {
            activeStates = new Stack<State>();
            stateBehaviours = new ObjectStateLinker<Behaviour>((b, e) => b.enabled = e, (b) => b.enabled);
            stateGameObjects = new ObjectStateLinker<GameObject>((g, e) => g.SetActive(e), (g) => g.activeSelf);
        }

        /// <summary>
        /// Finds the states of the given type in the stack of active states.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T FindState<T>() where T : State
        {
            foreach (State state in activeStates)
                if (state is T)
                    return state as T;
            
            return null;
        }

        /// <summary>
        /// Links the given MonoBehaviour script to the provided state type.
        /// </summary>
        /// <typeparam name="T">The type of state with which to link the <see cref="Behaviour"/>.</typeparam>
        /// <param name="behaviour">The MonoBehaviour to link.</param>
        /// <param name="enabledByDefault">If true, this object will be enabled when its scene becomes active.</param>
        /// <param name="useStrictLinking">When true, this Behaviour will be disabled when a new <see cref="State"/>
        /// is pushed before its parent <see cref="State"/> is popped.</param>
        public void Link<T>(Behaviour behaviour, bool enabledByDefault = true, bool useStrictLinking = true) where T : State
        {
            stateBehaviours.Link<T>(behaviour, CurrentState is T, enabledByDefault, useStrictLinking);
        }

        /// <summary>
        /// Links the given GameObject to the provided state type.
        /// </summary>
        /// <typeparam name="T">The type of state with which to link the <see cref="GameObject"/>.</typeparam>
        /// <param name="gameObject">The GameObject to link.</param>
        /// <param name="enabledByDefault">If true, this object will be enabled when its scene becomes active.</param>
        /// <param name="useStrictLinking">When true, this GameObject will be disabled when a new <see cref="State"/>
        /// is pushed before its parent <see cref="State"/> is popped.</param>
        public void Link<T>(GameObject gameObject, bool enabledByDefault = true, bool useStrictLinking = true) where T : State
        {
            stateGameObjects.Link<T>(gameObject, CurrentState is T, enabledByDefault, useStrictLinking);
        }

        /// <summary>
        /// Adds a new state to the StateMachine and pauses the current one if it exists.
        /// </summary>
        /// <param name="state"></param>
        public void PushState(State state)
        {
            if (CurrentState != null)
                CurrentState.Pause();

            DisableAllObjects(false);

            if (!activeStates.Contains(state))
                activeStates.Push(state);

            CurrentState = state;
            CurrentState.StateMachine = this;
            CurrentState.Awake();

            EnableAllObjects();

            CurrentState.Start();
            CurrentState.Resume();
        }

        /// <summary>
        /// Removes the current state from the StateMachine.
        /// </summary>
        public bool PopState()
        {
            if (CurrentState == null)
                return false;

            CurrentState.Pause();
            CurrentState.End();
            CurrentState.StateMachine = null;

            DisableAllObjects(true);

            activeStates.Pop();

            if (activeStates.Count > 0)
            {
                CurrentState = activeStates.First();

                EnableAllObjects();

                CurrentState.Resume();
            }
            else
            {
                CurrentState = null;
            }

            return true;
        }

        /// <summary>
        /// Pops all open States and pushes the given State.
        /// </summary>
        /// <param name="state"></param>
        public void ChangeState(State state)
        {
            while (PopState()) ;
            PushState(state);
        }

        /// <summary>
        /// Enables all objects linked to the current state.
        /// </summary>
        private void EnableAllObjects()
        {
            Type stateType = CurrentState.GetType();

            stateBehaviours.EnableObjects(stateType);
            stateGameObjects.EnableObjects(stateType);
        }

        /// <summary>
        /// Disables all objects linked to the current state.
        /// </summary>
        /// <param name="force"></param>
        private void DisableAllObjects(bool force)
        {
            if (CurrentState == null)
                return;

            Type stateType = CurrentState.GetType();

            stateBehaviours.DisableObjects(stateType, force);
            stateGameObjects.DisableObjects(stateType, force);
        }

        /// <summary>
        /// Sets the Instance property to this active instance.
        /// </summary>
        private void Awake()
        {
            if (sceneGlobalInstance)
                SceneGlobal = this;
        }

        /// <summary>
        /// Resets the scene global instance when the <see cref="StateMachine"/> is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (sceneGlobalInstance)
                SceneGlobal = null;
        }

        /// <summary>
        /// Updates the current state.
        /// </summary>
        private void Update()
        {
            CurrentState?.Update();
        }

        /// <summary>
        /// LateUpdates the current state.
        /// </summary>
        private void LateUpdate()
        {
            CurrentState?.LateUpdate();
        }

        /// <summary>
        /// FixedUpdates the current state.
        /// </summary>
        private void FixedUpdate()
        {
            CurrentState?.FixedUpdate();
        }

        /// <summary>
        /// Updates the GUI of the current state.
        /// </summary>
        private void OnGUI()
        {
            CurrentState?.OnGUI();
        }
    }
}