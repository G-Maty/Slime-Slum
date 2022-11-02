using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *�@�ړ��E�W�����v�E�ڋߍU���̌��{�R�[�h
 */

public class Player_Move: MonoBehaviour
{
    public float moveSpeed = 5.5f;
    public float jumpForce = 200f;
    private bool isJump = false; //�W�����v�����ǂ���
    private bool isup = false; //����t�������ǂ���
    private bool damage = false;


    [SerializeField] private LayerMask groundLayer; //for GroundCheck



    //���C���[��Enemy��ǉ�
    public LayerMask enemyLayer; //����̃��C���[�ł̂݃R���C�_�[�����m����t�B���^�[(�G���G����Ȃ����̋��)
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isJump) //�X�y�[�X��������āA�W�����v������Ȃ����
        {
        }
        Movement(); //����
    }

    void Movement()
    {
        float x = Input.GetAxis("Horizontal");
        //float y = Input.GetAxis("Vertical");
        
        if (x > 0)
        {
            transform.localScale = new Vector3(1.0f, 1.0f, 1);
        }
        if (x < 0)
        {
            transform.localScale = new Vector3(-1.0f, 1.0f, 1);
        }
        Jump(); //�W�����v
        anim.SetFloat("run", Mathf.Abs(x)); //run�A�j���[�V����
        rb.velocity = new Vector2(x * moveSpeed, rb.velocity.y); //�ړ��̎��s
    }

    void Jump()
    {
        bool isGround = CheckGround();
        bool isground_up = checkground_up();
        bool isground_down = checkground_down();


        if(!isground_up && isground_down) //���ڒn
        {
            if (Input.GetKeyDown(KeyCode.W) && !isup) //�󒆂ł̓W�����v�ł��Ȃ�
            {
                anim.SetBool("landing_down", false);
                isJump = true;
                anim.SetTrigger("jump");
                rb.gravityScale = -3f;
                rb.AddForce(transform.up * jumpForce); //�͂������ăW�����v
                isup = true;
            }
            if (!isup)
            {
                anim.SetBool("landing_down",true);
            }
        }
        if(isground_up && !isground_down) //��ڒn
        {
            if (Input.GetKeyDown(KeyCode.S) && isup) //�󒆂ł̓W�����v�ł��Ȃ�
            {
                anim.SetBool("landing_up",false);
                isJump = true;
                anim.SetTrigger("jump");
                rb.gravityScale = 3f;
                rb.AddForce(transform.up * -jumpForce); //�͂������ăW�����v
                isup = false;
            }
            if (isup)
            {
                anim.SetBool("landing_up",true);
            }
        }


            
        Debug.Log(isJump);
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

    //�ڒn�`�F�b�N
    private bool CheckGround()
    {
        bool check_down = checkground_down();
        bool check_up = checkground_up();

        if (check_down && check_up) //�ڒn���Ă�����true
        {
            Debug.Log("double graound");
            return false;
        }
        else if(check_down || check_up)
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }

    private bool checkground_down()
    {
        //�����̋���
        float raydown_distance = 0.5f;
        //float rayRadius = 0.2f;
        //�����Փˎ��A�Փ˃I�u�W�F�N�g�̏����i�[(�n�ʃ��C���Ɍ���)
        RaycastHit2D hitrayinfo_down;
        hitrayinfo_down = Physics2D.Raycast(transform.position,  Vector2.down, raydown_distance, groundLayer);
        //�f�o�b�O�p
        Debug.DrawRay(transform.position, Vector3.down * raydown_distance, Color.red);
        //�������n�ʃ��C���̃R���C�_�ƂԂ������Ƃ���true��Ԃ�
        if(hitrayinfo_down.distance == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private bool checkground_up()
    {
        //�����̋���
        float rayup_distance = 0.5f;
        //float rayRadius = 0.2f;
        //�����Փˎ��A�Փ˃I�u�W�F�N�g�̏����i�[(�n�ʃ��C���Ɍ���)
        RaycastHit2D hitrayinfo_up;
        hitrayinfo_up = Physics2D.Raycast(transform.position,  Vector2.up, rayup_distance, groundLayer);
        //�f�o�b�O�p
        Debug.DrawRay(transform.position, Vector3.up * rayup_distance, Color.blue);
        //�������n�ʃ��C���̃R���C�_�ƂԂ������Ƃ���true��Ԃ�
        if (hitrayinfo_up.distance == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
