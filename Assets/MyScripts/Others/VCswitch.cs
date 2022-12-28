using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/*
 * アクティブバーチャルカメラの切り替え
 * CM Vcamオブジェクトにアタッチ
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
        //現在のアクティブなバーチャルカメラを取得
        CinemachineVirtualCamera current = cmBrain.ActiveVirtualCamera as CinemachineVirtualCamera; //キャストも必要

        current.Priority = 10; //古い方のVCをオフにする
        this.GetComponent<CinemachineVirtualCamera>().Priority = 100; //今いるVCをオンにする
    }
}
