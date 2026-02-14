using UnityEngine;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif

/// <summary>
/// Steam 초기화 및 관리. 씬에 하나만 존재해야 합니다.
/// Steamworks.NET 패키지가 설치된 후 동작합니다.
/// </summary>
public class SteamManager : MonoBehaviour
{
    private static SteamManager instance;
    private bool initialized;

    public static bool Initialized
    {
        get { return instance != null && instance.initialized; }
    }

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

#if !DISABLESTEAMWORKS
        try
        {
            if (SteamAPI.RestartAppIfNecessary(new AppId_t(4426340)))
            {
                Application.Quit();
                return;
            }
        }
        catch (System.DllNotFoundException e)
        {
            Debug.LogWarning("[SteamManager] steam_api64.dll을 찾을 수 없습니다. Steam 없이 실행합니다.\n" + e);
            return;
        }

        initialized = SteamAPI.Init();
        if (!initialized)
        {
            Debug.LogWarning("[SteamManager] SteamAPI.Init() 실패. Steam 클라이언트가 실행 중인지 확인하세요.");
        }
        else
        {
            Debug.Log("[SteamManager] Steam 초기화 성공!");
        }
#endif
    }

    void Update()
    {
#if !DISABLESTEAMWORKS
        if (initialized)
            SteamAPI.RunCallbacks();
#endif
    }

    void OnDestroy()
    {
#if !DISABLESTEAMWORKS
        if (instance == this)
        {
            initialized = false;
            SteamAPI.Shutdown();
        }
#endif
    }
}
