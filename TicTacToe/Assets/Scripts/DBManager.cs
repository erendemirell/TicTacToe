using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System.Collections.Generic;
using UnityEngine;

public class DBManager : MonoBehaviour
{
    public bool isPlayersReady;
    public bool isMyTurn = true;
    public bool updateScreen = false;

    User user;
    RoomInfo roomInfo;

    DatabaseReference matchmaking;
    DatabaseReference rooms;

    private void Awake()
    {
        int dbCount = FindObjectsOfType<DBManager>().Length;

        if (dbCount > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("...");//write your database name

        rooms = FirebaseDatabase.DefaultInstance.GetReference("rooms");
        matchmaking = FirebaseDatabase.DefaultInstance.GetReference("matchmaking");

        matchmaking.ChildAdded += Matchmaking_ChildAdded;
    }

    private void Matchmaking_ChildAdded(object sender, ChildChangedEventArgs e)
    {
        if (matchmaking != null)
        {
            matchmaking.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    print("Database Error");
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot data = task.Result;

                    if ((int)data.ChildrenCount % 2 == 0 && data.ChildrenCount != 0)
                    {
                        DataSnapshot players = matchmaking.LimitToFirst(2).GetValueAsync().Result;
                        string playersJson = players.GetRawJsonValue();
                        IEnumerable<DataSnapshot> children = players.Children;
                        string[] playerIDs = new string[2];
                        int i = 0;
                        foreach (var child in children)
                        {
                            playerIDs[i] = (string)child.Child("name").Value;
                            i++;
                        }

                        DatabaseReference roomID = rooms.Push();
                        string roomInfoJson = GetRoomInfo(roomID.Key, playerIDs);
                        roomID.SetRawJsonValueAsync(roomInfoJson);

                        matchmaking.RemoveValueAsync();
                    }
                }
            });
        }
    }

    private string GetRoomInfo(string gameId, string[] playerIDs)
    {
        roomInfo = new RoomInfo()
        {
            roomId = gameId,
            playerIds = playerIDs,
            holes = new string[9],
            turn = playerIDs[0],
            gameStatus = "continues"
        };

        string roomInfoJson = JsonUtility.ToJson(roomInfo);
        return roomInfoJson;
    }

    public void GetPlayerName(string playerName)
    {
        user = new User()
        {
            name = playerName
        };

        string userJson = JsonUtility.ToJson(user);

        matchmaking.Push().SetRawJsonValueAsync(userJson);
    }

    private void Rooms_ValueChanged(object sender, ValueChangedEventArgs args)
    {
        DataSnapshot addedGame = args.Snapshot;

        if (addedGame.GetRawJsonValue() != null && roomInfo.roomId != null)
        {
            if (addedGame.Child(roomInfo.roomId).HasChild("ready"))
            {
                if (addedGame.Child(roomInfo.roomId).Child("ready").ChildrenCount % 2 == 0)
                {
                    isPlayersReady = true;
                    rooms.ValueChanged -= Rooms_ValueChanged;
                }
            }
        }
    }

    public void SetPlayerReady()
    {
        rooms.Child(roomInfo.roomId).Child("ready").Child(user.name).SetValueAsync(true);
        rooms.ValueChanged += Rooms_ValueChanged;
    }

    public void StartTurnListener()
    {
        rooms.Child(roomInfo.roomId).Child("turn").ValueChanged += Turn_ValueChanged;
    }

    private void Turn_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        GetWhoseTurn();

        DataSnapshot snapshot = e.Snapshot;

        roomInfo.turn = (string)snapshot.Value;
    }

    public int GetXOSprite()
    {
        if (user.name == roomInfo.playerIds[0])
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }

    public void ChangeTurn()
    {
        if (user.name == roomInfo.playerIds[0])
        {
            rooms.Child(roomInfo.roomId).Child("turn").SetValueAsync(roomInfo.playerIds[1]);
        }
        else
        {
            rooms.Child(roomInfo.roomId).Child("turn").SetValueAsync(roomInfo.playerIds[0]);
        }

        StartTurnListener();
    }

    public void GetWhoseTurn()
    {
        rooms.Child(roomInfo.roomId).Child("turn").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                print("Database Error");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if ((string)snapshot.Value == user.name)
                {
                    isMyTurn = true;
                }
                else
                {
                    isMyTurn = false;
                }
            }
        });
    }

    public void SetDBHoles(int index, int isX)
    {
        rooms.Child(roomInfo.roomId).Child("holes").Child(index.ToString()).SetValueAsync(isX == 0 ? "x" : "o");
        StartDBHolesListener();
    }

    private void StartDBHolesListener()
    {
        rooms.Child(roomInfo.roomId).Child("holes").ValueChanged += DBholes_ValueChanged;
    }

    private void StopDBHolesListener()
    {
        rooms.Child(roomInfo.roomId).Child("holes").ValueChanged -= DBholes_ValueChanged;
    }

    private void DBholes_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        Dictionary<string, object> dbHoles = new Dictionary<string, object>();

        DataSnapshot snapshot = e.Snapshot;

        foreach (var child in snapshot.Children)
        {
            dbHoles.Add(child.Key, child.Value);
        }

        FindObjectOfType<GameManager>().UpdateHoles(dbHoles);
    }

    public void FinishGame(string gameStatus)
    {
        rooms.Child(roomInfo.roomId).Child("gameStatus").SetValueAsync(gameStatus);
        rooms.Child(roomInfo.roomId).Child("gameStatus").ValueChanged += GameStatus_ValueChanged;
    }

    private void GameStatus_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        DataSnapshot snapshot = e.Snapshot;

        if ((string)snapshot.Value == "finished")
        {
            StopDBHolesListener();
            GetWhoWinGame();
        }
    }

    private void GetWhoWinGame()
    {
        string winText;

        if (roomInfo.turn == user.name)
        {
            winText = "You Lost";
        }
        else
        {
            winText = "You Win";
        }

        FindObjectOfType<GameManager>().WriteGameStatusText(winText);
    }
}