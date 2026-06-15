using Firebase.Database;
using Newtonsoft.Json;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    FirebaseDatabase database;
    DatabaseReference reference;
    UnityMainThreadDispatcher dispatcher;

    [Header("Firebase")]
    [SerializeField] string databaseUrl = "https://sarang-dbcae-default-rtdb.asia-southeast1.firebasedatabase.app/";

    [Header("UI")]
    [SerializeField] Text CoinText;
    [SerializeField] Text MessageText;

    string userKey;
    int currentCoin;
    Dictionary<string, int> inventory = new Dictionary<string, int>();

    void Start()
    {
        database = FirebaseDatabase.GetInstance(databaseUrl);
        reference = database.RootReference;
        dispatcher = UnityMainThreadDispatcher.Instance();

        // PlayerPrefs에서 내 UserKey 불러오기
        userKey = PlayerPrefs.GetString("UserKey");

        if (string.IsNullOrEmpty(userKey))
        {
            MessageText.text = "로그인 정보가 없습니다.";
            return;
        }

        LoadUserData();
    }

    // Firebase에서 코인이랑 인벤토리 불러오기
    void LoadUserData()
    {
        reference
            .Child("UserInfo")
            .Child(userKey)
            .GetValueAsync()
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    dispatcher.Enqueue(() =>
                    {
                        MessageText.text = "유저 정보 불러오기 실패";
                    });
                    return;
                }

                DataSnapshot snapshot = task.Result;

                // Coin 값 가져오기
                currentCoin = int.Parse(snapshot.Child("Coin").Value.ToString());

                // Inventory JSON 문자열 → Dictionary로 변환
                string inventoryJson = snapshot.Child("Inventory").Value.ToString();
                inventory = JsonConvert.DeserializeObject<Dictionary<string, int>>(inventoryJson);

                dispatcher.Enqueue(() =>
                {
                    RefreshUI();
                    MessageText.text = "유저 정보 불러오기 완료";
                });
            });
    }

    void RefreshUI()
    {
        CoinText.text = "Coin : " + currentCoin;
    }

    // 각 버튼에 연결 - 아이템 이름이랑 가격 전달
    public void OnClickBuySword()
    {
        BuyItem("Sword", 100);
    }

    public void OnClickBuyShield()
    {
        BuyItem("Shield", 200);
    }

    public void OnClickBuyElixir()
    {
        BuyItem("Elixir", 300);
    }

    // 아이템 구매 처리 - 코인 확인 → 차감 → Firebase 저장
    void BuyItem(string itemName, int price)
    {
        // 코인 부족하면 구매 불가
        if (currentCoin < price)
        {
            MessageText.text = "코인이 부족합니다.";
            return;
        }

        // 코인 차감하고 아이템 개수 +1
        currentCoin -= price;

        if (inventory.ContainsKey(itemName))
        {
            inventory[itemName]++;
        }
        else
        {
            inventory[itemName] = 1;
        }

        SaveUserData(itemName);
    }

    // 변경된 Coin이랑 Inventory Firebase에 저장
    void SaveUserData(string boughtItemName)
    {
        string inventoryJson = JsonConvert.SerializeObject(inventory);

        Dictionary<string, object> updateData = new Dictionary<string, object>();
        updateData["Coin"] = currentCoin;
        updateData["Inventory"] = inventoryJson;

        reference
            .Child("UserInfo")
            .Child(userKey)
            .UpdateChildrenAsync(updateData)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    dispatcher.Enqueue(() =>
                    {
                        MessageText.text = "구매 저장 실패";
                    });
                    return;
                }

                dispatcher.Enqueue(() =>
                {
                    RefreshUI();
                    MessageText.text = boughtItemName + " 구매 완료! (남은 코인 : " + currentCoin + ")";
                });
            });
    }
}