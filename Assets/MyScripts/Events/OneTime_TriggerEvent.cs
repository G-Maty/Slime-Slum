using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;

/*
 * 一度きりのイベント発生トリガー
 * イベントが発生すると自身のアクティブをFalseにすることで
 * 二度と呼ばれないようにする
 */

public class OneTime_TriggerEvent : MonoBehaviour
{
    [SerializeField]
    private Flowchart eventFlowchart = null;
    [SerializeField]
    private string sendMessage = "";
    private GameManager gameManager;
    private bool isTalking = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(Talk());
        }
    }

    IEnumerator Talk()
    {
        if (isTalking) //すでに会話イベント中ならbreak
        {
            yield break;
        }
        //会話中のプレイヤーの動きを制限するため
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        isTalking = true;
        gameManager.Restrict_PlayerMove(); //Player硬直
        //player_move.Freeze_player(); //Player硬直
        //player_move.enabled = false; //移動を制限
        //player_move.Unzip_player(); //Player解凍（以前の重力等を引き継ぎ）


        eventFlowchart.SendFungusMessage(sendMessage); //フローチャートにメッセージを送信して特定のイベント（ブロック）開始
        yield return new WaitUntil(() => eventFlowchart.GetExecutingBlocks().Count == 0); //イベント（ブロック）が終了するまで待つ

        isTalking = false;
        this.gameObject.SetActive(false); //イベント発生を防ぐ
        gameManager.Unrestrict_PlayerMove(); //Player解凍
        //player_move.enabled = true; //移動の制限解除
    }
}
