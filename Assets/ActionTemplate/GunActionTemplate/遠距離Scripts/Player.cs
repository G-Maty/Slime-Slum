using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float jumpForce = 600f;

    public GameObject bullet;
    public Transform shotPoint;
    Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;


    //�e���΂��N�[���^�C���i���Ԃ������قǘA�˂��Â炭�Ȃ�j
    float coolTime = 0.2f; //�ҋ@����
    float leftCoolTime; //�ҋ@���Ă��鎞��

    bool isRight; //�E�������ǂ���
    private bool isJump = false; //�W�����v�����ǂ���
    private bool isAttack = false; //�U�������ǂ���
    private bool damage = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        isRight = true;
        leftCoolTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        shot();
    }

    void Direction(float inputX) //�X�P�[����ς�����@���ƒe���E�����ɂ�����΂Ȃ�����
    {
        //�E���������͂�180�x��]�A�������E���͂�180�x��]
        if(isRight && inputX < 0)
        {
            transform.Rotate(0, 180f, 0);
            isRight = false;
        }
        if(!isRight && inputX > 0)
        {
            transform.Rotate(0, 180f, 0);
            isRight = true;
        }
    }

    void Movement()
    {
        float x = Input.GetAxis("Horizontal");
        //float y = Input.GetAxis("Vertical");
        Direction(x); //�X�P�[����ς�����@���ƒe���E�����ɂ�����΂Ȃ�����
        if (Input.GetKeyDown(KeyCode.W) && !isJump) //�󒆂ł̓W�����v�ł��Ȃ�
        {
            rb.AddForce(transform.up * jumpForce); //�͂������ăW�����v
            //anim.SetTrigger("jump");
            isJump = true;
        }
        anim.SetFloat("run", Mathf.Abs(x)); //run�A�j���[�V����
        rb.velocity = new Vector2(x * moveSpeed, rb.velocity.y); //�ړ��̎��s
        if (isAttack) //�U�����͓������~�߂�
        {
            rb.velocity = new Vector2(0, 0);
        }

    }

    void shot()
    {
        leftCoolTime -= Time.deltaTime; //�N�[���^�C���X�V(shot�֐��͖�Update�Ă΂��)
        if(leftCoolTime <= 0) //�c��ҋ@���Ԃ�0�b�ȉ��̂Ƃ�
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                anim.SetTrigger("shot"); //�V���b�g�A�j���[�V����
                Instantiate(bullet, shotPoint.position, transform.rotation); //�e��O���ɔ��˂���
                leftCoolTime = coolTime; //�N�[���^�C������
            }
        }
    }

    private void Damage()
    {
        //���Ƀ_���[�W��ԁi�����G���Ԓ��j�Ȃ�I��
        if (damage)
        {
            return;
        }
        StartCoroutine("DamageTimer");
        /*
         * �_���[�W����
         */
    }

    //�_���[�W���󂯂��u�Ԃ̖��G���Ԃ̃^�C�}�[
    private IEnumerator DamageTimer()
    {
        //���Ƀ_���[�W��ԂȂ�I��
        if (damage)
        {
            yield break;
        }
        damage = true;
        //anim.SetTrigger("damage");
        //���G���Ԓ��̓_��
        for (int i = 0; i < 10; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(0.05f);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(0.05f);
        }
        damage = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)�@//�ڒn����ѓ����蔻��
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            isJump = false;
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Damage();
        }
    }

}
