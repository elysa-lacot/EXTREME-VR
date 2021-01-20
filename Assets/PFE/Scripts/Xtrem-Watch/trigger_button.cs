using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtremeVR;

namespace ExtremeVR{

public class trigger_button : MonoBehaviour
{
    public GameObject ui;
    public GameObject watch_zone;
    public Material mat_not_watching;
    public Material mat_is_watching;
    public AudioClip audio_activate;
    public AudioClip audio_desactivate;

    private AudioSource audioSource;
    private bool isPressed;

    // Start is called before the first frame update
    void Start()
    {
        ui.SetActive(false);
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isPressed == true)
        {
            ui.SetActive(true);
        }
        else
        {
            ui.SetActive(false);
        }
        
    }
    private void OnTriggerEnter(Collider other)
    {
        string object_tag = other.tag;
        if (object_tag.Contains("Button_Show_UI"))
        {
            if(isPressed == false)
            {
                isPressed = true;   
                watch_zone.GetComponent<Renderer>().material = mat_is_watching;
                audioSource.PlayOneShot(audio_activate);
                CollectObjects.isNotifOn = true;
            }
            else
            {
                isPressed = false;
                watch_zone.GetComponent<Renderer>().material = mat_not_watching;
                audioSource.PlayOneShot(audio_desactivate);
                CollectObjects.isNotifOn = false;
            }

        }
    }
}
}
