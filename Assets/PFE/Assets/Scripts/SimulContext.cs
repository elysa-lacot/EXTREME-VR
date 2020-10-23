using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using static Scene;
using static PrintType;


interface ISimulContext
{
    bool LoadScene(string file);
    bool RestartScene();
    void SetUserFreeze(bool isFrozen);
}

public class SimulContext : MonoBehaviour, ISimulContext
{
    // Start is called before the first frame update
    AudioSource _audiosource;
    public AudioClip _loot;
    public AudioClip _drop;
    public Text notifText;
    public Text inventoryListText;
    public UnityPrint printable;
    public DropObjectsManager Dom;
    public CameraMove camera;
    private Boolean _objectdropped = false;
    private Dictionary<String,GameObject> _inactiveObjects;
    public bool IsSceneFunctionRunning { get { return _s.IsSceneFunctionRunning;}}
    void Awake()
    {
        //Debug.Log("Awake Method");
        LoadScene("scene01a");
        //Run();
        _audiosource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsSceneFunctionRunning) WaitCtrlPress();
        if(_objectdropped == true)
        {
            UpdateInventoryListText();
            _objectdropped = false;
        }

    }

    private Scene _s = null;
    private bool _isRunning = false;
    private List<string> _objInventory;
    public void Run()
    {
        if(_s == null)
        {
            Debug.Log("Please load a scene before calling the Run() method");
            return;
        }
        _isRunning = true;
        _objInventory = new List<string>();
        //printable = new UnityPrint();
        //printable.notifText = this.notifText;
        _s.setPrintOutput(printable);
        //s.addTakeObjectUnorderedTask("123");
        StartCoroutine(_s.FirstTry());
        _isRunning = false;
    }

    public void TakeObj(string name)
    {
        UnityEngine.Debug.Log("ça passe takeobj");
        if(!_objInventory.Contains(name))
        {
            _audiosource.PlayOneShot(_loot, 0.5F);
            UnityEngine.Debug.Log("ça passe le if");
            _s.TakeObject(name);
            _objInventory.Add(name);
            _inactiveObjects.Add(name,GameObject.Find(name));
            GameObject.Find(name).SetActive(false);
            printable.PrintToUser(name + " pris !",PrintType.WITH_TIMEOUT,3);
            inventoryListText.text += name + "\n";
            Dom.CreateNewObject(name);
        }
    }

    public void DropObj(string name)
    {
        UnityEngine.Debug.Log("On entre dans la fct drop simc");
        if (_objInventory.Contains(name))
        {
            _audiosource.PlayOneShot(_drop, 0.5F);
            UnityEngine.Debug.Log("nom: " + name);
            _s.DropObject(name);
            UnityEngine.Debug.Log(name);
            _objInventory.Remove(name);
           //GameObject.Find(name).SetActive(true);
           _inactiveObjects[name].SetActive(true);
            _inactiveObjects.Remove(name);
            printable.PrintToUser(name + " enlevé !", PrintType.WITH_TIMEOUT, 3);
            _objectdropped = true;
        }
    }

    public void UpdateInventoryListText()
    {
        inventoryListText.text = "";
        foreach(String name in _objInventory)
        {
            inventoryListText.text += name + "\n";
        } 
    }

    public void WaitCtrlPress()
    {
        if (Input.GetButtonDown("Drop"))
        {
            UnityEngine.Debug.Log("Ctrl presse");
            if (Dom.ctrlpressed == true)
            {
                Dom.UnPrint();
                camera.IsFrozen = false;
            }
            else
            {
                Camera mycam = camera.GetComponent<Camera>();
                Dom.cameraposition = mycam.ScreenToViewportPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mycam.nearClipPlane));
                camera.IsFrozen = true;
                Dom.PrintDropList();
            }
            Dom.ctrlpressed = !Dom.ctrlpressed;
        }
    }

    public Behaviour GetHaloComponent(String name)
    {
        return (Behaviour)GameObject.Find(name).GetComponent("Halo");
    }

    public bool LoadScene(string file)
    {
        _s = FileTools.LoadTextFile(file);
        _s.SimulContext = this;
        _objInventory = new List<string>();
        _inactiveObjects = new Dictionary<string, GameObject>();
        //printable = new UnityPrint();
        //printable.notifText = this.notifText;
        _s.setPrintOutput(printable);
        //s.addTakeObjectUnorderedTask("123");
        StartCoroutine(_s.FirstTry());
        return true;
    }

    public bool RestartScene()
    {
        _objInventory.Clear();
        inventoryListText.text = "";
        return false;
    }

    public Boolean Validate()
    {
        StartCoroutine(_s.CheckTasks());
        return true;
    }

    //DO NOT USE (Use the printable attribute instead)
    public void PrintNotif(string text)
    {
        notifText.text = text;
    }

    public void SetUserFreeze(bool isFrozen)
    {
        camera.IsFrozen = isFrozen;
    }

    public void SignalUserInput()
    {
        _s.SignalUserInput();
    }
}
