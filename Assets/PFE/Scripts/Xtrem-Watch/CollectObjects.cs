using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using ExtremeVR;

namespace ExtremeVR{

/**
*  \class CollectObjects
*  \author Lionel Cordas
*  \author Sofiane Kerrakchou
*  \brief Gestion de la liste d'objets de la montre
*/
class CollectObjects : MonoBehaviour
{
    public AudioClip audioCollecting;
    public AudioSource audioSource;
    public TextMeshProUGUI textMeshPro;
    public GameObject notifications;
    private int count = 0;
    public static bool isNotifOn;
    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;
    public TextMeshProUGUI text3;
    public TextMeshProUGUI text4;
    public TextMeshProUGUI text5;

    public List<string> objectsCollected;
    public Dictionary<string,GameObject> gameObjCollected;
    public int index;
    private int currentOffset;
    public SimulContext simulContext;
    public UnityPrint printable;
    public ObjectsOriginalTransform oot = null;
    
    //public static int selectorIndex=0;

    private TextMeshProUGUI[] texts;
    // Start is called before the first frame update
    void Start()
    {
        notifications.SetActive(false);
        isNotifOn = false;
        //AttachSimuContext();
        objectsCollected = new List<string>();
        gameObjCollected = new Dictionary<string,GameObject>();
        texts=new TextMeshProUGUI[5];
        texts[0]=text1;
        text1.color = new Color32(0,255,0,255);
        texts[1]=text2;
        texts[2]=text3;
        texts[3]=text4;
        texts[4]=text5;
        index =0;
        currentOffset = 0;
        printable = PlayerSingleton.GetInstance().GetComponentInChildren<UnityPrint>();
    }

    public void SetSimulContext(SimulContext sc){
        this.simulContext = sc;
    }
    /*
    public void AttachSimuContext(){
        SimulContext sim = (SimulContext)FindObjectOfType(typeof(SimulContext));
        if (sim){
            Debug.Log("Simulate Context object found: " + sim.name);
            simulContext = sim;
        }
        else
            Debug.Log("No SImulate Context object could be found");
    }*/


    // Update is called once per frame
    void Update()
    {
        CheckCount();
        //UpdateIndex(selectorIndex);
    }

    private void OnTriggerEnter(Collider other)
    {
        try{
            GameObject obj = GameObject.Find(other.name);
            if(obj != null)
            {
                if(obj.GetComponent<OVRGrabbable>() != null)
                {
                    if (obj.GetComponent<OVRGrabbable>().isGrabbed)
                    {
                        string object_label = LayerMask.LayerToName(obj.layer);
                        if (object_label.Contains("Grabbable"))
                        {
                            notifications.SetActive(true);
                            count++;
                            // starts vibration on the right Touch controller
                            OVRInput.SetControllerVibration(1, 1, OVRInput.Controller.LTouch);
                            audioSource.PlayOneShot(audioCollecting);
                            //Destroy(obj);
                            gameObjCollected.Add(obj.name,obj);
                            
                            textMeshPro.SetText(count.ToString());
                            this.simulContext.TakeObj(obj.name, obj.tag);
                            objectsCollected.Add(obj.name);
                            AddInfoToUI(currentOffset);
                            //obj.SetActive(false);
                            obj.GetComponent<Renderer>().enabled = false;
                            Renderer []renderers = obj.GetComponentsInChildren<Renderer>();
                            foreach (Renderer r in renderers) r.enabled = false;
                        }
                    }
                }
            }
        } catch (Exception e) { printable.PrintToUser(e.StackTrace,PrintType.WITH_CONFIRMATION);}
    }

    void AddInfoToUI(int offset){
        if(objectsCollected.Count > offset){
            text1.text = objectsCollected[offset];
            if(objectsCollected.Count > offset+1){
                text2.text = objectsCollected[offset+1];
                if(objectsCollected.Count > offset+2){
                    text3.text = objectsCollected[offset+2];
                    if(objectsCollected.Count > offset+3){
                        text4.text = objectsCollected[offset+3];
                        if(objectsCollected.Count > offset+4){
                            text5.text = objectsCollected[offset+4];
                        } else text5.text = "";
                    } else text4.text = "";
                } else text3.text = "";
            } else text2.text = "";
        } else text1.text = "";
        //UpdateIndex(0);
    }

    void CheckCount()
    {
        if(isNotifOn == true)
        {
            count = 0; //reinit the count of Notifications
            notifications.SetActive(false);
        }
    }

    public void UpdateIndex(int value){
        try{
            if(value >= objectsCollected.Count || value < 0) return;
            /*if(value < 5){
                texts[value].color = new Color32(0,255,0,255);
                if(value != index) texts[index].color = new Color32(255,255,255,255);
            }
            else{
                if(value - currentOffset > 5 || value - currentOffset < 0)
                AddInfoToUI(value-4);
            }*/
            //if(value - currentOffset < 5 && value - currentOffset > 0)
            texts[index - currentOffset].color = new Color32(255,255,255,255);
            if(value - currentOffset >= 5)
            {
                currentOffset++;
                AddInfoToUI(currentOffset);
            }
            else if(value - currentOffset < 0)
            {
                currentOffset--;
                AddInfoToUI(currentOffset);
            }

            texts[value - currentOffset].color = new Color32(0,255,0,255);
            
            index = value;
        } catch (Exception e) { printable.PrintToUser(e.Message,PrintType.WITH_CONFIRMATION);}
    }

    /** Bug à corriger */
    public void DropSelectedObject()
    {
        try{
            gameObjCollected[objectsCollected[index]].GetComponent<Renderer>().enabled = true;
            Renderer []renderers = gameObjCollected[objectsCollected[index]].GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers) r.enabled = true;
            simulContext.DropObj(objectsCollected[index],gameObjCollected[objectsCollected[index]].tag);
            ObjectTransform t = oot.getTransform(gameObjCollected[objectsCollected[index]]);
            gameObjCollected[objectsCollected[index]].transform.position = t.position;
            gameObjCollected[objectsCollected[index]].transform.rotation = t.rotation;
            gameObjCollected.Remove(objectsCollected[index]);
            objectsCollected.RemoveAt(index);
            //if(index >= objectsCollected.Count && objectsCollected.Count > 0) UpdateIndex(index - 1);
            //else AddInfoToUI(currentOffset);
            if(index >= objectsCollected.Count && objectsCollected.Count > 0)
            {
                texts[index - currentOffset].color = new Color32(255,255,255,255);
                index--;
                if(index - currentOffset >= 5)
                {
                    currentOffset=index - currentOffset - 5;
                }
                texts[index - currentOffset].color = new Color32(0,255,0,255);
            }
            AddInfoToUI(currentOffset);
        }
        catch (Exception e) { printable.PrintToUser("DR:"+e.Message,PrintType.WITH_CONFIRMATION);}
        //UpdateIndex(0);
    }

    void DeleteFromList(string item){
        if(objectsCollected.Contains(item)){
            objectsCollected.Remove(item);
        } 
    } 
}
}
