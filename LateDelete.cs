using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LateDelete : MonoBehaviour
{
    public float TimeToDeath = 1f;
    void Start()
    {
        StartCoroutine(death(TimeToDeath));
    }
    IEnumerator death(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
   
}
