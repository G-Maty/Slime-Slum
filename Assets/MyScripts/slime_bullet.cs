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
            enem.OnDamage();     //OnDamage�֐����Ăяo��
        }
        Debug.Log("hit");
        Instantiate(impact, transform.position, transform.rotation); //�Ȃɂ��ɂԂ������Ƃ��G�t�F�N�g���o��
        Destroy(gameObject); //�G�ɂ���������e������
    }
}
