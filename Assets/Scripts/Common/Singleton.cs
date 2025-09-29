using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _quitting = false;

    public static T Instance
    {
        get
        {
            if (_quitting) return null;

            lock (_lock)
            {
                if (_instance != null) return _instance;

                // ������ ã�ƺ���
                _instance = FindObjectOfType<T>();
                if (_instance != null) return _instance;

                // ������ �����ؼ� ����
                var go = new GameObject(typeof(T).Name);
                _instance = go.AddComponent<T>();
                DontDestroyOnLoad(go);
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        // �ߺ� �ν��Ͻ� ����
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _quitting = true;
    }
}
