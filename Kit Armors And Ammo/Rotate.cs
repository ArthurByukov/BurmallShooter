using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private float rotateAxisX;
    [SerializeField] private float rotateAxisY;
    [SerializeField] private float rotateAxisZ;
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(rotateAxisX,rotateAxisY,rotateAxisZ));
    }
}
