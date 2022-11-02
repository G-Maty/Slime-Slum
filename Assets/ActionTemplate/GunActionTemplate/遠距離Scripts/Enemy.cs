using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int hp = 5; //�̗�
    public GameObject deathEffect; //���j���̃G�t�F�N�g
    public void OnDamage()
    {
        hp -= 1;
        if(hp <= 0)
        {
            Instantiate(deathEffect,transform.position,transform.rotation); //���j���ɃG�t�F�N�g���o��
            Destroy(gameObject); //�G����������
        }
    }
}
