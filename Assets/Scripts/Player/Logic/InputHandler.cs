using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public enum InputMode
{
    Gameplay,
    UI
}
public class InputHandler : MonoBehaviour
{
    [SerializeField]private InputMode currentInputMode;
    public PlayerFSMContext ctx;
    private PlayerInput playerInput;
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }
    void OnEnable()
    {
        playerInput.onActionTriggered += OnActionTriggered;
        SwitchInputMode(currentInputMode);
    }
    void OnDisable()
    {
        playerInput.onActionTriggered -= OnActionTriggered;
    }


    #region InputSystem事件回调
    private void OnActionTriggered(InputAction.CallbackContext context)
    {
        Debug.Log($"输入事件触发，当前输入模式:{currentInputMode},触发的动作:{context.action.name},触发的阶段:{context.phase}");
       switch (context.action.name)
        {
            case "Move":
                OnMove(context);
                break;
            case "Jump":
                OnJump(context);
                break;
            case "Firearm":
                OnFirearm(context);
                break;
            case "Aim":
                OnAim(context);
                break;
            case "Look":
                OnLook(context);
                break;
            case "Fire":
                OnFire(context);
                break;
            case "Reload":
                OnReload(context);
                break;
            case "Crouch":
                OnCrouch(context);
                break;
            case "RadialMenu":
                OnRadialMenu(context);
                break;
        }
    }
    public void OnMove(InputAction.CallbackContext context) 
    {
            ctx.input.moveInput = context.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ctx.input.jumpInput = true;
            ctx.input.crouchInput = false; //按下跳跃键时取消下蹲状态
    
        }
    }
    /// <summary>
    /// 切换武器事件回调,目前只支持单一武器切换（空手/拿枪），后续可扩展为多武器切换
    /// </summary>
    /// <param name="context"></param>
    public void OnFirearm(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //ctx.input.RifleInput = animator.GetBool("isFirearm") ? false : true;
            //EventCenter.EventTrigger<SwitchWeaponEvent>( new SwitchWeaponEvent(WeaponType.Rifle));//TODO:测试武器切换，后续需要扩展
        }
    }
    public void OnAim(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ctx.input.aimInput = ctx.animator.GetBool("isAiming") ? false : true;
        }
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        ctx.input.lookInput = context.ReadValue<Vector2>();
    }
    public void OnFire(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                ctx.input.fireInput = true;
                break;
            case InputActionPhase.Canceled:
                ctx.input.fireInput = false;
                break;
            case InputActionPhase.Performed:
                ctx.input.fireInput = true;
                break;
        }
    }
    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ctx.input.reloadInput = true;
        }
    }

    //TODO:待解决在跳跃等不应该触发下蹲的状态下，按下下蹲键会触发下蹲状态的问题
    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ctx.input.crouchInput = !ctx.input.crouchInput;
        }
    }
    public void OnRadialMenu(InputAction.CallbackContext context)
    {
        if (context.started && !ctx.input.radialMenuInput)
        {
            ctx.input.radialMenuInput = true;
            UIManager.Instance.ShowPanel<RadialMenuPanel>(UILevel.Top);
            SwitchInputMode(InputMode.UI);
        }else if (context.started && ctx.input.radialMenuInput)
        {
            ctx.input.radialMenuInput = false;
            UIManager.Instance.HidePanel<RadialMenuPanel>();
            SwitchInputMode(InputMode.Gameplay);
        }
    }
    #endregion
    #region methods
    /// <summary>
    /// 切换输入模式
    /// </summary>
    /// <param name="mode">输入模式</param>
    private void SwitchInputMode(InputMode mode)
    {
        Debug.Log($"切换输入模式为:{mode}");
        currentInputMode = mode;

    }
    #endregion
}