using UnityEngine;
public class TopUIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject shopPanel; 

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }

    public void OpenShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            Debug.Log("상점 패널이 활성화되었습니다.");
        }
    }

    public void CloseShop()
    {
        if (shopPanel != null && shopPanel.activeSelf)
        {
            shopPanel.SetActive(false);
        }
    }

}