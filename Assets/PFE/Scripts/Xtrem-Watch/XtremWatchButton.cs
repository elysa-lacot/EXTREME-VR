using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XtremWatchButton : MonoBehaviour
{
    public float endstop;//how far my plane will go
    public bool isPressed;

    private Transform location;
    private Vector3 startPos;

    // Start is called before the first frame update
    void Start()
    {
        location = GetComponent<Transform>();
        startPos = location.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (location.position.y - startPos.y < endstop)
        {//check to see if the button has been pressed all the way down

            location.position = new Vector3(location.position.x, endstop + startPos.y, location.position.z);
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            isPressed = true;//update pressed
        }

        if (location.position.y > startPos.y )
        {
            location.position = new Vector3(location.position.x, startPos.y, location.position.z);
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }

    }

    void OnCollisionExit(Collision collision)//check for when to unlock the button
    {
        if (collision.gameObject.tag == "Hand_R")
        {
            GetComponent<Rigidbody>().constraints &= ~RigidbodyConstraints.FreezePositionY; //Remove Y movement constraint.
            isPressed = false;//update pressed
        }
    }
}
