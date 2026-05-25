using UnityEngine;

public sealed class ExecutionOrderEnforcer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoad()
    {
        CreateSystemManager<EnemyManager>("EnemyManager");
        CreateSystemManager<SaveSystem>("SaveSystem");
    }

    private static void CreateSystemManager<T>(string managerName) where T : Component
    {
        if (FindObjectOfType<T>() != null)
        {
            return;
        }

        GameObject manager = new GameObject(managerName);

        manager.AddComponent<T>();

        DontDestroyOnLoad(manager);
    }
}