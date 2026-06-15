using Firebase.Database;
using PimDeWitte.UnityMainThreadDispatcher;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UserLogin : MonoBehaviour
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
    [SerializeField] bool LoadNextSceneAfterLogin = false;

    void Start()
    {
        database = FirebaseDatabase.GetInstance(databaseUrl);
        reference = database.RootReference;
        dispatcher = UnityMainThreadDispatcher.Instance();
    }

    // 로그인 버튼에 연결
    public void OnClickLogin()
    {
        string nickName = NickNameInput.text.Trim();

        // 닉네임 안 입력했으면 로그인 막기
        if (string.IsNullOrEmpty(nickName))
        {
            CheckText.text = "닉네임을 입력하세요.";
            return;
        }

        Login(nickName);
    }

    // Firebase에서 닉네임으로 유저 찾기
    void Login(string nickName)
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

                // 닉네임이 Firebase에 없으면 로그인 실패
                if (!snapshot.HasChildren)
                {
                    dispatcher.Enqueue(() =>
                    {
                        CheckText.text = "존재하지 않는 닉네임입니다.";
                    });
                    return;
                }

                // 유저 찾으면 UserKey 저장하고 다음 씬으로
                foreach (DataSnapshot userSnapshot in snapshot.Children)
                {
                    string userKey = userSnapshot.Key;

                    dispatcher.Enqueue(() =>
                    {
                        PlayerPrefs.SetString("UserKey", userKey);
                        PlayerPrefs.SetString("UserNickName", nickName);
                        PlayerPrefs.Save();

                        CheckText.text = "로그인 성공";

                        if (LoadNextSceneAfterLogin)
                        {
                            SceneManager.LoadScene(NextSceneName);
                        }
                    });

                    break;
                }
            });
    }
}