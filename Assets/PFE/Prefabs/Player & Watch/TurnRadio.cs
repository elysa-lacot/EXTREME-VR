using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnRadio : MonoBehaviour
{
    public AudioClip music;
    public AudioSource audioSource;

    private bool isON;
    // Start is called before the first frame update
    void Start()
    {
        audioSource.Play(0);
        isON = true;
    }
    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        string object_tag = other.tag;
        if (object_tag.Contains("TriggerRadio"))
        {
            if (isON == true)
            {
                isON = false;
                audioSource.Pause();
            }
            else
            {
                isON = true;
                audioSource.UnPause();
            }

        }
    }
}
