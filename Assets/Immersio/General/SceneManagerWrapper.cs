using UnityEngine;
using UnityEngine.SceneManagement;

namespace Immersio.Utility
{
    public class SceneManagerWrapper : Singleton<SceneManagerWrapper>
    {
        string _uniqueScene = null;
        string _reloadingScene = null;


        public string UniqueScene { get { return _uniqueScene; }  }


        override protected void Awake()
        {
            base.Awake();

            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }


        public void LoadCommonScene(string name)
        {
            print(" ~~~ LoadCommonScene: " + name);

            SceneManager.LoadScene(name, LoadSceneMode.Additive);
        }

        public void LoadUniqueScene(string name)
        {
            print(" *** LoadUniqueScene: " + name);

            if (string.IsNullOrEmpty(name) || SceneManager.GetSceneByName(name).isLoaded)
            {
                return;
            }

            // Unload previous unique scene
            if (SceneManager.GetSceneByName(_uniqueScene).isLoaded)
            {
                SceneManager.UnloadSceneAsync(_uniqueScene);
            }

            SceneManager.LoadScene(name, LoadSceneMode.Additive);

            // Assign new loaded scene as the current unique scene
            _uniqueScene = name;
        }

        public void ReloadCurrentUniqueScene()
        {
            if (string.IsNullOrEmpty(_uniqueScene))
            {
                return;
            }

            _reloadingScene = _uniqueScene;
            SceneManager.UnloadSceneAsync(_reloadingScene);
        }
        void OnSceneUnloaded(Scene s)
        {
            //print(" OnSceneUnloaded: " + s);

            if (_reloadingScene != null)
            {
                LoadUniqueScene(_reloadingScene);
                _reloadingScene = null;
            }
        }
    }
}