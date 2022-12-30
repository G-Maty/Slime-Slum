using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using DG.Tweening;
using Arbor;
using Arbor.BehaviourTree; //For BehaviorTree
using Fungus;
using UniRx;
using Unity.VisualScripting;
using TMPro;
using System.Diagnostics.Tracing;
using UnityEngine.UI;

/*
 * SlimeMachine
 *・HP管理、ダメージ処理
 *・shot処理呼び出し元
 *・出現演出呼び出し元
 *・撃破時のイベント呼び出し
 *・SE
 *・UI管理
 */

public class SlimeMachine : MonoBehaviour
{
    //Fungus関係
    [SerializeField] private Flowchart eventFlowchart;
    [SerializeField] private string SlimeMachine_ButtleEndMessage;
    [SerializeField] private string SlimeMachine_TimeUpMessage;

    public bool IsBossBreak { get; set; } //撃破フラグ
    private bool isDamage = false;
    private GameManager gameManager;
    private int maxHP;
    private int HP;
    private BehaviourTree behaviortree;
    private ParameterContainer parameter;
    private SpriteRenderer slimemachine_spr;
    private DeathAction_SlimeMachine deathAction;
    [SerializeField] private GameObject bulletPrefab; //弾丸(プレハブ)
    [SerializeField] private GameObject homingbulletPrefab; //追跡弾丸(プレハブ)
    [SerializeField] private Transform shotpoint;
    [SerializeField] private int recoveryHP; //HP回復
    [SerializeField] private int ButtleTimerSet; //制限時間

    //Audio関係
    private AudioSource audioSource;
    [SerializeField] private AudioClip shotSE;
    [SerializeField] private AudioClip damagedSE;
    [SerializeField] private AudioClip explosionSE;

    //UI関係は子オブジェクトに設定
    [SerializeField] private TextMeshProUGUI countTimer; //タイムテキスト
    [SerializeField] private Slider HPslider; //HPゲージ


    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        behaviortree = GetComponent<BehaviourTree>();
        parameter = GetComponent<ParameterContainer>();
        slimemachine_spr = GetComponent<SpriteRenderer>();
        deathAction = GetComponent<DeathAction_SlimeMachine>(); //ArborのdeathAction
        audioSource = GetComponent<AudioSource>();
        //初期化
        maxHP = parameter.GetInt("HP", 0);
        HP = maxHP;
        HPslider.maxValue = maxHP;
        HPslider.value = maxHP;
        countTimer.text = ButtleTimerSet.ToString();
        slimemachine_spr.color = new Color(255,255,255,0);
        behaviortree.enabled = false;
        this.gameObject.SetActive(false);

        //Playerがやられたときの処理
        gameManager.playerDeath_observable.Subscribe(_ =>
        {
            HP = HP + recoveryHP;
            if(HP > maxHP)
            {
                HP = maxHP;
            }
            parameter.SetInt("HP", HP); //HP回復
            HPslider.value = parameter.GetInt("HP");
        }).AddTo(this);

        deathAction.ButtleFin.Subscribe(_ =>
        {
            eventFlowchart.SendFungusMessage(SlimeMachine_ButtleEndMessage);
        }).AddTo(this); //撃破演出完了時会話イベントを呼び出す

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

    //制限時間スタート
    public void ButtleTimerStart()
    {
        StartCoroutine(ButtleTimer());
    }

    //AI起動
    public void ButtleStart()
    {
        behaviortree.enabled = true;
        ButtleTimerStart(); //カウントダウンスタート
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
            StartCoroutine(DamageEff());
            parameter.SetInt("HP",HP--); //HP管理
            HPslider.value = parameter.GetInt("HP");
            SEplayOneShot("damage"); //SE
            Destroy(collision.gameObject); //あたった弾丸を消去
        }
    }

    //タイムオーバー時の初期化(Fungus仕様)
    private void BossInitialization()
    {
        BossFadeOut();
        HP = maxHP;
        behaviortree.enabled = false;
        HPslider.value = maxHP;
        countTimer.text = ButtleTimerSet.ToString();
        //ボスイベントトリガーを復活
        eventFlowchart.SendFungusMessage(SlimeMachine_TimeUpMessage);
    }

    //制限時間処理
    IEnumerator ButtleTimer()
    {
        for (int i = ButtleTimerSet; i > -1; i--)
        {
            if (IsBossBreak)
            {
                yield break;
            }
            yield return new WaitForSeconds(1);
            countTimer.color = Color.white; //初期化のため
            countTimer.text = i.ToString();
            if (i < 11)
            {
                countTimer.color = Color.red;
            }
        }
        BossInitialization(); //時間切れの処理
    }

    IEnumerator DamageEff()
    {
        if (isDamage)
        {
            yield break;
        }
        isDamage = true;
        for(int i = 0; i < 3; i++)
        {
            slimemachine_spr.enabled = false;
            yield return new WaitForSeconds(0.05f);
            slimemachine_spr.enabled = true;
            yield return new WaitForSeconds(0.05f);
        }
        isDamage = false;
    }

    public void SEplayOneShot(string str)
    {
        switch (str)
        {
            case "shot":
                audioSource.PlayOneShot(shotSE);
                break;
            case "damage":
                audioSource.PlayOneShot(damagedSE);
                break;
            case "explode":
                audioSource.PlayOneShot(explosionSE);
                break;
        }
    }
}
