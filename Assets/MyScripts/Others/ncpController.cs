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
        if (isTalking) //���łɉ�b�C�x���g���Ȃ�break
        {
            yield break;
        }
        //��b���̃v���C���[�̓����𐧌����邽��
        player_move = GameObject.FindGameObjectWithTag("Player").GetComponent<Player_Move>();

        isTalking = true;
        player_move.Freeze_player(); //Player�d��
        player_move.enabled = false; //�ړ��𐧌�

        flowChart.SendFungusMessage(message); //�t���[�`���[�g�Ƀ��b�Z�[�W�𑗐M���ē���̃C�x���g�i�u���b�N�j�J�n
        yield return new WaitUntil(() => flowChart.GetExecutingBlocks().Count == 0); //�C�x���g�i�u���b�N�j���I������܂ő҂�

        isTalking = false; 
        player_move.enabled = true; //�ړ��̐�������
        player_move.Unzip_player(); //Player�𓀁i�ȑO�̏d�͓��������p���j
    }
}