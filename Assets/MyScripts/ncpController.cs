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
        if (isTalking)
        {
            yield break;
        }
        player_move = GameObject.FindGameObjectWithTag("Player").GetComponent<Player_Move>();

        isTalking = true;
        player_move.Freeze_player();
        player_move.enabled = false;
        //�t���[�`���[�g�Ƀ��b�Z�[�W�𑗐M���ău���b�N���J�n
        flowChart.SendFungusMessage(message);
        //�u���b�N���I������܂ő҂�
        yield return new WaitUntil(() => flowChart.GetExecutingBlocks().Count == 0);

        isTalking = false;
        player_move.enabled = true;
        player_move.Unzip_player();
    }
}