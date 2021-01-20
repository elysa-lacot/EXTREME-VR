using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtremeVR;

namespace ExtremeVR{

class UI_Button_Handler : MonoBehaviour
{
    public GameObject buttonUp;
    public GameObject buttonDown;
    public GameObject buttonDelete;
    public GameObject selector;
    public AudioClip audio_click;
    public CollectObjects collector; 
    private RectTransform myRectTransform;
    private AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        myRectTransform = selector.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        string object_tag = other.tag;
        if (object_tag.Contains("UI_Button"))
        {
            if (other.name.Contains("CubeButtonUp"))
            {
                buttonUp.transform.localScale *= 1.2f;
                collector.UpdateIndex(collector.index -1);
            }
            if (other.name.Contains("CubeButtonDown"))
            {
                buttonDown.transform.localScale *= 1.2f;
                collector.UpdateIndex(collector.index +1);

            }
            if (other.name.Contains("CubeButtonDelete"))
            {
                buttonDelete.transform.localScale *= 1.2f;
                //CollectObjects.DeleteFromList()
                collector.DropSelectedObject();

            }
            audioSource.PlayOneShot(audio_click);

        }
    }
    private void OnTriggerExit(Collider other)
    {
        string object_tag = other.tag;
        if (object_tag.Contains("UI_Button"))
        {
            if (other.name.Contains("CubeButtonUp"))
            {
                buttonUp.transform.localScale /= 1.2f;
                myRectTransform.localPosition += new Vector3(0, 30f, 0);
            }
            if (other.name.Contains("CubeButtonDown"))
            {
                buttonDown.transform.localScale /= 1.2f;
                myRectTransform.localPosition -= new Vector3(0, 30f, 0);
            }
            if (other.name.Contains("CubeButtonDelete"))
            {
                buttonDelete.transform.localScale /= 1.2f;
                //selector.transform.localScale *= 1.1f;


            }

        }
    }
}
}
