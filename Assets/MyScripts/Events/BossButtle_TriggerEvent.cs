using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;
using DG.Tweening;

/*
 * 一度以上のイベント発生トリガー
 * 任意のボタンを押すことで
 * イベントが何回でも呼び出し可能
 * 
 */

public class BossButtle_TriggerEvent : MonoBehaviour
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
    [SerializeField] private SpriteRenderer buttleTrigger_renderer;
    [SerializeField] private SpriteRenderer particleUp_renderer;
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
                miniUI.SetActive(false);
                buttleTrigger_renderer.DOFade(0, 1f).SetLink(gameObject); //透明化
                particleUp_renderer.DOFade(0, 1f).SetLink(gameObject); //透明化
                StartCoroutine(Talk()); //会話イベント開始
            }
        }
    }

    //トリガーを再表示、Fungusコマンドから呼び出し
    public void initializationTrigger()
    {
        buttleTrigger_renderer.DOFade(1, 1f).SetLink(gameObject); //表示
        particleUp_renderer.DOFade(1, 1f).SetLink(gameObject); //表示
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            triggerflg = true;
            if (miniUI != null)
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
        gameManager.Restrict_PlayerMove(); //Playerの移動制限

        eventFlowchart.SendFungusMessage(sendMessage); //フローチャートにメッセージを送信して特定のイベント（ブロック）開始
        yield return new WaitUntil(() => eventFlowchart.GetExecutingBlocks().Count == 0); //イベント（ブロック）が終了するまで待つ
        isTalking = false;
        gameManager.Unrestrict_PlayerMove(); //Playerの解凍
        this.gameObject.SetActive(false);
    }
}
