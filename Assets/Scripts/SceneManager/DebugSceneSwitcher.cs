using UnityEngine;
using UnityEngine.Rendering;



// 개발 단계에서 씬 전환 하게 해주는 Debug Ui (키보드 단축키로 씬 전환)
public class DebugSceneSwitcher : MonoBehaviour
{
    [SerializeField] private bool showDebugPanel = false;
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            showDebugPanel = !showDebugPanel;

            // 1번 누르면 메인 타이틀 씬 
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SceneLoader.Instance.LoadScene(SceneLoader.GameScene.MainTitle);
            }
            // 2번 누르면 주점 씬
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SceneLoader.Instance.LoadScene(SceneLoader.GameScene.Pub);
            }
            // 3번 누르면 던전 씬
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SceneLoader.Instance.LoadScene(SceneLoader.GameScene.Dungeon);
            }
        }
    }

    private void OnGUI()
    {
        if (!showDebugPanel) return;

        GUI.Box (new Rect (10, 10, 200, 140), "Scene Switcher (F1)");
        
        if (GUI.Button (new Rect(20, 40, 180, 25), "1. Main Title"))
        {
            SceneLoader.Instance.LoadScene(SceneLoader.GameScene.MainTitle);
        }
        if (GUI.Button (new Rect(20, 70, 180, 25), "2. Pub (주점)"))
        {
            SceneLoader.Instance.LoadScene(SceneLoader.GameScene.Pub);
        }
        if (GUI.Button (new Rect(20, 100, 180, 25), "3. Dungeon (던전)"))
        {
            SceneLoader.Instance.LoadScene(SceneLoader.GameScene.Dungeon);
        }
    }
}
