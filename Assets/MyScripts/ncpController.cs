using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;

[RequireComponent(typeof(Flowchart))]
public class ncpController : MonoBehaviour
{

    [SerializeField]
    private string message = "";

    private bool isTalking = false;
    private Player_Move player_move;
    private Flowchart flowChart;

    void Start()
    {
        flowChart = GetComponent<Flowchart>();
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
        player_move = GameObject.FindGameObjectWithTag("Player").GetComponent<Player_Move>();

        isTalking = true;
        player_move.Freeze_player(); //Player硬直
        player_move.enabled = false; //移動を制限

        flowChart.SendFungusMessage(message); //フローチャートにメッセージを送信して特定のイベント（ブロック）開始
        yield return new WaitUntil(() => flowChart.GetExecutingBlocks().Count == 0); //イベント（ブロック）が終了するまで待つ

        isTalking = false; 
        player_move.enabled = true; //移動の制限解除
        player_move.Unzip_player(); //Player解凍（以前の重力等を引き継ぎ）
    }
}