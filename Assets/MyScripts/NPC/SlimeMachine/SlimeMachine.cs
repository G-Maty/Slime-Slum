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
 *�EHP�Ǘ��A�_���[�W����
 *�Eshot�����Ăяo����
 *�E�o�����o�Ăяo����
 *�E���j���̃C�x���g�Ăяo��
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
        //������
        initialHP = parameter.GetInt("HP", 0);
        slimemachine_spr.color = new Color(255,255,255,0);
        behaviortree.enabled = false;
        this.gameObject.SetActive(false);
        deathAction.ButtleFin.Subscribe(_ =>
        {
            eventFlowchart.SendFungusMessage(SlimeMachine_ButtleEndMessage);
        }); //�o�g���I������b�C�x���g���Ăяo��
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

    //AI�N��
    public void ButtleStart()
    {
        behaviortree.enabled = true;
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
            parameter.SetInt("HP",initialHP--); //HP�Ǘ�
            Destroy(collision.gameObject); //���������e�ۂ�����
        }
    }

    
}
