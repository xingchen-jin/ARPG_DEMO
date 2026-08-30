using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineVirtualCamera normalCamera;
    [SerializeField] private CinemachineVirtualCamera aimCamera;
    [SerializeField] private Transform aimTargetTransform;
    private CinemachineInputProvider inputProvider;
    public Camera MainCamera => mainCamera;
    
    public Transform AimTargetTransform => aimTargetTransform;
    protected override void Awake()
    {
        base.Awake();
        inputProvider = normalCamera.GetComponent<CinemachineInputProvider>();
    }
    void Start()
    {
        SwitchToNormalCamera();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    /// <summary>
    ///转换到普通摄像机
    /// </summary>
    public void SwitchToNormalCamera()
    {
        //TODO: 完善切换逻辑
        aimCamera.Priority = 0;
        normalCamera.Priority = 10;
    }

    public void SwitchToAimCamera()
    {
        aimCamera.Priority = 10;
        normalCamera.Priority = 0;
    }
    /// <summary>
    /// 显示鼠标并解锁，且相机输入取消
    /// </summary>
    public void ShowCursorAndUnlock()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        // 取消相机输入
        CloseCursorInput();

    }
    public void HideCursorAndLock()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        // 恢复相机输入
        OpenCursorInput();
    }
    private void OpenCursorInput()
    {
        if(inputProvider == null)
        {
            Debug.LogWarning("CinemachineInputProvider未找到，无法恢复相机输入。");
            return;
        }
        inputProvider.enabled = true;
    }
    private void CloseCursorInput()
    {
        if(inputProvider == null)
        {
            Debug.LogWarning("CinemachineInputProvider未找到，无法关闭相机输入。");
            return;
        }
        inputProvider.enabled = false;
    }
}
