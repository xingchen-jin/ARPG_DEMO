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
    public Camera MainCamera => mainCamera;
    
    public Transform AimTargetTransform => aimTargetTransform;
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
}
