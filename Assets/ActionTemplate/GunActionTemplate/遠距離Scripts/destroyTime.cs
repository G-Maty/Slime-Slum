using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyTime : MonoBehaviour
{
    public float leftTime = 3f; //�e��������܂ł̎���
    void Start()
    {
        Destroy(gameObject, leftTime); //���Ԃ���������e������
    }
}
