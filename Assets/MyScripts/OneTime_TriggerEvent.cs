using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;

/*
 * ��x����̃C�x���g�����g���K�[
 * �C�x���g����������Ǝ��g�̃A�N�e�B�u��False�ɂ��邱�Ƃ�
 * ��x�ƌĂ΂�Ȃ��悤�ɂ���
 */

public class OneTime_TriggerEvent : MonoBehaviour
{
    [SerializeField]
    private Flowchart eventFlowchart = null;
    [SerializeField]
    private string sendMessage = "";
    private Player_Move player_move;
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
        this.gameObject.SetActive(false); //�C�x���g������h��
        player_move.enabled = true; //�ړ��̐�������
        player_move.Unzip_player(); //Player�𓀁i�ȑO�̏d�͓��������p���j
    }
}