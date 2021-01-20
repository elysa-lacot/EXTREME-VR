using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ExtremeVR;

namespace ExtremeVR
{

    public class SceneLoader
    {
        // Start is called before the first frame update
        //public Text MainText;
        private static string sceneFile;
        
        public static string SceneToLoad { get { return sceneFile; } set { sceneFile = value; }}


        public static IEnumerator LoadScene()
        {
            GameObject Player = PlayerSingleton.GetInstance();
            float gravity = 0;

            /*
            AsyncOperation asyncLoad;
            asyncLoad = SceneManager.LoadSceneAsync("DefaultScene", LoadSceneMode.Single);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            */
            UnityEngine.SceneManagement.Scene unityScene;
            UnityEngine.SceneManagement.Scene currentScene = SceneManager.GetActiveScene();
            SimulContext sc = null;
            int count = SceneManager.sceneCount;
            //currentScene.name += ".toBeRemoved";

            ExtremeVR.Scene extremeVRScene;
            extremeVRScene = FileTools.LoadTextFile(sceneFile);
            if(extremeVRScene.UnityScene == "") extremeVRScene.UnityScene = "DefaultScene";
            Debug.Log("Unity Scene : " + extremeVRScene.UnityScene);

            if(currentScene.name != extremeVRScene.UnityScene)
            {
                GameObject[] currentObj = currentScene.GetRootGameObjects();
                for(int i = 0;i<currentObj.Length;i++)
                {
                    if(currentObj[i].GetComponent(typeof(SimulContext)) == null && currentObj[i].GetComponent(typeof(MainMenu)) == null && currentObj[i].GetComponent(typeof(OVRCameraRig)) == null && currentObj[i].GetComponent(typeof(OVRPlayerController)) == null)
                        currentObj[i].SetActive(false);                   
                }

                gravity = Player.GetComponent<OVRPlayerController>().GravityModifier;
                Player.GetComponent<OVRPlayerController>().GravityModifier = 0;

                AsyncOperation asyncLoad;
                asyncLoad = SceneManager.LoadSceneAsync(extremeVRScene.UnityScene,LoadSceneMode.Additive);

                while (!asyncLoad.isDone)
                {
                    yield return null;
                }

                unityScene = SceneManager.GetSceneAt(count);
                Debug.Log("IsValid => " + unityScene.IsValid());
            }
            else unityScene = currentScene;

            GameObject[] gameObj = unityScene.GetRootGameObjects();
            Debug.Log("gameObj Count " + gameObj.Length);
            for(int i = 0;i<gameObj.Length;i++)
            {
                Debug.Log("name " + gameObj[i].name);
                if(gameObj[i].GetComponent(typeof(SimulContext)) != null)
                {
                    sc = gameObj[i].GetComponent(typeof(SimulContext)) as SimulContext;
                    sc.LoadScene(extremeVRScene);
                    PlayerSingleton.GetInstance().GetComponentInChildren<ExtremeVR.CollectObjects>().SetSimulContext(sc);
                }
            }
            foreach (Scene.ObjectOptions ob in sc.CurrentScene.ObjOptions)
            {
                if(ob.type == Scene.ObjectOptions.TYPE_OBJ_NAME)
                {
                    GameObject go;
                    go = GameObject.Find(ob.name);
                    go.SetActive(ob.isShown);
                }
                else if(ob.type == Scene.ObjectOptions.TYPE_OBJ_TAG)
                {
                    GameObject[] gos;
                    gos = GameObject.FindGameObjectsWithTag(ob.name);
                    foreach (GameObject i in gos)
                    {
                        i.SetActive(ob.isShown);
                    }
                }
            }
            Debug.Log("CONFIGURED");

            Player.GetComponent<OVRPlayerController>().GravityModifier = gravity;
            if (currentScene.name != extremeVRScene.UnityScene) SceneManager.UnloadSceneAsync(currentScene);
           
            yield break;
        }
    }
}