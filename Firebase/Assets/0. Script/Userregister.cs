using Firebase.Database;
using PimDeWitte.UnityMainThreadDispatcher;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UserRegister : MonoBehaviour
{
    FirebaseDatabase database;
    DatabaseReference reference;
    UnityMainThreadDispatcher dispatcher;

    [Header("Firebase")]
    [SerializeField] string databaseUrl = "https://sarang-dbcae-default-rtdb.asia-southeast1.firebasedatabase.app/";

    [Header("UI")]
    [SerializeField] InputField NickNameInput;
    [SerializeField] Text CheckText;

    [Header("Scene")]
    [SerializeField] string NextSceneName = "MainScene";
    [SerializeField] bool LoadNextSceneAfterRegister = false;

    void Start()
    {
        database = FirebaseDatabase.GetInstance(databaseUrl);
        reference = database.RootReference;
        dispatcher = UnityMainThreadDispatcher.Instance();
    }

    // 회원가입 버튼에 연결
    public void OnClickRegister()
    {
        string nickName = NickNameInput.text.Trim();

        // 닉네임 안 입력했으면 가입 막기
        if (string.IsNullOrEmpty(nickName))
        {
            CheckText.text = "닉네임을 입력하세요.";
            return;
        }

        CheckDuplicateNickName(nickName);
    }

    // Firebase에서 같은 닉네임 있는지 먼저 확인
    void CheckDuplicateNickName(string nickName)
    {
        reference
            .Child("UserInfo")
            .OrderByChild("NickName")
            .EqualTo(nickName)
            .GetValueAsync()
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    dispatcher.Enqueue(() =>
                    {
                        CheckText.text = "Firebase 읽기 오류";
                    });
                    return;
                }

                DataSnapshot snapshot = task.Result;

                // 이미 같은 닉네임 존재하면 가입 거절
                if (snapshot.HasChildren)
                {
                    dispatcher.Enqueue(() =>
                    {
                        CheckText.text = "이미 사용 중인 닉네임입니다.";
                    });
                    return;
                }

                // 중복 없으면 유저 생성
                CreateUser(nickName);
            });
    }

    // Firebase에 새 유저 데이터 저장
    void CreateUser(string nickName)
    {
        DatabaseReference newUserRef = reference.Child("UserInfo").Push();
        string userKey = newUserRef.Key;

        // UserData 생성자에서 Sword/Shield/Elixir 0개로 초기화됨
        UserData userData = new UserData(nickName);
        string json = JsonUtility.ToJson(userData);

        newUserRef.SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                dispatcher.Enqueue(() =>
                {
                    CheckText.text = "회원가입 실패";
                });
                return;
            }

            dispatcher.Enqueue(() =>
            {
                // 내 키랑 닉네임 로컬에 저장
                PlayerPrefs.SetString("UserKey", userKey);
                PlayerPrefs.SetString("UserNickName", nickName);
                PlayerPrefs.Save();

                CheckText.text = "회원가입 완료";

                if (LoadNextSceneAfterRegister)
                {
                    SceneManager.LoadScene(NextSceneName);
                }
            });
        });
    }
}