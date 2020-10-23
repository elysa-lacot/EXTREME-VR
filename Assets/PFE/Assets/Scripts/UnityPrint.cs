using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using static IPrintable;


public class UnityPrint : MonoBehaviour, IPrintable
{
    public Text notifText;
    public Text confirmationText;
    public double remainingTime;
    public CameraMove MainCamera;
    private bool isWaitingText = false;
    public SimulContext simul;
    //For checkbox
    private bool _isWaitingForAnswser = false;
    public bool IsWaitingForAnswer() { return _isWaitingForAnswser; }

    public void Update()
    {
        if(isWaitingText)
        {
            if((remainingTime-=Time.deltaTime) < 0)
            {
                notifText.text = "";
                isWaitingText = false;
                simul.SignalUserInput();
            }
        }
        else if(confirmationText.text != null)
            if(confirmationText.text != "")
            {
                if(Input.GetButtonDown("Submit"))
                {
                    confirmationText.text = "";
                    simul.SignalUserInput();
                }
            }
    }
    
    public void PrintToUser(String text, int type, double time = -1)
    {
        if((type & PrintType.WITH_CONFIRMATION) == PrintType.WITH_CONFIRMATION)
        {
            Debug.Log(text + "\nPress Enter to continue");
            if(notifText != null) confirmationText.text = text + "\nPress Enter to continue";
            else Debug.Log("NotifText = null !");
            //Console.ReadKey();
            Debug.Log("Confirmation");
        }
        else if((type & PrintType.WITH_TIMEOUT) == PrintType.WITH_TIMEOUT)
        {
            Debug.Log(text);
            if(notifText != null) 
            {
                notifText.text = text;
                remainingTime = time;
                isWaitingText = true;
            }
            else Debug.Log("NotifText = null !");
            //Thread.Sleep((int)time * 1000);
            Debug.Log("Wait for " + remainingTime);
        }
        else Debug.Log("PrintToUser : Unknown type");
    }

    public List<int> CheckboxToUser(string message,List<string> choices)
    {
        MainCamera.IsFrozen = true;
        int pas = 0;
        Vector3 cameraposbis,sp, v;
        Camera mycam = MainCamera.GetCamera();
        Vector3 cameraposition =
            MainCamera.GetCameraPosition();
        float x = cameraposition[0];
        float y = cameraposition[1];
        float z = cameraposition[2];
        int _policysize = 30;

        foreach (string ch  in choices)
        {
            UnityEngine.Debug.Log("Add checkbox choice");
            GameObject newobj = new GameObject(name+".chkbox.choice");
            newobj.SetActive(false);
            newobj.AddComponent(typeof(TextMesh));
            newobj.AddComponent(typeof(BoxCollider));
            newobj.GetComponent<TextMesh>().fontSize = _policysize;
            newobj.GetComponent<TextMesh>().fontStyle = FontStyle.Bold;
            newobj.GetComponent<TextMesh>().text = ch;
            TextMesh mesh = newobj.GetComponent<TextMesh>();
            BoxCollider box = newobj.GetComponent<BoxCollider>();
            box.size = new Vector3(GetWidth(mesh), _policysize*0.1f, 1);
            float xx = (float)(box.center[0] + GetWidth(mesh) * 0.5);
            float yy = (float)(box.center[1] - _policysize * 0.1f * 0.5);
            box.center = new Vector3(xx, yy, box.center[2]);

            cameraposbis = new Vector3(x,y,z);
            UnityEngine.Debug.Log(cameraposbis);
            sp = mycam.ViewportToScreenPoint(cameraposbis);
            v = mycam.ScreenToWorldPoint(new Vector3(sp[0]-50, sp[1] + 200 - pas, sp[2] + 50));
            newobj.transform.position= v;
            pas = pas + _policysize + 15;
            newobj.SetActive(true);
        }

        return null;
    }
    
    public List<int> GetCheckboxAnswers()
    {
        return null;
    }

    private void _addCheckboxChoice(string choice)
    {
        int _policysize = 30;

        UnityEngine.Debug.Log("Add checkbox choice");
        GameObject newobj = new GameObject(name+".chkbox.choice");
        newobj.SetActive(true);
        newobj.AddComponent(typeof(TextMesh));
        newobj.AddComponent(typeof(BoxCollider));
        newobj.GetComponent<TextMesh>().fontSize = _policysize;
        newobj.GetComponent<TextMesh>().fontStyle = FontStyle.Bold;
        newobj.GetComponent<TextMesh>().text = choice;
        TextMesh mesh = newobj.GetComponent<TextMesh>();
        BoxCollider box = newobj.GetComponent<BoxCollider>();
        box.size = new Vector3(GetWidth(mesh), _policysize*0.1f, 1);
        float x = (float)(box.center[0] + GetWidth(mesh) * 0.5);
        float y = (float)(box.center[1] - _policysize * 0.1f * 0.5);
        box.center = new Vector3(x, y, box.center[2]);
    }

    public float GetWidth(TextMesh mesh)
    {
        float width = 0;
        foreach (char symbol in mesh.text)
        {
            CharacterInfo info;
            if (mesh.font.GetCharacterInfo(symbol, out info, mesh.fontSize, mesh.fontStyle))
            {
                width += info.advance;
            }
        }
        return width * mesh.characterSize * 0.1f;
    }
}