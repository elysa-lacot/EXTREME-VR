using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Written by Elysa LACOT - November 2020
// Following Valem's Tutorial on YouTube: 'How to make a door in VR - Unity Tutorial'

public class ValveGrabbable : OVRGrabbable
{
    public Transform handler ;
    public float forceReleaseDistance = 0.5f ;

    public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
    	base.GrabEnd(Vector3.zero, Vector3.zero) ;
    	transform.position = handler.transform.position ;
    	transform.rotation = handler.transform.rotation ;

    	// The door won't move when we'll release it
    	Rigidbody rbHandler = handler.GetComponent<Rigidbody>() ;
    	rbHandler.velocity = Vector3.zero ;
    	rbHandler.angularVelocity = Vector3.zero ;
    }

    private void Update()
    {
    	// Force the release of the GrabbableHandler
    	if(Vector3.Distance(handler.position, transform.position) > forceReleaseDistance)
    	{
    		grabbedBy.ForceRelease(this) ;
    	}
    }
}
