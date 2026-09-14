using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimTargetUpdater : MonoBehaviour
{
    private Transform aimTarget;
    private Camera aimCamera;

    public LayerMask aimLayerMask;

    [Range(0f, 10f)]
    [Header("最小距离")]
    public float minAimDistance = 2.5f;
    
    [Range(50f, 100f)]
    [Header("最大距离")]
    [Tooltip("最大距离,若超出扫描范围，则准星位置为相机前方最大距离")]
    public float maxDistance = 100f;

    private void Awake()
    {
        aimTarget = GetComponent<Transform>();
        aimCamera = Camera.main;
    }


    void LateUpdate()
    {
        Ray ray = new Ray(aimCamera.transform.position, aimCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, aimLayerMask))
        {
            float distance = Vector3.Distance(aimCamera.transform.position, hitInfo.point);
            distance = Mathf.Clamp(distance, minAimDistance, maxDistance);//限制准星距离在最小和最大范围内
            aimTarget.position = aimCamera.transform.position + aimCamera.transform.forward * distance;
        }
        else
        {
            aimTarget.position = aimCamera.transform.position + aimCamera.transform.forward * maxDistance;
        }
    }
}
