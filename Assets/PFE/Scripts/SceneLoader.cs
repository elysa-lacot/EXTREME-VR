using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ExtremeVR;

namespace ExtremeVR
{
    /**
    *  \class SceneLoader
    *  \author Sofiane Kerrakchou
    *  \brief Permet de charger un scénario
    */
    public class SceneLoader
    {
        private static string sceneFile;
        
        /** Contient le nom du fichier scénario à charger */
        public static string SceneToLoad { get { return sceneFile; } set { sceneFile = value; }}

        /** Charge le scénario indiqué l'attribut SceneToLoad \n
        * A modifier : mettre la classe en DontDestroyOnLoad permettrait de simplifier le chargement et ne pas utiliser la méthode comme coroutine:
        * => Permettrait d'utiliser un chargement Single et non Additive
        * => Plus besoin de désactiver les objets manuellement
        */
        public static IEnumerator LoadScene()
        {
            GameObject Player = PlayerSingleton.GetInstance();
            float gravity = 0;

            //Scène unity à charger
            UnityEngine.SceneManagement.Scene unityScene;
            UnityEngine.SceneManagement.Scene currentScene = SceneManager.GetActiveScene();
            SimulContext sc = null;
            ObjectsOriginalTransform oot = null;
            int count = SceneManager.sceneCount;

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
                if(gameObj[i].GetComponent(typeof(ObjectsOriginalTransform)) != null)
                {
                    oot = gameObj[i].GetComponent(typeof(ObjectsOriginalTransform)) as ObjectsOriginalTransform;
                    PlayerSingleton.GetInstance().GetComponentInChildren<ExtremeVR.CollectObjects>().oot = oot;
                }
            }
            GameObject [] allObjects = Object.FindObjectsOfType<GameObject>();
            foreach (GameObject o in allObjects)
            {
                if(LayerMask.LayerToName(o.layer).Contains("Grabbable"))
                {
                    oot.addObject(o);
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