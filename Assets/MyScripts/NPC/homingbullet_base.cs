using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class homingbullet_base : MonoBehaviour
{
    private Transform playerTrans; //�ǂ�������Ώۂ�Transform
    [SerializeField] GameObject impact; //�G�ɂԂ������Ƃ��̃G�t�F�N�g
    [SerializeField] private float bulletSpeed;  �@ //�e�̑��x
    [SerializeField] private float limitSpeed;      //�e�̐������x
    private Rigidbody2D rb;                         //�e��Rigidbody2D
    private Transform bulletTrans;                  //�e��Transform

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bulletTrans = GetComponent<Transform>();
    }

    private void Start()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
        if(playerTrans == null) //�v���C���[��������Ȃ��ꍇ
        {
            Instantiate(impact, transform.position, transform.rotation); //�G�t�F�N�g���o��
            Destroy(gameObject);
        }
        else
        {
            Vector3 vector3 = playerTrans.position - bulletTrans.position;  //�e����ǂ�������Ώۂւ̕������v�Z
            rb.AddForce(vector3.normalized * bulletSpeed);                  //�����̒�����1�ɐ��K���A�C�ӂ̗͂�AddForce�ŉ�����

            float speedXTemp = Mathf.Clamp(rb.velocity.x, -limitSpeed, limitSpeed); //X�����̑��x�𐧌�
            float speedYTemp = Mathf.Clamp(rb.velocity.y, -limitSpeed, limitSpeed);  //Y�����̑��x�𐧌�
            rb.velocity = new Vector3(speedXTemp, speedYTemp);�@�@�@�@�@�@�@�@�@�@�@//���ۂɐ��������l����
        }
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Instantiate(impact, transform.position, transform.rotation); //�Ԃ������Ƃ��G�t�F�N�g���o��
        Destroy(gameObject); //����������e������
    }
}
