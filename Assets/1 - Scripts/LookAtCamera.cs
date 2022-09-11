using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    public class LookAtCamera : MonoBehaviour
    {
        Camera cam;

        private void Awake()
        {
            cam = FindObjectOfType<Camera>();
        }

        private void Update()
        {
            transform.LookAt(cam.transform);    
            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
        }
    }
}
