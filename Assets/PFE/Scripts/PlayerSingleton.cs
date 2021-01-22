using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtremeVR;

namespace ExtremeVR{

    /**
    *  \class PlayerSingleton
    *  \author Sofiane Kerrakchou
    *  \brief Permet de garder le même Player durant toute l'exécution (le tracking de l'Oculus Quest ne fonctionne plus si l'on change de Player, même en supprimant l'ancien)
    */
    public class PlayerSingleton : MonoBehaviour
    {
        private static GameObject instance = null;
        void Awake()
        {
            if (instance == null)
            {
                instance = this.gameObject;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        public static GameObject GetInstance()
        {
            return instance;
        }
    }
}
