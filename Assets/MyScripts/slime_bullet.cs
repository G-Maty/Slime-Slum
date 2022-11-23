using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Unity.VisualScripting;

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

    /*
    private void OnTriggerEnter2D(Collider2D collision) //�e�ۂ͕ǂƓG�ɂ̂ݏՓ�
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground")�@|| collision.tag == "Enemy") //�ǂ̓��C���[�Ŕ��f
        {        
        }
    }
    */

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Instantiate(impact, transform.position, transform.rotation); //�Ԃ������Ƃ��G�t�F�N�g���o��
        Destroy(gameObject); //����������e������
    }

}
