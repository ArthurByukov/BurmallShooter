using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject MusicManager;
    // Start is called before the first frame update
    void Start()
    {
       gameObject.GetComponent<MusicManager>().StartWaveMusic();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
