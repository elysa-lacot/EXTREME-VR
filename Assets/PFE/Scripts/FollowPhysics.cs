using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Written by Elysa LACOT - November 2020
// Following Valem's Tutorial on YouTube: 'How to make a door in VR - Unity Tutorial'

public class FollowPhysics : MonoBehaviour
{
    public Transform target ;
    Rigidbody rb ;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>() ;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.MovePosition(target.transform.position) ;
    }
}
