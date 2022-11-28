using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using DG.Tweening;
using Arbor;
using Arbor.BehaviourTree; //For BehaviorTree
using Fungus;
using UniRx;

/*
 * SlimeMachine
 *・HP管理、ダメージ処理
 *・shot処理呼び出し元
 *・出現演出呼び出し元
 *・撃破時のイベント呼び出し
 */

public class SlimeMachine : MonoBehaviour
{
    [SerializeField] private Flowchart eventFlowchart;
    [SerializeField] private string SlimeMachine_ButtleEndMessage;
    private int initialHP;
    private BehaviourTree behaviortree;
    private ParameterContainer parameter;
    private SpriteRenderer slimemachine_spr;
    private DeathAction_SlimeMachine deathAction;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject homingbulletPrefab;
    [SerializeField] private Transform shotpoint;

    // Start is called before the first frame update
    void Start()
    {
        behaviortree = GetComponent<BehaviourTree>();
        parameter = GetComponent<ParameterContainer>();
        slimemachine_spr = GetComponent<SpriteRenderer>();
        deathAction = GetComponent<DeathAction_SlimeMachine>();
        //初期化
        initialHP = parameter.GetInt("HP", 0);
        slimemachine_spr.color = new Color(255,255,255,0);
        behaviortree.enabled = false;
        this.gameObject.SetActive(false);
        deathAction.ButtleFin.Subscribe(_ =>
        {
            eventFlowchart.SendFungusMessage(SlimeMachine_ButtleEndMessage);
        }); //バトル終了時会話イベントを呼び出す
    }

    public void SlimeMachine_shot() //Arbor側で呼び出し
    {
        Instantiate(bulletPrefab, shotpoint.position, shotpoint.rotation);
    }

    public void SlimeMachine_homingshot() //Arbor側で呼び出し
    {
        Instantiate(homingbulletPrefab, shotpoint.position, shotpoint.rotation);
    }

    //出現演出（Fungusで呼び出し。Arbor側でするのもあり？）
    public void BossAppearance()
    {
        this.gameObject.SetActive(true); //Fungus内で呼ばれるのでほとんど意味ない
        //スプライトのアルファ値を１にする
        slimemachine_spr.DOFade(1, 2).SetLink(gameObject);
    }

    //AI起動
    public void ButtleStart()
    {
        behaviortree.enabled = true;
    }

    //退場演出(Arbor側で呼び出し)
    public void BossFadeOut()
    {
        slimemachine_spr.DOFade(0, 2).SetLink(gameObject).OnComplete(() =>
        {
            this.gameObject.SetActive(false);
        });
    }

    private void OnCollisionEnter2D(UnityEngine.Collision2D collision) 
    {
        if (collision.gameObject.CompareTag("PlayerBullet")) //ダメージ処理
        {
            parameter.SetInt("HP",initialHP--); //HP管理
            Destroy(collision.gameObject); //あたった弾丸を消去
        }
    }

    
}
