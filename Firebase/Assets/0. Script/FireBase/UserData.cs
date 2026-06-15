using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class UserData
{
    public string NickName;
    public int Coin;
    public int Score;

    // Dictionary는 JsonUtility로 바로 저장하기 어렵기 때문에
    // Newtonsoft.Json으로 JSON 문자열로 변환해서 저장한다.
    public string UnitList;
    public string Inventory;

    public UserData()
    {
    }

    public UserData(string nickName)
    {
        NickName = nickName;
        Coin = 500;
        Score = 0;

        // 기본 유닛 세팅 - Unit1만 보유한 상태로 시작
        Dictionary<string, bool> unitList = new Dictionary<string, bool>();
        unitList["Unit1"] = true;

        for (int i = 2; i <= 6; i++)
        {
            unitList["Unit" + i] = false;
        }

        // 기본 인벤토리 - 아이템 3종류 전부 0개로 시작
        Dictionary<string, int> inventory = new Dictionary<string, int>();
        inventory["Sword"] = 0;
        inventory["Shield"] = 0;
        inventory["Elixir"] = 0;

        UnitList = JsonConvert.SerializeObject(unitList);
        Inventory = JsonConvert.SerializeObject(inventory);
    }
}