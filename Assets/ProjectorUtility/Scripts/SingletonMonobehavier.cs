using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Common
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (T)FindObjectOfType(typeof(T));

                    if (instance == null)
                    {
                        Debug.LogWarning(typeof(T) + "is nothing on scene");
                    }
                }
                return instance;
            }
        }

        protected virtual void Awake()
        {
            Initialize();
        }

        void Initialize()
        {
            if (instance != null)
            {
                Destroy(this.gameObject);
                return;
            }
            instance = this as T;
            Debug.Log(this.GetType().Name + " Initialize.");
        }
    }
}