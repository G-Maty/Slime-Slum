using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UniRx;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    //チェックポイント関係
    public GameObject PlayerPref;
    private GameObject OperatingPlayer;
    private Player_Move player_move;
    //private Stack<Transform> RespawnPointStack = new Stack<Transform>();
    private Transform RespawnPoint_memory;
    private CinemachineVirtualCamera RespawnPointVC = null;
    private bool iswarp = false;

    private CinemachineBrain cmBrain;

    //残弾数管理関係
    [SerializeField] private int MaxBullets = 10; //最大残弾数
    [SerializeField] private int RemainingBullets = 0; //残弾数(プレイヤー更新でも失われない)


    /*
     * GameManager
     * 主にゲーム進行に関するもの
     * チェックポイント処理
     * プレイヤーの残弾数管理(プレイヤーが交代したら新しいオブジェクトになるため)
     */

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        OperatingPlayer = GameObject.FindGameObjectWithTag("Player"); //プレイヤーを取得
        cmBrain = Camera.main.GetComponent<CinemachineBrain>();
        player_move = OperatingPlayer.GetComponent<Player_Move>();
        player_move.checkPoint_Update = checkPoint_Update; //関数を登録(デリゲート)
        player_move.warpCheckpoint = warpCheckpoint; //関数を登録(デリゲート)

        player_move.remainingBullets = 0; //回復地点をふまない限り残弾補充なし

        shotNotification_Subscribe(); //発射通知の購読
        
    }

    // Update is called once per frame
    void Update()
    {
        player_Update(); //チェックポイントへワープ時新規プレイヤーの情報を取得


    }

    public void checkPoint_Update(GameObject checkpoint) //チェックポイントのアップデート処理
    {
        checkpoint.transform.GetChild(0).gameObject.SetActive(true); //最新ポイントの点火
        if(RespawnPoint_memory != null && checkpoint != RespawnPoint_memory.gameObject)
        {
            RespawnPoint_memory.GetChild(0).gameObject.SetActive(false); //古いポイントの消火
        }
        RespawnPoint_memory = checkpoint.transform; //チェックポイントの位置を記憶
        CinemachineVirtualCamera current = cmBrain.ActiveVirtualCamera as CinemachineVirtualCamera; //キャストも必要
        RespawnPointVC = current;　//リスポーン地点のVCを記憶
    }

    public void warpCheckpoint() //古いプレイヤーを除去し、新規プレイヤーを投入
    {
        iswarp = true;
        Vector2 warppoint = new Vector2(RespawnPoint_memory.position.x, RespawnPoint_memory.position.y + 0.3f);

        CinemachineVirtualCamera current = cmBrain.ActiveVirtualCamera as CinemachineVirtualCamera; //キャストも必要
        current.Priority = 10;
        RespawnPointVC.Priority = 100; //リスポーン地点のVCを有効化
        Destroy(OperatingPlayer);
        Instantiate(PlayerPref,warppoint,transform.rotation); //位置はスタックからポップ
    }

    private void player_Update()
    {
        if (iswarp) //チェックポイントへワープ時新規プレイヤーの情報を取得
        {
            iswarp = false;
            OperatingPlayer = GameObject.FindGameObjectWithTag("Player");
            player_move = OperatingPlayer.GetComponent<Player_Move>();
            player_move.checkPoint_Update = checkPoint_Update;
            player_move.warpCheckpoint = warpCheckpoint; //関数を登録(デリゲート)

            player_move.remainingBullets = RemainingBullets; //現在の残弾数を引き継ぎ

            shotNotification_Subscribe();
        }
    }

    private void shotNotification_Subscribe()
    {
        //購読(発射通知の受け取り)
        player_move.shot_observable.Subscribe(
            x =>
            {
                RemainingBullets = RemainingBullets - x;
                player_move.remainingBullets = RemainingBullets;
                Debug.Log("残弾：" + RemainingBullets + "発");
            }).AddTo(this);
        //購読（残弾補充通知の受け取り）
        player_move.recovery_observable.Subscribe(
            _ =>
            {
                RemainingBullets = MaxBullets;
                player_move.remainingBullets = RemainingBullets;
                Debug.Log("残弾補充");
            }).AddTo(this);
    }
}
