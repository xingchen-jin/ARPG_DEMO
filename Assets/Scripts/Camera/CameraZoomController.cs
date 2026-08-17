using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class CinemachineController : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;
    private CinemachineFramingTransposer framingTransposer;
    [Header("CameraSettings")]
    [SerializeField] private float defaultDistance;
    [SerializeField] private float sensitivity;
    [SerializeField] private float smoothness;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    private float targetDistance;
    
    private float scrollValue;
    private PlayerInputMap playerInputMap;
    void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();

        framingTransposer.m_CameraDistance = defaultDistance;
        targetDistance = defaultDistance;

        playerInputMap = new PlayerInputMap();
        playerInputMap.Enable();
    }
    void Update()
    {
        scrollValue = GetScroller();
    }

    private float GetScroller()
    {
        return playerInputMap.Player.MouseScroll.ReadValue<Vector2>().y;
    }

    void LateUpdate()
    {
        targetDistance -= scrollValue * sensitivity;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        framingTransposer.m_CameraDistance = Mathf.Lerp(framingTransposer.m_CameraDistance, targetDistance, smoothness * Time.deltaTime);
    }
}
