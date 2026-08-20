using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MyFSM{
    public interface IState
    {

        void OnEnter();
        void OnExit();
        void OnUpdate();
        // void OnInput(PlayerInputData inputData);
        void OnFixedUpdate();
        void OnLateUpdate();
        // void OnCheck();
    }
}
