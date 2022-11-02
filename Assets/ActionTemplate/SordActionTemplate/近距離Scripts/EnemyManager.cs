using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *�@�G�̌��{�R�[�h
 */

public class EnemyManager : MonoBehaviour
{
    public int hp;
    Animator anim;
    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void OnDamage(int damage) //�v���C���[��attack�֐��ŌĂяo�����
    {
        //�����͎󂯂�_���[�W�A�܂�v���C���[�̍U����
        hp -= damage;
        anim.SetTrigger("hurt"); //�_���[�W�A�j���[�V����
        if(hp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        hp = 0;
        anim.SetTrigger("die"); //die�A�j���[�V����
    }

}
