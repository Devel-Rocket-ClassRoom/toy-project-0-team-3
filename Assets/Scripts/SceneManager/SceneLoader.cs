using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// 씬 전환을 담당하는 싱글톤 매니저
/// 씬을 로드할 때는 SceneManager 대신 이 클래스를 사용
/// 사용 예: SceneLoader.Instance.LoadScene(SceneLoader.GameScene.Dungeon);
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance {get; private set;}

    // 씬이 로드되기 전에 자동으로 실행되어 인스턴스를 생성합니다. (수동으로 씬에 배치할 필요 없음)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        var go = new GameObject("[SceneLoader]");
        Instance = go.AddComponent<SceneLoader>();
        go.AddComponent<DebugSceneSwitcher>(); // 디버그용 씬 전환 UI도 함께 추가
        DontDestroyOnLoad(go);
    }

    // 게임에서 사용하는 씬 목록. 씬을 추가하면 이 enum과 sceneNames에 함께 등록하면 됨
    public enum GameScene
    {
        MainTitle,
        Pub,
        Dungeon
    }

    // enum 값과 실제 씬 파일 이름을 매핑. 씬 이름이 바뀌면 여기서만 수정
    private readonly Dictionary<GameScene, string> sceneNames = new Dictionary<GameScene, string>
    {
        { GameScene.MainTitle, "Prototype_1_Main" },
        { GameScene.Pub, "Prototype_1_Pub" },
        { GameScene.Dungeon, "Prototype_1_Dungeon_Sangwook"}
    };

    private void Awake()
    {
        // 중복 인스턴스 방지 (싱글톤 패턴)
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>씬을 동기 방식으로 즉시 전환. 로딩 중 화면이 잠깐 멈출 수 있습니다.</summary>
    public void LoadScene(GameScene scene)
    {
        SceneManager.LoadScene(sceneNames[scene]);
    }

    /// <summary>씬을 비동기 방식으로 전환. 로딩 중에도 게임이 멈추지 않습니다.</summary>
    /// 후에 던전 입장같은 로딩이 필요한 곳에 로딩창 추가 가능
    public void LoadSceneAsync(GameScene scene)
    {
        SceneManager.LoadSceneAsync(sceneNames[scene]);
    }
}
