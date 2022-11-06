using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SwitchVirtualCamera : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera nextVC = default;

    private CinemachineBrain cmBrain;
    //[SerializeField] private CinemachineVirtualCamera previousVC;

    // Start is called before the first frame update
    void Start()
    {
        cmBrain = Camera.main.GetComponent<CinemachineBrain>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }
    }

    private void DisableCurrentVC()
    {
        //���݂̃A�N�e�B�u�ȃo�[�`�����J�������擾
        CinemachineVirtualCamera current = cmBrain.ActiveVirtualCamera as CinemachineVirtualCamera; //�L���X�g���K�v
        current.Priority = 0;
    }

    private void enableNextVC()
    {
        nextVC.enabled = true;
        nextVC.Priority = 100;
    }
}
