using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/*
 * �A�N�e�B�u�o�[�`�����J�����̐؂�ւ�
 * CM Vcam�I�u�W�F�N�g�ɃA�^�b�`
 */

public class VCswitch : MonoBehaviour
{
    private CinemachineBrain cmBrain;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }
        cmBrain = Camera.main.GetComponent<CinemachineBrain>();
        //���݂̃A�N�e�B�u�ȃo�[�`�����J�������擾
        CinemachineVirtualCamera current = cmBrain.ActiveVirtualCamera as CinemachineVirtualCamera; //�L���X�g���K�v

        current.Priority = 10; //�Â�����VC���I�t�ɂ���
        this.GetComponent<CinemachineVirtualCamera>().Priority = 100; //������VC���I���ɂ���
    }
}
