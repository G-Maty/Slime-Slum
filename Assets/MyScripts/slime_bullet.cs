using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slime_bullet : MonoBehaviour
{
    [SerializeField] GameObject impact; //�G�ɂԂ������Ƃ��̃G�t�F�N�g
    [SerializeField] float speed = 10f; //�e��
    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed; //�ʂ��΂�
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            //�쐬�����R�[�hEnemy�N���X�^�̋�ϐ�enem���쐬���A�Ԃ������I�u�W�F�N�g�̃R���|�[�l���gEnemy�N���X���擾����
            Enemy enem = collision.GetComponent<Enemy>();
            Instantiate(impact, transform.position, transform.rotation); //�G�ɂԂ������Ƃ��G�t�F�N�g���o��
            Destroy(gameObject); //�G�ɂ���������e������
            enem.OnDamage();     //OnDamage�֐����Ăяo��
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Instantiate(impact, transform.position, transform.rotation); //�G�ɂԂ������Ƃ��G�t�F�N�g���o��
            Destroy(gameObject); //�G�ɂ���������e������
        }
        //Debug.Log("hit");
    }
}
