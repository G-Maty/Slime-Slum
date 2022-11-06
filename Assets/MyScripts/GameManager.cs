using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public GameObject PlayerPref;
    private GameObject OperatingPlayer;
    private Player_Move player_move;
    private Stack<Transform> RespawnPointStack = new Stack<Transform>();
    private CinemachineVirtualCamera RespawnPointVC = null;
    private bool iswarp = false;

    private CinemachineBrain cmBrain;

    /*
     * GameManager
     * 主にゲーム進行に関するもの
     * チェックポイント処理
     */

    // Start is called before the first frame update
    void Start()
    {
        OperatingPlayer = GameObject.FindGameObjectWithTag("Player");
        cmBrain = Camera.main.GetComponent<CinemachineBrain>();
        player_move = OperatingPlayer.GetComponent<Player_Move>();
        player_move.checkPoint_Update = checkPoint_Update;
        player_move.warpCheckpoint = warpCheckpoint; //関数を登録(デリゲート)
    }

    // Update is called once per frame
    void Update()
    {
        if (iswarp) //ワープ時新規プレイヤーの情報を取得
        {
            iswarp = false;
            OperatingPlayer = GameObject.FindGameObjectWithTag("Player");
            player_move = OperatingPlayer.GetComponent<Player_Move>();
            player_move.checkPoint_Update = checkPoint_Update;
            player_move.warpCheckpoint = warpCheckpoint; //関数を登録(デリゲート)
        }
    }

    public void checkPoint_Update(GameObject checkpoint) //チェックポイントのアップデート処理
    {
        checkpoint.transform.GetChild(0).gameObject.SetActive(true); //最新ポイントの点火
        if (RespawnPointStack.Count != 0 && checkpoint != RespawnPointStack.Peek().gameObject)
        {
            RespawnPointStack.Peek().GetChild(0).gameObject.SetActive(false); //古いポイントの消火
        }
        RespawnPointStack.Push(checkpoint.transform); //チェックポイントの位置をスタックへプッシュ
        CinemachineVirtualCamera current = cmBrain.ActiveVirtualCamera as CinemachineVirtualCamera; //キャストも必要
        RespawnPointVC = current;　//リスポーン地点のVCを記憶
        //Debug.Log("チェックポイント更新");
    }

    public void warpCheckpoint() //古いプレイヤーを除去し、新規プレイヤーを投入
    {
        iswarp = true;
        Vector2 warppoint = new Vector2(RespawnPointStack.Peek().position.x, RespawnPointStack.Peek().position.y + 0.3f);

        CinemachineVirtualCamera current = cmBrain.ActiveVirtualCamera as CinemachineVirtualCamera; //キャストも必要
        current.Priority = 10;
        RespawnPointVC.Priority = 100; //リスポーン地点のVCを有効化
        Destroy(OperatingPlayer);
        Instantiate(PlayerPref,warppoint,transform.rotation); //位置はスタックからポップ
        //Debug.Log("チェックポイントへ");
    }
}
