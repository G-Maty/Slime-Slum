using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UniRx;
using System;
using UnityEngine.UI;
using DG.Tweening;

/*
 * GameManager
 * 主にゲーム進行に関するもの
 * チェックポイント処理
 * プレイヤーの残弾数管理(プレイヤーが交代したら新しいオブジェクトになるため)
 * プレイヤーUI
 */

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
    //private Image baseImage; //UI
    private Image bulletGauge; //UI


    //プレイヤーリスポーン通知（ボスHP回復とか用）
    private Subject<Unit> _playerDeath = new Subject<Unit>();
    public IObservable<Unit> playerDeath_observable => _playerDeath;


    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        OperatingPlayer = GameObject.FindGameObjectWithTag("Player"); //プレイヤーを取得
        cmBrain = Camera.main.GetComponent<CinemachineBrain>();
        player_move = OperatingPlayer.GetComponent<Player_Move>();
        //baseImage = OperatingPlayer.transform.Find("BulletMGCanvas/BaseImage").GetComponent<Image>(); //UIを取得
        bulletGauge = OperatingPlayer.transform.Find("BulletMGCanvas/BulletGauge").GetComponent<Image>();
        checkpointUpdate_Subscribe();
        warpCheckPoint_Subscribe(); //ワープ通知の購読

        player_move.remainingBullets = 0; //序盤は回復地点をふまない限り残弾補充なし
        bulletGauge.fillAmount = 0;

        shotNotification_Subscribe(); //発射通知の購読
        
    }

    // Update is called once per frame
    void Update()
    {
        player_Update(); //チェックポイントへワープ時新規プレイヤーの情報を取得
    }

    private void player_Update()
    {
        if (iswarp) //チェックポイントへワープ時新規プレイヤーの情報を取得
        {
            iswarp = false;
            OperatingPlayer = GameObject.FindGameObjectWithTag("Player");
            player_move = OperatingPlayer.GetComponent<Player_Move>();
            //player_move.checkPoint_Update = checkPoint_Update;
            //baseImage = OperatingPlayer.transform.Find("BulletMGCanvas/BaseImage").GetComponent<Image>(); //UIを取得
            bulletGauge = OperatingPlayer.transform.Find("BulletMGCanvas/BulletGauge").GetComponent<Image>();
            checkpointUpdate_Subscribe();
            warpCheckPoint_Subscribe();

            player_move.remainingBullets = RemainingBullets; //現在の残弾数を引き継ぎ

            shotNotification_Subscribe();
        }
    }

    private void checkpointUpdate_Subscribe() //チェックポイントのアップデート処理
    {
        player_move.checkpointUpdate_observabele.Subscribe(
            checkpoint =>
            {
                checkpoint.transform.GetChild(0).gameObject.SetActive(true); //最新ポイントの点火
                if (RespawnPoint_memory != null && checkpoint != RespawnPoint_memory.gameObject)
                {
                    RespawnPoint_memory.GetChild(0).gameObject.SetActive(false); //古いポイントの消火
                }
                RespawnPoint_memory = checkpoint.transform; //チェックポイントの位置を記憶
                CinemachineVirtualCamera current = cmBrain.ActiveVirtualCamera as CinemachineVirtualCamera; //キャストも必要
                RespawnPointVC = current;　//リスポーン地点のVCを記憶
            }
            ).AddTo(this);
    }

    private void warpCheckPoint_Subscribe() //古いプレイヤーを除去し、新規プレイヤーを投入
    {
        player_move.warpCheckPoint_observable.Subscribe(
            _ =>
            {
                iswarp = true;
                Vector2 warppoint = new Vector2(RespawnPoint_memory.position.x, RespawnPoint_memory.position.y + 0.3f);
                CinemachineVirtualCamera current = cmBrain.ActiveVirtualCamera as CinemachineVirtualCamera; //キャストも必要
                _playerDeath.OnNext(Unit.Default);
                current.Priority = 10; //0でよくね
                RespawnPointVC.Priority = 100; //リスポーン地点のVCを有効化
                Destroy(OperatingPlayer);
                Instantiate(PlayerPref, warppoint, transform.rotation); //位置はスタックからポップ
            }
            ).AddTo(this);
    }

    private void shotNotification_Subscribe()
    {
        
        //購読(発射通知の受け取り)
        player_move.shot_observable.Subscribe(
            x =>
            {
                RemainingBullets = RemainingBullets - x;
                player_move.remainingBullets = RemainingBullets;
                bulletGauge.fillAmount = (float)RemainingBullets / (float)MaxBullets;
                //Debug.Log("残弾：" + RemainingBullets + "発");
            }).AddTo(this);
        //購読（残弾補充通知の受け取り）
        player_move.recovery_observable.Subscribe(
            _ =>
            {
                RemainingBullets = MaxBullets;
                player_move.remainingBullets = RemainingBullets;
                //残弾UI : アニメつけておしゃれにした
                bulletGauge.DOFillAmount(MaxBullets / MaxBullets,0.5f).SetLink(gameObject);
                //bulletGauge.fillAmount = MaxBullets / MaxBullets;
                //Debug.Log("残弾補充");
            }).AddTo(this);
    }
}
