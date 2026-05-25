using DoorControl;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameManager : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenu";

    public static GameManager Instance { get; private set; }

    [SerializeField] private bool _shouldStartNewGame = false;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == MainMenuSceneName || _shouldStartNewGame)
        {
            StartNewGame();
        }
    }

    public void StartNewGame()
    {
        GameStateManager.ResetGameState();

        _shouldStartNewGame = false;

        PlayerPrefs.DeleteKey("LastChance_Active");
        PlayerPrefs.DeleteKey("ArmorPlates_Used");

        SaveSystem.Instance?.DeleteSaveData();

        ResetKeyCollectionUI();
    }

    public void SetNewGameFlag()
    {
        _shouldStartNewGame = true;
    }

    private void ResetKeyCollectionUI()
    {
        KeyCollection keyCollection = FindObjectOfType<KeyCollection>();

        keyCollection?.ResetAllKeys();
    }
}