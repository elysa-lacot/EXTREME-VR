using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

namespace ExtremeVR
{
    public class MainMenu : MonoBehaviour
    {
        private int selectedIndex;
        public Text mainText;
        public Text levelFileText;
        public GameObject LeftArrowImage;
        public GameObject RightArrowImage;
        private bool axisReleased;
        private List<string> scenarioFiles;
        
        // Start is called before the first frame update
        void Start()
        {
            
            askPermission() ;

            try
            {
                //DirectoryInfo dir = new DirectoryInfo("Assets/PFE/Resources/Scenario");
                DirectoryInfo dir = new DirectoryInfo("/mnt/sdcard/scenario");
                //DirectoryInfo dir = new DirectoryInfo("/storage/emulated/0/Android/data/com.DefaultCompany.PFE/files/Assets/PFE/Resources/Scenario");
                //DirectoryInfo dir = new DirectoryInfo("/mnt/sdcard/Android/obb/com.DefaultCompany.PFE/Assets/PFE/Resources/Scenario");
                 FileInfo[] info = dir.GetFiles("*.txt");
                scenarioFiles = new List<string>();
                UnityEngine.SceneManagement.Scene currentScene = SceneManager.GetActiveScene();
                GameObject[] currentObj = currentScene.GetRootGameObjects();
                /*for (int i = 0; i < currentObj.Length; i++)
                {
                    if (currentObj[i].GetComponent(typeof(OVRCameraRig)) != null || currentObj[i].GetComponent(typeof(OVRPlayerController)) != null)
                        DontDestroyOnLoad(currentObj[i]);
                }*/

                foreach (FileInfo f in info)
                {
                    scenarioFiles.Add(f.Name.Replace(".txt",""));
                }

                if(scenarioFiles.Count == 0)
                {
                    levelFileText.text = "ERROR : NO SCENARIO FILE FOUND";
                    return;
                }

                selectedIndex = 0;
                LeftArrowImage.SetActive(false);
                if(scenarioFiles.Count > 1) RightArrowImage.SetActive(true);
                levelFileText.text = scenarioFiles[0];
                //Application.persistentDataPath
                axisReleased = true;
            }
            catch(Exception e)
            {
                levelFileText.text = e.Message ;
            } 
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Debug.Log("Loading...");
                mainText.text = "Chargement...";
                //sceneFile = scenarioFiles[selectedIndex];
                SceneLoader.SceneToLoad = scenarioFiles[selectedIndex];
                StartCoroutine(SceneLoader.LoadScene());
            }
            if (Input.GetButtonDown("Fire2") && selectedIndex < (scenarioFiles.Count - 1))
            {
                selectedIndex++;
                levelFileText.text = scenarioFiles[selectedIndex];
                if(scenarioFiles.Count -1 <= selectedIndex) RightArrowImage.SetActive(false);
                LeftArrowImage.SetActive(true);
                axisReleased = false;
            }
            if (Input.GetButtonDown("Jump") && selectedIndex > 0)
            {
                selectedIndex--;
                levelFileText.text = scenarioFiles[selectedIndex];
                if(selectedIndex == 0) LeftArrowImage.SetActive(false);
                RightArrowImage.SetActive(true);
                axisReleased = false;
            }
            if (Input.GetAxis("Horizontal") >= -0.25 && Input.GetAxis("Horizontal") <= 0.25)
            {
                axisReleased = true;
            }
        }

        void askPermission()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead) || 
                !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            }
        }
    }
}