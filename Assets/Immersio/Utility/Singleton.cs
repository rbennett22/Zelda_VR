using UnityEngine;


namespace Immersio.Utility
{
    // Summary:
    //      Makes any class that inherits from it a singleton MonoBehaviour,
    //
    // Example Usage:
    //      public class CLASSNAME : Singleton<CLASSNAME> { }
    //
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T _instance;


        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));
                    if (_instance == null)
                    {
                        //print("Singleton :: Instantiating Singleton instance of type " + typeof(T).ToString());

                        GameObject go = new GameObject();
                        DontDestroyOnLoad(go);
                        go.name = GetNameFromType();
                        _instance = go.AddComponent<T>();
                    }
                    DontDestroyOnLoad(_instance);
                }
                return _instance;
            }
        }

        public static bool InstanceExists
        {
            get
            {
                return (_instance != null)
                    || (FindObjectOfType(typeof(T)) != null);
            }
        }


        static string GetNameFromType()
        {
            string name = typeof(T).ToString();
            if (name.Contains("."))
            {
                int startIdx = name.LastIndexOf('.') + 1;
                name = name.Substring(startIdx);
            }
            return name;
        }
    }
}
