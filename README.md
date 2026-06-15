# Firebase 유저 데이터 관리 시스템

**이름:** 박사랑  
**학번:** 3B  
**과목:** 온라인 프로그래밍  

---

## 구현한 기능 목록

- 회원가입 (닉네임 입력, 중복 검사, Firebase 저장)
- 로그인 (닉네임으로 Firebase에서 유저 검색)
- 코인 불러오기 (Firebase에서 Coin 값 읽어서 UI 표시)
- 아이템 구매 (코인 차감, 인벤토리 증가, Firebase 저장)
- 코인 부족 시 구매 불가 처리
- 인벤토리 표시 (Firebase에서 아이템 개수 불러오기)
- 아이템 사용 (개수 감소, Firebase 저장, 아이템별 메시지 출력)

---

## 아이템 설명

| 아이템 | 설명 |
|--------|------|
| Sword (검) | 장착하면 공격력이 상승하는 무기 아이템 |
| Shield (방패) | 장착하면 방어력이 상승하는 방어 아이템 |
| Elixir (엘릭서) | 사용하면 HP가 회복되는 회복 아이템 |

---

## 아이템 가격표

| 아이템 | 가격 |
|--------|------|
| Sword (검) | 100 코인 |
| Shield (방패) | 200 코인 |
| Elixir (엘릭서) | 300 코인 |

---

## Firebase 데이터 구조

```
UserInfo
 └── 유저고유키 (Push로 자동 생성)
      ├── NickName  : 유저 닉네임 (string)
      ├── Coin      : 보유 코인 (int)
      ├── Score     : 점수 (int)
      ├── UnitList  : 보유 유닛 목록 (JSON 문자열)
      └── Inventory : 아이템 보유 개수 (JSON 문자열)
```

**저장 예시:**
```json
{
  "UserInfo": {
    "-Ov9obs7O0vuXIUuysMQ": {
      "NickName": "박사랑",
      "Coin": 200,
      "Score": 0,
      "UnitList": "{\"Unit1\":true,\"Unit2\":false,\"Unit3\":false}",
      "Inventory": "{\"Sword\":1,\"Shield\":1,\"Elixir\":0}"
    }
  }
}
```

---

## PlayerPrefs에 저장한 값

| 키 | 값 | 설명 |
|----|-----|------|
| UserKey | -Ov9obs7O0vuXIUuysMQ | Firebase 유저 고유키 |
| UserNickName | 박사랑 | 로그인한 유저 닉네임 |

로그인 또는 회원가입 성공 시 PlayerPrefs에 저장되며,
ShopManager와 InventoryManager에서 UserKey를 불러와 Firebase 경로를 찾는 데 사용한다.

---

## Inventory JSON 처리 방식

인벤토리는 `Dictionary<string, int>` 형태로 관리한다.

Firebase에는 JSON 문자열로 저장되며, 불러올 때 `Newtonsoft.Json`으로 파싱한다.

**저장할 때:**
```csharp
string inventoryJson = JsonConvert.SerializeObject(inventory);
// 결과: {"Sword":1,"Shield":1,"Elixir":0}
```

**불러올 때:**
```csharp
inventory = JsonConvert.DeserializeObject<Dictionary<string, int>>(inventoryJson);
```

`JsonUtility`는 Dictionary를 지원하지 않기 때문에 반드시 `Newtonsoft.Json`을 사용해야 한다.

---

## 씬 구성

| 씬 이름 | 역할 |
|---------|------|
| SampleScene | 로그인 / 회원가입 화면 |
| ShopScene | 상점 - 코인으로 아이템 구매 |
| InventoryScene | 인벤토리 - 아이템 확인 및 사용 |

---

## 실행 중 발생한 문제와 해결 방법

**문제 1. Coin이 0으로 저장됨**  
원인: `JsonUtility.ToJson()`은 int 필드를 제대로 직렬화하지 못함  
해결: `JsonConvert.SerializeObject()`로 변경

**문제 2. UnityMainThreadDispatcher 오류**  
원인: Firebase 콜백은 별도 스레드에서 실행되어 UI 접근 불가  
해결: 각 씬에 `MainThreadExecutor` 프리팹 추가

**문제 3. Newtonsoft.Json을 찾을 수 없음**  
원인: 패키지가 설치되지 않음  
해결: Package Manager에서 `com.unity.nuget.newtonsoft-json` 설치

**문제 4. 씬 전환 안 됨**  
원인: Build Profiles에 씬이 등록되지 않음  
해결: File → Build Profiles에서 씬 3개 모두 추가

**문제 5. GitHub Push 용량 초과**  
원인: Firebase 플러그인 파일이 100MB 초과  
해결: Git LFS 설정으로 대용량 파일 처리
