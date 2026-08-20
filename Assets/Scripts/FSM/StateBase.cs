using System.Collections;
using System.Collections.Generic;

using UnityEngine;
namespace MyFSM
{

    public abstract class StateBase : IState
    {
        public virtual void OnEnter(){}
        public virtual void OnExit(){}
        public virtual void OnUpdate(){}
        public virtual void OnFixedUpdate(){}

        public virtual void OnLateUpdate(){}
  
    }

}
