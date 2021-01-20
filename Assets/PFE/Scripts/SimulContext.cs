using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using ExtremeVR;


namespace ExtremeVR
{

    interface ISimulContext
    {
        bool LoadScene(string file);
        bool RestartScene();
        void SetUserFreeze(bool isFrozen);
    }

    class SimulContext : MonoBehaviour, ISimulContext
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
        public String ScenarioFile;
        private Scene _s = null;
        public Scene CurrentScene { get {return _s;}}
        private bool _isRunning = false;
        public bool DebugMode = false;
        private List<string> _objInventory;
       


        void Awake()
        {
            //Debug.Log("Awake Method");
            //LoadScene(ScenarioFile);
            //Run();
            _audiosource = GetComponent<AudioSource>();
            printable = PlayerSingleton.GetInstance().GetComponentInChildren<UnityPrint>();
            printable.simul = this;
        }

        void Start()
        {
            if(ScenarioFile != "" && DebugMode)
            {
                LoadScene(ScenarioFile);
            }
            else
            {
                printable.PrintToUser("NO SCENE LOADED !",PrintType.WITH_CONFIRMATION);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(_isRunning)
            {
                if(!IsSceneFunctionRunning) WaitCtrlPress();
                if(_objectdropped == true)
                {
                    UpdateInventoryListText();
                    _objectdropped = false;
                }
            }
            if (Input.GetButtonDown("Fire3"))
            {
                Validate();
            }

        }

        public void Run()
        {
            if(_s == null)
            {
                Debug.Log("Please load a scene before calling the Run() method");
                return;
            }
            _isRunning = true;
            _objInventory = new List<string>();
            UpdateInventoryListText();
            //printable = new UnityPrint();
            //printable.notifText = this.notifText;
            _s.setPrintOutput(printable);
            //s.addTakeObjectUnorderedTask("123");
            StartCoroutine(StartFirstTry());
            _isRunning = false;
        }

        private IEnumerator StartFirstTry()
        {
            yield return StartCoroutine(_s.FirstTry());
            yield return StartCoroutine(_s.AtStart());
        }

        public void TakeObj(string objName,string objTag)
        {
            UnityEngine.Debug.Log("ça passe takeobj");
            if(!_objInventory.Contains(objName))
            {
                _audiosource.PlayOneShot(_loot, 0.5F);
                UnityEngine.Debug.Log("ça passe le if");
                _s.TakeObject(objTag);
                _objInventory.Add(objName);
                //_inactiveObjects.Add(objName,GameObject.Find(objName));
                //GameObject.Find(objName).SetActive(false);
                printable.PrintToUser(objName + " pris !",PrintType.WITH_TIMEOUT,3);
                inventoryListText.text += objName + "\n";
                //Dom.CreateNewObject(objName);
            }
        }

        public void DropObj(string objName,string objTag)
        {
            UnityEngine.Debug.Log("On entre dans la fct drop simc");
            if (_objInventory.Contains(objName))
            {
                _audiosource.PlayOneShot(_drop, 0.5F);
                UnityEngine.Debug.Log("nom: " + objName);
                _s.DropObject(objTag);
                UnityEngine.Debug.Log(objName);
                _objInventory.Remove(objName);
            //GameObject.Find(name).SetActive(true);
                //_inactiveObjects[objName].SetActive(true);
                //_inactiveObjects.Remove(objName);
                printable.PrintToUser(objName + " enlevé !", PrintType.WITH_TIMEOUT, 3);
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
            //GameObject.Find(name).SetActive(false);
            return (Behaviour)GameObject.Find(name).GetComponent("Halo");
        }

        public bool LoadScene(string file)
        {
            /*_s = FileTools.LoadTextFile(file);
            _s.SimulContext = this;
            _objInventory = new List<string>();
            _inactiveObjects = new Dictionary<string, GameObject>();
            //printable = new UnityPrint();
            //printable.notifText = this.notifText;
            _s.setPrintOutput(printable);
            //s.addTakeObjectUnorderedTask("123");
            StartCoroutine(_s.FirstTry());*/
            SceneLoader.SceneToLoad = file;
            StartCoroutine(SceneLoader.LoadScene());
            return true;
        }

        public void LoadScene(ExtremeVR.Scene scene)
        {
            _s = scene;
            _s.SimulContext = this;
            _inactiveObjects = new Dictionary<string, GameObject>();

            Run();
        }

        public bool RestartScene()
        {
            foreach (string s in _objInventory)
            {
                Debug.Log("Inventory => " + s);
                _inactiveObjects[s].SetActive(true);
                //GameObject.Find(s).SetActive(true);
            }
            _objInventory.Clear();
            UpdateInventoryListText();
            inventoryListText.text = "";
            StartCoroutine(_s.AtStart());
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
}