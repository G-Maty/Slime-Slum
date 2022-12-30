using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;

/*
 * ��x�ȏ�̃C�x���g�����g���K�[
 * �C�ӂ̃{�^�����������Ƃ�
 * �C�x���g������ł��Ăяo���\
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
        if (isTalking) //���łɉ�b�C�x���g���Ȃ�break
        {
            yield break;
        }
        //��b���̃v���C���[�̓����𐧌����邽��
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        isTalking = true;
        gameManager.Restrict_PlayerMove(); //Player�d��

        eventFlowchart.SendFungusMessage(sendMessage); //�t���[�`���[�g�Ƀ��b�Z�[�W�𑗐M���ē���̃C�x���g�i�u���b�N�j�J�n
        yield return new WaitUntil(() => eventFlowchart.GetExecutingBlocks().Count == 0); //�C�x���g�i�u���b�N�j���I������܂ő҂�

        isTalking = false;
        gameManager.Unrestrict_PlayerMove(); //Player��
    }
}