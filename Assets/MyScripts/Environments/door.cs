using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/*
 * �J�ǂ̏���
 * FungusFlowchart�R�}���h�ŌĂяo��
 * Door�I�u�W�F�N�g�ɃA�^�b�`
 * �h�A���\������e�u���b�N�̃X�v���C�g��z��Ɋi�[
 */

public class door : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer[] doorBlockSprite;
    [Tooltip("�f�t�H���g�ŕǂ��o�������邩")]
    public bool isStartSet = true;

    // Start is called before the first frame update
    void Start()
    {
        //�f�t�H���g�Ńh�A���o�������Ȃ��ꍇ�͓������A��A�N�e�B�u�ɂ���
        if (!isStartSet)
        {
            foreach (SpriteRenderer spr in doorBlockSprite)
            {
                spr.color = new Color(255, 255, 255, 0);
                spr.gameObject.SetActive(false);
            }
        }    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void open_door()
    {
        StartCoroutine(StartOpenDoor());
    }

    public void close_door()
    {
        StartCoroutine(StartCloseDoor());
    }

    IEnumerator StartOpenDoor()
    {
        
        for (int i = doorBlockSprite.Length-1; i > -1; i--) //�h�A���t�F�[�h�A�E�g
        {
            doorBlockSprite[i].DOFade(0, 1).SetLink(gameObject);
            yield return new WaitForSeconds(0.5f);
        }
        for(int j = 0; j < doorBlockSprite.Length; j++) //�h�A���A�N�e�B�u
        {
            doorBlockSprite[j].gameObject.SetActive(false);
        }
    }

    IEnumerator StartCloseDoor()
    {
        foreach (SpriteRenderer spr in doorBlockSprite) //�h�A���A�N�e�B�u
        {
            spr.gameObject.SetActive(true);
        }

        foreach (SpriteRenderer spr in doorBlockSprite) //�h�A���t�F�[�h�C��
        {
            spr.DOFade(1, 1).SetLink(gameObject);
            yield return new WaitForSeconds(0.5f);
        }
    }
}
