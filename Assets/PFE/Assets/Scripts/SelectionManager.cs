using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;

public class SelectionManager : MonoBehaviour
{
    // Start is called before the first frame update
    public Button selectButton;
    public SimulContext simc;
    public Behaviour lastactive;
    void Start()
    {
        //Button btn = selectButton.GetComponent<Button>();
        //btn.onClick.AddListener(SelectObject);
    }

    // Update is called once per frame
    void Update()
    {
        if(!simc.IsSceneFunctionRunning)
        {
            HoverObject();
            if (Input.GetMouseButtonDown(0))
            {
                SelectObject();
            }
            if (Input.GetMouseButtonDown(1))
            {
                Validate();
            }
        }
    }

    void SelectObject()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay (ray.origin, ray.direction * 100, Color.yellow);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit,100000))
        {
            string name = hit.collider.gameObject.name;
            Debug.Log("SELECTION MANAGER : object selected (" + name + ")");
            simc.TakeObj(name);
        }
        else Debug.Log("SELECTION MANAGER : nothing selected");
    }

    void HoverObject()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100000))
        {
            string name = hit.collider.gameObject.name;
            Debug.Log("SELECTION MANAGER : object detected (" + name + ")");
            Behaviour halo = simc.GetHaloComponent(name);
            halo.enabled = true;
            lastactive = halo;
        }
        else if(lastactive != null)
        {
            Debug.Log("SELECTION MANAGER : nothing selected");
            lastactive.enabled = false;
            lastactive = null;
        }
    }

    void Validate()
    {
        Debug.Log("Validate");
        simc.Validate();
    }
}
