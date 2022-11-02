using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *�@�ړ��E�W�����v�E�ڋߍU���̌��{�R�[�h
 */

public class PlayerManager : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float jumpForce = 600f;
    private bool isJump = false; //�W�����v�����ǂ���
    private bool isAttack = false; //�U�������ǂ���
    private bool damage = false;


    public Transform attackPoint; //�U�������蔻�蕔��
    public float attackRadius; //�U�������蔻��̔��a
    //���C���[��Enemy��ǉ�
    public LayerMask enemyLayer; //����̃��C���[�ł̂݃R���C�_�[�����m����t�B���^�[(�G���G����Ȃ����̋��)
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    Animator anim;
    int at = 1; //�U����

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isJump) //�X�y�[�X��������āA�W�����v������Ȃ����
        {
            anim.SetTrigger("attack"); //attack�A�j���[�V����(���̃��[�V�����r���̃C�x���g��attack�֐����Ă΂��)
            isAttack = true;
        }
        Movement(); //����

    }

    public void attack() //attack�A�j���[�V�����̃C�x���g�@�\�ŌĂяo�������
    {
        //�����蔻�蕔�ɂԂ��������̂�hitEnemys�ɂ͂���
        Collider2D[] hitEnemys = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);
        foreach(Collider2D hitEnemy in hitEnemys)
        {
            hitEnemy.GetComponent<EnemyManager>().OnDamage(at); //�Ԃ������G�̃X�N���v�g��OnDamage�֐����Ă�
        }
    }

    private void OnDrawGizmosSelected() //�����蔻�蕔�̉���
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position,attackRadius);
    }

    void Movement()
    {
        float x = Input.GetAxis("Horizontal");
        //float y = Input.GetAxis("Vertical");
        if (x > 0)
        {
            transform.localScale = new Vector3(-1.0f, 1.0f, 1);
        }
        if (x < 0)
        {
            transform.localScale = new Vector3(1.0f, 1.0f, 1);
        }
        if(Input.GetKeyDown(KeyCode.W) && !isJump) //�󒆂ł̓W�����v�ł��Ȃ�
        {
            rb.AddForce(transform.up * jumpForce); //�͂������ăW�����v
            anim.SetTrigger("jump");
            isJump = true;
        }
        anim.SetFloat("run", Mathf.Abs(x)); //run�A�j���[�V����
        rb.velocity = new Vector2(x * moveSpeed, rb.velocity.y); //�ړ��̎��s
        if (isAttack) //�U�����͓������~�߂�
        {
            rb.velocity = new Vector2(0, 0);
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

    public void AttackFinish() //�U�����[�V�����I���ɃC�x���g�ŌĂ΂�AisAttack�t���O��߂�
    {
        isAttack = false;
    }

}
