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
 *�EHP�Ǘ��A�_���[�W����
 *�Eshot�����Ăяo����
 *�E�o�����o�Ăяo����
 *�E���j���̃C�x���g�Ăяo��
 *�ESE
 *�EUI�Ǘ�
 */

public class SlimeMachine : MonoBehaviour
{
    //Fungus�֌W
    [SerializeField] private Flowchart eventFlowchart;
    [SerializeField] private string SlimeMachine_ButtleEndMessage;
    [SerializeField] private string SlimeMachine_TimeUpMessage;

    public bool IsBossBreak { get; set; } //���j�t���O
    private bool isDamage = false;
    private GameManager gameManager;
    private int maxHP;
    private int HP;
    private BehaviourTree behaviortree;
    private ParameterContainer parameter;
    private SpriteRenderer slimemachine_spr;
    private DeathAction_SlimeMachine deathAction;
    [SerializeField] private GameObject bulletPrefab; //�e��(�v���n�u)
    [SerializeField] private GameObject homingbulletPrefab; //�ǐՒe��(�v���n�u)
    [SerializeField] private Transform shotpoint;
    [SerializeField] private int recoveryHP; //HP��
    [SerializeField] private int ButtleTimerSet; //��������

    //Audio�֌W
    private AudioSource audioSource;
    [SerializeField] private AudioClip shotSE;
    [SerializeField] private AudioClip damagedSE;
    [SerializeField] private AudioClip explosionSE;

    //UI�֌W�͎q�I�u�W�F�N�g�ɐݒ�
    [SerializeField] private TextMeshProUGUI countTimer; //�^�C���e�L�X�g
    [SerializeField] private Slider HPslider; //HP�Q�[�W


    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        behaviortree = GetComponent<BehaviourTree>();
        parameter = GetComponent<ParameterContainer>();
        slimemachine_spr = GetComponent<SpriteRenderer>();
        deathAction = GetComponent<DeathAction_SlimeMachine>(); //Arbor��deathAction
        audioSource = GetComponent<AudioSource>();
        //������
        maxHP = parameter.GetInt("HP", 0);
        HP = maxHP;
        HPslider.maxValue = maxHP;
        HPslider.value = maxHP;
        countTimer.text = ButtleTimerSet.ToString();
        slimemachine_spr.color = new Color(255,255,255,0);
        behaviortree.enabled = false;
        this.gameObject.SetActive(false);

        //Player�����ꂽ�Ƃ��̏���
        gameManager.playerDeath_observable.Subscribe(_ =>
        {
            HP = HP + recoveryHP;
            if(HP > maxHP)
            {
                HP = maxHP;
            }
            parameter.SetInt("HP", HP); //HP��
            HPslider.value = parameter.GetInt("HP");
        }).AddTo(this);

        deathAction.ButtleFin.Subscribe(_ =>
        {
            eventFlowchart.SendFungusMessage(SlimeMachine_ButtleEndMessage);
        }).AddTo(this); //���j���o��������b�C�x���g���Ăяo��

    }


    public void SlimeMachine_shot() //Arbor���ŌĂяo��
    {
        Instantiate(bulletPrefab, shotpoint.position, shotpoint.rotation);
    }

    public void SlimeMachine_homingshot() //Arbor���ŌĂяo��
    {
        Instantiate(homingbulletPrefab, shotpoint.position, shotpoint.rotation);
    }

    //�o�����o�iFungus�ŌĂяo���BArbor���ł���̂�����H�j
    public void BossAppearance()
    {
        this.gameObject.SetActive(true); //Fungus���ŌĂ΂��̂łقƂ�ǈӖ��Ȃ�
        //�X�v���C�g�̃A���t�@�l���P�ɂ���
        slimemachine_spr.DOFade(1, 2).SetLink(gameObject);
    }

    //�������ԃX�^�[�g
    public void ButtleTimerStart()
    {
        StartCoroutine(ButtleTimer());
    }

    //AI�N��
    public void ButtleStart()
    {
        behaviortree.enabled = true;
        ButtleTimerStart(); //�J�E���g�_�E���X�^�[�g
    }

    //�ޏꉉ�o(Arbor���ŌĂяo��)
    public void BossFadeOut()
    {
        slimemachine_spr.DOFade(0, 2).SetLink(gameObject).OnComplete(() =>
        {
            this.gameObject.SetActive(false);
        });
    }

    private void OnCollisionEnter2D(UnityEngine.Collision2D collision) 
    {
        if (collision.gameObject.CompareTag("PlayerBullet")) //�_���[�W����
        {
            StartCoroutine(DamageEff());
            parameter.SetInt("HP",HP--); //HP�Ǘ�
            HPslider.value = parameter.GetInt("HP");
            SEplayOneShot("damage"); //SE
            Destroy(collision.gameObject); //���������e�ۂ�����
        }
    }

    //�^�C���I�[�o�[���̏�����(Fungus�d�l)
    private void BossInitialization()
    {
        BossFadeOut();
        HP = maxHP;
        behaviortree.enabled = false;
        HPslider.value = maxHP;
        countTimer.text = ButtleTimerSet.ToString();
        //�{�X�C�x���g�g���K�[�𕜊�
        eventFlowchart.SendFungusMessage(SlimeMachine_TimeUpMessage);
    }

    //�������ԏ���
    IEnumerator ButtleTimer()
    {
        for (int i = ButtleTimerSet; i > -1; i--)
        {
            if (IsBossBreak)
            {
                yield break;
            }
            yield return new WaitForSeconds(1);
            countTimer.color = Color.white; //�������̂���
            countTimer.text = i.ToString();
            if (i < 11)
            {
                countTimer.color = Color.red;
            }
        }
        BossInitialization(); //���Ԑ؂�̏���
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
