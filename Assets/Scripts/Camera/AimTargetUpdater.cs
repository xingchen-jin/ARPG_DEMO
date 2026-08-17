using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimTargetUpdater : MonoBehaviour
{
    private Transform aimTarget;
    private Camera aimCamera;

    public float maxDistance = 100f;
    public LayerMask aimLayerMask;
    private void Awake()
    {
        aimTarget = GetComponent<Transform>();
        aimCamera = Camera.main;
    }



    void Update()
    {
        Ray ray = new Ray(aimCamera.transform.position, aimCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, aimLayerMask))
        {
            aimTarget.position = hitInfo.point;
        }
        else
        {
            aimTarget.position = aimCamera.transform.position + aimCamera.transform.forward * maxDistance;
        }
    }
}
