using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet_base : MonoBehaviour
{
    [SerializeField] GameObject impact; //�G�ɂԂ������Ƃ��̃G�t�F�N�g
    [SerializeField] public float speed = 10f; //�e��
    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = -transform.right * speed; //�ʂ��΂�
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Instantiate(impact, transform.position, transform.rotation); //�Ԃ������Ƃ��G�t�F�N�g���o��
        Destroy(gameObject); //����������e������
    }
}
