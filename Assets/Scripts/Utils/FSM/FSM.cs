using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MyFSM
{
    public class FSM<TStateType> where TStateType : Enum
    {
        public IState curState; //当前状态
        public TStateType curStateType; //当前状态枚举
        public TStateType prevStateType; //上一个状态枚举

        public Dictionary<TStateType, IState> stateDict;    //存储状态机所有状态
        public Dictionary<TStateType,List<(TStateType,Func<bool> condition)>> stateTransitionDict; //存储状态机所有状态的转换关系,from为key

        public FSMContext context;//存储状态机上下文信息,比如人物数据
        public FSM(FSMContext context)
        {
            this.context = context;
            stateDict = new Dictionary<TStateType, IState>();
            stateTransitionDict = new Dictionary<TStateType, List<(TStateType, Func<bool> condition)>>();
        }

        public void AddState(TStateType type, IState state,FSMContext context)
        {
            if (stateDict.ContainsKey(type))
            {
                Debug.LogError("状态机已经存在该状态: " + type);
                return;
            }
            state.Initialization(context);
            stateDict.Add(type, state);
        }
        public void AddTransition(TStateType from, TStateType to, Func<bool> condition)
        {
            if (!stateTransitionDict.ContainsKey(from))
            {
                stateTransitionDict.Add(from, new List<(TStateType, Func<bool> condition)>());
            }
            stateTransitionDict[from].Add((to, condition));
        }

        public void SwitchState(TStateType type)
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
            prevStateType = curStateType;
            curStateType = type;
            curState.OnEnter();
        }

        public void OnUpdate()
        {
            curState?.OnUpdate();
            CheckTransition();
        }
        public void OnFixedUpdate()
        {
            curState?.OnFixedUpdate();
        }
        public void OnLateUpdate()
        {
            curState?.OnLateUpdate();
        }
        
        private void CheckTransition()
        {
            if (stateTransitionDict.TryGetValue(curStateType, out var transitions))
            {
                foreach (var (to, condition) in transitions)
                {
                    if (condition())
                    {
                        SwitchState(to);
                        return;
                    }
                }
            }
        }
    }
}