using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchmakingScene : MonoBehaviour
{
    DBManager DB;

    // Start is called before the first frame update
    void Start()
    {
        DB = FindObjectOfType<DBManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (DB.isPlayersReady)
        {
            DB.isPlayersReady = false;
            SceneManager.LoadScene("GameScene");
        }
    }

    public void ReadyButtonClick()
    {
        DB.SetPlayerReady();
    }
}
