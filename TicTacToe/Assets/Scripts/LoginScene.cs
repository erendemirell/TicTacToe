using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginScene : MonoBehaviour
{
    public InputField playerName;    

    DBManager DB;

    // Start is called before the first frame update
    void Start()
    {
        DB = FindObjectOfType<DBManager>();
    }

    public void PlayButtonClick()
    {
        DB.GetPlayerName(playerName.text);
        SceneManager.LoadScene("MatchmakingScene");
    }
}
