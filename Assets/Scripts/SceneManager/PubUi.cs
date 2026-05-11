using UnityEngine;
using UnityEngine.UI;

public class PubUi : MonoBehaviour
{
    [SerializeField]
    private Button toMainButton;

    [SerializeField]
    private Button toDungeonButton;

    private void Start()
    {
        toMainButton.onClick.AddListener(OnBackToMain);
        toDungeonButton.onClick.AddListener(OnEnterDungeon);
    }

    private void OnBackToMain()
    {
        SceneLoader.Instance.LoadScene(SceneLoader.GameScene.MainTitle);
    }

    private void OnEnterDungeon()
    {
        SceneLoader.Instance.LoadScene(SceneLoader.GameScene.Dungeon);
    }
}
