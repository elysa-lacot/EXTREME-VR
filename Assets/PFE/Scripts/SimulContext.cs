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

    /**
    *  \class SimulContext
    *  \author Sofiane Kerrakchou
    *  \brief Classe représentant le contexte de la simulation
    */
    class SimulContext : MonoBehaviour, ISimulContext
    {
        AudioSource _audiosource;
        public AudioClip _loot;
        public AudioClip _drop;
        /** A SUPPRIMER APRES VERIFICATION */
        public Text notifText;
        /** A SUPPRIMER APRES VERIFICATION */
        public Text inventoryListText;
        public UnityPrint printable;
        /** A SUPPRIMER APRES VERIFICATION */
        public DropObjectsManager Dom;
        /** A SUPPRIMER APRES VERIFICATION */
        public CameraMove camera;
        private Boolean _objectdropped = false;
        /** A SUPPRIMER APRES VERIFICATION */
        private Dictionary<String,GameObject> _inactiveObjects;
        public bool IsSceneFunctionRunning { get { return _s.IsSceneFunctionRunning;}}
        public String ScenarioFile;
        private Scene _s = null;
        public Scene CurrentScene { get {return _s;}}
        private bool _isRunning = false;
        /** Si vrai, le fichier indiqué dans ScenarioFile sera automatiquement chargé */
        public bool DebugMode = false;
        private List<string> _objInventory;
       


        void Awake()
        {
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
            _s.setPrintOutput(printable);
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
                printable.PrintToUser(objName + " pris !",PrintType.WITH_TIMEOUT,3);
                inventoryListText.text += objName + "\n";
            }
        }

        public void DropObj(string objName,string objTag)
        {
            UnityEngine.Debug.Log("On entre dans la fct drop simc");
            if (_objInventory.Contains(objName))
            {
                _audiosource.PlayOneShot(_drop, 0.5F);
                UnityEngine.Debug.Log("nom: " + objName);
                if(!_s.DropObject(objTag))printable.PrintToUser(objName + " non trouvé !", PrintType.WITH_CONFIRMATION, 3);;
                UnityEngine.Debug.Log(objName);
                _objInventory.Remove(objName);
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
            return (Behaviour)GameObject.Find(name).GetComponent("Halo");
        }

        public bool LoadScene(string file)
        {
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
                _inactiveObjects[s].SetActive(true);
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

        /** NE PAS UTILISER (Utilisez l'attribut printable à la place) */
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