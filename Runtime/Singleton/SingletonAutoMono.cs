using UnityEngine;

namespace Base
{
    public class SingletonAutoMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T GetInstance()
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject
                {
                    name = typeof(T).ToString()
                };
                _instance = obj.AddComponent<T>();
            }

            return _instance;
        }
    }
}
