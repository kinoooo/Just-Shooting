using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform followTarget;
    private void Update()
    {
        if (followTarget != null)
        {
            transform.position = new Vector3(followTarget.position.x, transform.position.y, followTarget.position.z - 10);
        }
    }
}
