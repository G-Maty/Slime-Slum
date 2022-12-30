using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;

/*
 * 一度以上のイベント発生トリガー
 * 任意のボタンを押すことで
 * イベントが何回でも呼び出し可能
 * 
 */

public class ManyTime_TriggerEvent : MonoBehaviour
{
    [SerializeField]
    private Flowchart eventFlowchart = null;
    [SerializeField]
    private string sendMessage = "";
    [SerializeField]
    private string keycode = "";
    [SerializeField]
    private GameObject miniUI = null;
    private GameManager gameManager;
    private bool triggerflg = false; //trueなら指定のキーでイベント開始
    private bool isTalking = false;

    // Start is called before the first frame update
    void Start()
    {
        miniUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (triggerflg && !isTalking)
        {
            if (Input.GetKeyDown(keycode))
            {
                StartCoroutine(Talk());
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            triggerflg = true;
            if(miniUI != null)
            {
                miniUI.SetActive(true);
            }

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            triggerflg = false;
            if (miniUI != null)
            {
                miniUI.SetActive(false);
            }
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

        eventFlowchart.SendFungusMessage(sendMessage); //フローチャートにメッセージを送信して特定のイベント（ブロック）開始
        yield return new WaitUntil(() => eventFlowchart.GetExecutingBlocks().Count == 0); //イベント（ブロック）が終了するまで待つ

        isTalking = false;
        gameManager.Unrestrict_PlayerMove(); //Player解凍
    }
}