using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MyFSM
{
    public enum StateType
    {
        None,Locomotion,Jump,Attack,Die
    }
    public interface IState
    {
        void OnEnter();
        void OnExit();
        void OnUpdate();
        void OnInput(PlayerInputData inputData);
        void OnFixedUpdate();
        // void OnCheck();
    }
    [Serializable]
    public class FSMContext{}

    public class FSM
    {
        public IState curState;
        public Dictionary<StateType, IState> stateDict;
        public FSMContext context;//存储状态机上下文信息,比如人物数据
        public FSM(FSMContext context)
        {
            this.context = context;
            stateDict = new Dictionary<StateType, IState>();
        }

        public void AddState(StateType type, IState state)
        {
            if (!stateDict.ContainsKey(type))
            {
                stateDict.Add(type, state);
            }else
            {
                Debug.LogError("状态已存在 " + type);
            }
        }

        public void SwitchState(StateType type)
        {
            if (!stateDict.ContainsKey(type))
            {
                Debug.LogError("找不到状态: " + type);
                return;
            }
            if (curState != null)
            {
                curState.OnExit();
            }
            curState = stateDict[type];
            curState.OnEnter();
        }
        public void OnUpdate()
        {
            curState?.OnUpdate();
        }
        public void OnFixedUpdate()
        {
            curState?.OnFixedUpdate();
        }
        public void OnInput(PlayerInputData inputData)
        {
            curState?.OnInput(inputData);
        }
    }
}