using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void GoToInventory()
    {
        SceneManager.LoadScene("InventoryScene");
    }

    public void GoToShop()
    {
        SceneManager.LoadScene("ShopScene");
    }
}