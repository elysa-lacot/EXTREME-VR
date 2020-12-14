using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XtremWatchShow : MonoBehaviour
{
    public XtremWatchButton button;
    public GameObject ui;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(button.isPressed == true)
        {
            ui.GetComponent<MeshRenderer>().enabled = true; //show it
        }
        else
        {
            ui.GetComponent<MeshRenderer>().enabled = false; //hide it
        }
    }
}
