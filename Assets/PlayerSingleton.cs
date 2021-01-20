using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtremeVR;
namespace ExtremeVR{

public class PlayerSingleton : MonoBehaviour
{
    private static GameObject instance = null;
    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this.gameObject;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   public static GameObject GetInstance()
    {
        return instance;
    }
}
}
