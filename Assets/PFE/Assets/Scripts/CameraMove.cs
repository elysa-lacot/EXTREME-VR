using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    private bool _isFrozen;
    public bool IsFrozen { get { return _isFrozen; } set { _isFrozen = value;}}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!_isFrozen)
        {
            Camera mycam = GetComponent<Camera>();
    
            float sensitivity = 0.05f;
            Vector3 vp = mycam.ScreenToViewportPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mycam.nearClipPlane));
            vp.x -= 0.5f;
            vp.y -= 0.5f;
            vp.x *= sensitivity;
            vp.y *= sensitivity;
            vp.x += 0.5f;
            vp.y += 0.5f;
            Vector3 sp = mycam.ViewportToScreenPoint(vp);
            Vector3 v = mycam.ScreenToWorldPoint(sp);
            
            transform.LookAt(v, Vector3.up);
        }
    }

    public Camera GetCamera()
    {
        return GetComponent<Camera>().GetComponent<Camera>();
    }

    public Vector3 GetCameraPosition()
    {
        Camera mycam = GetComponent<Camera>().GetComponent<Camera>();
        return mycam.ScreenToViewportPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mycam.nearClipPlane));
    }
}
