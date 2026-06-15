using Firebase.Database;
using Newtonsoft.Json;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    FirebaseDatabase database;
    DatabaseReference reference;
    UnityMainThreadDispatcher dispatcher;

    [Header("Firebase")]
    [SerializeField] string databaseUrl = "https://sarang-dbcae-default-rtdb.asia-southeast1.firebasedatabase.app/";

    [Header("UI")]
    [SerializeField] Text SwordCountText;
    [SerializeField] Text ShieldCountText;
    [SerializeField] Text ElixirCountText;
    [SerializeField] Text MessageText;

    string userKey;
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

        LoadInventory();
    }

    // Firebase에서 인벤토리 불러와서 화면에 표시
    void LoadInventory()
    {
        reference
            .Child("UserInfo")
            .Child(userKey)
            .Child("Inventory")
            .GetValueAsync()
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    dispatcher.Enqueue(() =>
                    {
                        MessageText.text = "인벤토리 불러오기 실패";
                    });
                    return;
                }

                DataSnapshot snapshot = task.Result;

                if (snapshot.Value == null)
                {
                    dispatcher.Enqueue(() =>
                    {
                        MessageText.text = "인벤토리 데이터가 없습니다.";
                    });
                    return;
                }

                // Inventory JSON 문자열 → Dictionary로 변환
                string inventoryJson = snapshot.Value.ToString();
                inventory = JsonConvert.DeserializeObject<Dictionary<string, int>>(inventoryJson);

                dispatcher.Enqueue(() =>
                {
                    RefreshUI();
                    MessageText.text = "인벤토리 불러오기 완료";
                });
            });
    }

    // UI에 아이템 개수 갱신
    void RefreshUI()
    {
        SwordCountText.text = "Sword : " + GetItemCount("Sword");
        ShieldCountText.text = "Shield : " + GetItemCount("Shield");
        ElixirCountText.text = "Elixir : " + GetItemCount("Elixir");
    }

    int GetItemCount(string itemName)
    {
        if (inventory.ContainsKey(itemName))
        {
            return inventory[itemName];
        }
        return 0;
    }

    // 각 버튼에 연결
    public void OnClickUseSword()
    {
        UseItem("Sword");
    }

    public void OnClickUseShield()
    {
        UseItem("Shield");
    }

    public void OnClickUseElixir()
    {
        UseItem("Elixir");
    }

    // 아이템 사용 처리 - 0개면 사용 불가, 있으면 -1 후 Firebase 저장
    void UseItem(string itemName)
    {
        // 아이템 0개면 사용 불가
        if (!inventory.ContainsKey(itemName) || inventory[itemName] <= 0)
        {
            MessageText.text = itemName + " 개수가 부족합니다.";
            return;
        }

        inventory[itemName]--;
        SaveInventory(itemName);
    }

    // 변경된 Inventory Firebase에 저장
    void SaveInventory(string usedItemName)
    {
        string inventoryJson = JsonConvert.SerializeObject(inventory);

        reference
            .Child("UserInfo")
            .Child(userKey)
            .Child("Inventory")
            .SetValueAsync(inventoryJson)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    dispatcher.Enqueue(() =>
                    {
                        MessageText.text = "인벤토리 저장 실패";
                    });
                    return;
                }

                dispatcher.Enqueue(() =>
                {
                    RefreshUI();

                    // 아이템별 사용 메시지 다르게 출력
                    if (usedItemName == "Sword")
                        MessageText.text = "Sword를 장착했습니다! 공격력이 상승합니다.";
                    else if (usedItemName == "Shield")
                        MessageText.text = "Shield를 들었습니다! 방어력이 상승합니다.";
                    else if (usedItemName == "Elixir")
                        MessageText.text = "Elixir를 마셨습니다! HP가 회복됩니다.";
                });
            });
    }
}