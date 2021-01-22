using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;

namespace ExtremeVR
{
    /**
    *  \class SelectionManager 
    *  \brief OBSOLETE, NE PAS UTILISER
    */
    class SelectionManager : MonoBehaviour
    {
        // Start is called before the first frame update
        public const int GRABBABLE_LAYER = 9;
        public Button selectButton;
        public SimulContext simc = null;
        private Behaviour lastactive;
        //private List<String> _ignored_objects;
        public GameObject Player;

        void Start()
        {
            simc = null;
            //_ignored_objects = new List<String>();
            //_ignored_objects.Add("Floor");
            //Button btn = selectButton.GetComponent<Button>();
            //btn.onClick.AddListener(SelectObject);
        }

        // Update is called once per frame
        void Update()
        {
            //Debug.Log("IsSFRunning " + simc.IsSceneFunctionRunning);
            if (simc!= null)
            {
                if (!simc.IsSceneFunctionRunning)
                {
                    //Debug.Log("SCAN//////////////");
                    HoverObject();
                    if (Input.GetMouseButtonDown(0))
                    {
                        SelectObject();
                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        Validate();
                    }
                    
                }
            }
        }

        void SelectObject()
        {
            //var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay (ray.origin, ray.direction * 100, Color.yellow);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit,100000))
            {
                string name = hit.collider.gameObject.name;
                //if(_ignored_objects.Contains(name)) return;
                if(hit.collider.gameObject.layer != GRABBABLE_LAYER) return;
                Debug.Log("SELECTION MANAGER : object selected (" + name + ")");
                simc.TakeObj(name, hit.collider.gameObject.tag);
            }
            else Debug.Log("SELECTION MANAGER : nothing selected");
        }

        void HoverObject()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //Debug.DrawRay(ray.origin, ray.direction * 5, Color.red);
            //Debug.Log("RAYCAST " + ray.origin + " " + ray.direction);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100000))
            {
                string name = hit.collider.gameObject.name;
                if(hit.collider.gameObject != lastactive && lastactive != null)
                {
                    lastactive.enabled = false;
                    lastactive = null;
                }
                else if(hit.collider.gameObject == lastactive) return;
                //if(_ignored_objects.Contains(name)) return;
                if(hit.collider.gameObject.layer != GRABBABLE_LAYER) return;
                Debug.Log("SELECTION MANAGER : object detected (" + name + ")");
                Behaviour halo = simc.GetHaloComponent(name);
                if(halo == null) return;
                //if(lastactive != null) lastactive.enabled = false;
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
}