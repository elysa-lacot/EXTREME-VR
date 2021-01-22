using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtremeVR
{
    /**
    *  \class ObjectTransform
    *  \author Sofiane Kerrakchou
    *  \brief Contient une position et une orientation
    */
    class ObjectTransform
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    /**
    *  \class ObjectsOriginalTransform
    *  \author Sofiane Kerrakchou
    *  \brief Garde en mémoire la position et l'orientation d'un objet
    */
    class ObjectsOriginalTransform : MonoBehaviour
    {
        private Dictionary<GameObject,ObjectTransform> transformList;

        void Awake()
        {
            transformList = new Dictionary<GameObject,ObjectTransform>();
        }

        public void addObject(GameObject obj)
        {
            Transform t = obj.GetComponent<Transform>();
            if(t == null) return;

            ObjectTransform newT = new ObjectTransform();
            newT.position = t.position;
            newT.rotation = t.rotation;
            
            transformList[obj] = newT;
        }

        public void clearList() { transformList.Clear(); }

        public ObjectTransform getTransform(GameObject o)
        {
            return transformList[o];
        }
    }
}