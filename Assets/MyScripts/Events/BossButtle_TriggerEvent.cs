using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;
using DG.Tweening;

/*
 * ��x�ȏ�̃C�x���g�����g���K�[
 * �C�ӂ̃{�^�����������Ƃ�
 * �C�x���g������ł��Ăяo���\
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
    private Player_Move player_move;
    [SerializeField] private SpriteRenderer buttleTrigger_renderer;
    [SerializeField] private SpriteRenderer particleUp_renderer;
    private bool triggerflg = false; //true�Ȃ�w��̃L�[�ŃC�x���g�J�n
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
                buttleTrigger_renderer.DOFade(0, 1f).SetLink(gameObject); //������
                particleUp_renderer.DOFade(0, 1f).SetLink(gameObject); //������
                StartCoroutine(Talk()); //��b�C�x���g�J�n
            }
        }
    }

    public void initializationTrigger()
    {
        buttleTrigger_renderer.DOFade(1, 1f).SetLink(gameObject); //�\��
        particleUp_renderer.DOFade(1, 1f).SetLink(gameObject); //�\��
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
        if (isTalking) //���łɉ�b�C�x���g���Ȃ�break
        {
            yield break;
        }
        //��b���̃v���C���[�̓����𐧌����邽��
        player_move = GameObject.FindGameObjectWithTag("Player").GetComponent<Player_Move>();

        isTalking = true;
        player_move.Freeze_player(); //Player�d��
        player_move.enabled = false; //�ړ��𐧌�

        eventFlowchart.SendFungusMessage(sendMessage); //�t���[�`���[�g�Ƀ��b�Z�[�W�𑗐M���ē���̃C�x���g�i�u���b�N�j�J�n
        yield return new WaitUntil(() => eventFlowchart.GetExecutingBlocks().Count == 0); //�C�x���g�i�u���b�N�j���I������܂ő҂�
        isTalking = false;
        player_move.enabled = true; //�ړ��̐�������
        player_move.Unzip_player(); //Player�𓀁i�ȑO�̏d�͓��������p���j
        this.gameObject.SetActive(false);
    }
}
