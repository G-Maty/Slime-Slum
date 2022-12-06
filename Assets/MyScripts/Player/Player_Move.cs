using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using UnityEngine.UI;
using DG.Tweening;
using Fungus;

/*
 * �v���C���[�̈ړ��E�A�j���[�V�����E�_���[�W����
 */


public class Player_Move: MonoBehaviour
{
    public float moveSpeed = 5.5f;
    public float jumpForce = 200f;
    private bool isJump = false; //�W�����v�����ǂ���
    private bool isup = false; //����t�������ǂ���
    private bool isRight;
    private bool isAttack = false; //�U�������ǂ���
    private bool damage = false;
    private float previousGravityScale;
    private Collider2D boxcollider2d;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    //shot�֌W
    [SerializeField]
    private string AttackKeyCode = "space";
    public GameObject bullet;
    public Transform shotPoint;
    public int remainingBullets { get; set; } //�c�e��(�����v���p�e�B,�o�b�N�t�B�[���h��_remainingBullets����)
    float coolTime = 0.2f; //�ҋ@����
    float leftCoolTime; //�ҋ@���Ă��鎞��
    private Subject<int> _shot = new Subject<int>();
    public IObservable<int> shot_observable => _shot; //�w�ǂ���@�\(Subscribe)�݂̂��O�Ɍ��J���邽��
    private Subject<Unit> _recoveryBullets = new Subject<Unit>();
    public IObservable<Unit> recovery_observable => _recoveryBullets;

    //�ڒn����֌W
    [SerializeField] private LayerMask groundLayer; //for GroundCheck
    

    private Animator anim;
    //�_���[�W�����ʒm
    private Subject<Unit> _warpCheckPoint = new Subject<Unit>();
    public IObservable<Unit> warpCheckPoint_observable => _warpCheckPoint;
    //�`�F�b�N�|�C���g�X�V�ʒm
    private Subject<GameObject> _checkpointUpdate = new Subject<GameObject>();
    public IObservable<GameObject> checkpointUpdate_observabele => _checkpointUpdate;

    //�v���C���[UI(����,�A�j���[�V�����𑀍�)
    private GameObject BulletMGCanvas; //��]�𖳌�������
    private Image baseImage; //UI
    private Image bulletGauge; //UI
    private bool UIcounter = false;





    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxcollider2d = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        BulletMGCanvas = transform.Find("BulletMGCanvas").gameObject;
        baseImage = transform.Find("BulletMGCanvas/BaseImage").GetComponent<Image>(); //�q�v�f��UI���擾
        bulletGauge = transform.Find("BulletMGCanvas/BulletGauge").GetComponent<Image>();
        isRight = true;
    }

    private void FixedUpdate()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        Movement(); //����
        if (isAttack)
        {
            StartCoroutine(UIAnimationSet());
        }
    }

    //�v���C���[���~�i����P�̂ł͂P�t���[���̂݁j
    //���̊֐����g�������ƁA�X�N���v�g��enable = false�ɂ���Ύ~�܂�B
    public void Freeze_player() 
    {
        previousGravityScale = rb.gravityScale;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;
    }

    //�v���C���[�̒�~�����i����P�̂ł͂P�t���[���̂݁j
    //���̊֐����g���O�ɃX�N���v�g��enable = true�ɂ���̂�Y�ꂸ�ɁI
    public void Unzip_player() //Freeze_player�ƃZ�b�g�Ŏg���̂��D�܂���
    {
        rb.gravityScale = previousGravityScale;
    }

    void Movement()
    {
        float x = Input.GetAxis("Horizontal");
        //float y = Input.GetAxis("Vertical");

        if(Mathf.Abs(rb.velocity.y) > 15f) //y���̑��x����
        {
            if(rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(x * moveSpeed, 15f);
            }
            else
            {
                rb.velocity = new Vector2(x * moveSpeed, -15f);
            }
        }

        if (!damage) //�_���[�W�󂯂Ă��Ȃ��Ƃ��ɍs���\
        {
            Direction(x);
            Jump(); //�W�����v
            if (!isJump)
            {
                shot(); //�V���b�g
            }
            anim.SetFloat("run", Mathf.Abs(x)); //run�A�j���[�V����
            rb.velocity = new Vector2(x * moveSpeed, rb.velocity.y); //�ړ��̎��s
            if (isAttack) //�U�����͓������~�߂�
            {
                rb.velocity = new Vector2(0, 0);
            }
        }
        else //�_���[�W�󂯂Ă���Ƃ��ɂ��̏�ɍd��
        {
            Freeze_player();
        }

    }

    void Direction(float inputX) //�X�P�[����ς�����@���ƒe���E�����ɂ�����΂Ȃ�����
    {
        //�E���������͂�180�x��]�A�������E���͂�180�x��]
        if (isRight && inputX < 0)
        {
            transform.Rotate(0, 180f, 0);
            BulletMGCanvas.transform.Rotate(0, 180f, 0); //UI�͉�]�𒼂�
            isRight = false;
        }
        if (!isRight && inputX > 0)
        {
            transform.Rotate(0, 180f, 0);
            BulletMGCanvas.transform.Rotate(0, 180f, 0); //UI�͉�]�𒼂�
            isRight = true;
        }
    }

    void Jump()
    {
        bool isground_up = checkground_up();
        bool isground_down = checkground_down();
        //float y = Input.GetAxis("Vertical");

        if (!isground_up && isground_down) //���ڒn��
        {
            if (Input.GetKeyDown(KeyCode.W) && !isup)
            {
                isJump = true;
                anim.SetBool("landing_down", false);
                anim.SetTrigger("jump");
                rb.gravityScale = -3f;
                rb.AddForce(transform.up * jumpForce); //�͂������ăW�����v
                isup = true;
            }
            if (!isup) //�W�����v�����u�Ԃ̌�쓮��h�~
            {
                isJump = false;
                anim.SetBool("landing_down",true); //���n�A�j���[�V����
            }
        }
        if(isground_up && !isground_down) //��ڒn��
        {
            if (Input.GetKeyDown(KeyCode.S) && isup) //�󒆂ł̓W�����v�ł��Ȃ�
            {
                isJump = true;
                anim.SetBool("landing_up",false);
                anim.SetTrigger("jump");
                rb.gravityScale = 3f;
                rb.AddForce(transform.up * -jumpForce); //�͂������ăW�����v
                isup = false;
            }
            if (isup) //�W�����v�����u�Ԃ̌�쓮��h�~
            {
                isJump = false;
                anim.SetBool("landing_up",true); //���n�A�j���[�V����
            }
        }       
    }

    private void shot()
    {
        leftCoolTime -= Time.deltaTime; //�N�[���^�C���X�V(shot�֐��͖�Update�Ă΂��)
        if (leftCoolTime <= 0 && remainingBullets > 0) //�c��ҋ@���Ԃ�0�b�ȉ��̂Ƃ��A���c�e����0�ȏ�̂Ƃ�
        {
            if (Input.GetKeyDown(AttackKeyCode))
            {
                //UI�\��
                baseImage.DOFade(1f, 0f).SetLink(gameObject);
                bulletGauge.DOFade(1f, 0f).SetLink(gameObject);
                isAttack = true;
                anim.SetTrigger("shot"); //�V���b�g�A�j���[�V����
                Instantiate(bullet, shotPoint.position, transform.rotation); //�e��O���ɔ��˂���
                _shot.OnNext(1); //1�����˂������Ƃ�ʒm
                leftCoolTime = coolTime; //�N�[���^�C������
            }
        }
    }

    public void shotEnd() //shot�A�j���[�V�����̍ŏI�t���[���ŗ��p
    {
        isAttack = false;
    }

    //Damage()�ł͎�ɖ��G���Ԃƃ_���[�W�A�j���[�V�����̎����A�X�g���[���̍w�ǒ��~
    //��̓I�ȃ_���[�W������DamageTimer����onDead�ɓo�^�ς�(�f���Q�[�g)
    private void Damage()
    {
        //���Ƀ_���[�W��ԁi�����G���Ԓ��j�Ȃ�I��
        if (damage)
        {
            return;
        }
        boxcollider2d.enabled = false;
        StartCoroutine("DamageTimer");
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
        _shot.OnCompleted(); //�w�Ǌ����i�X�g���[�����I�������邽�߁j
        _recoveryBullets.OnCompleted();�@//�w�Ǌ����i�X�g���[�����I�������邽�߁j
        _checkpointUpdate.OnCompleted(); //�w�Ǌ����i�X�g���[�����I�������邽�߁j
        //anim.SetTrigger("damage");
        //���G���Ԓ��̓_��
        for (int i = 0; i < 10; i++) //1�b
        {
            spriteRenderer.enabled = false; //���F�_��
            yield return new WaitForSeconds(0.05f);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(0.05f);
        }
        damage = false;
        //�_���[�W����
        _warpCheckPoint.OnNext(Unit.Default);
        _warpCheckPoint.OnCompleted();
    }

    //UI�t�F�[�h�A�j���[�V����
    private IEnumerator UIAnimationSet()
    {
        //���t���[���Ă΂��
        if (isAttack || UIcounter)
        {
            yield break;
        }
        else
        {
            if (baseImage.color.a == 0) //�A���t�@�l0�Ȃ炻�������A�j���[�V�������ĂԕK�v�Ȃ�
            {
                yield break;
            }
            UIcounter = true;
            for(int i=0; i < 30; i++)
            {
                if (isAttack)
                {
                    UIcounter = false;
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
            }
            //�v���C���[�ҋ@����4�b���Ƃɂ����ƌĂ΂��̂Ń����������������Ȃ̂ŃA���t�@�l0�Ȃ��΂�
            Debug.Log(baseImage.color.a);      
            Sequence sequence = DOTween.Sequence();
            sequence.Append(baseImage.DOFade(0f, 1f));
            sequence.Join(bulletGauge.DOFade(0f, 1f));
            sequence.Play().OnUpdate(() =>
            {
                if (isAttack)
                {
                    sequence.Kill();
                    UIcounter = false;
                    baseImage.DOFade(1f, 0f).SetLink(gameObject);
                    bulletGauge.DOFade(1f, 0f).SetLink(gameObject);
                }
            }).OnComplete(() => UIcounter = false).SetLink(gameObject);
        }
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Respawn"))
        {
            _checkpointUpdate.OnNext(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("RecoveryPoint"))
        {
            //UI�\��
            baseImage.DOFade(1f, 0f).SetLink(gameObject);
            bulletGauge.DOFade(1f, 0f).SetLink(gameObject);
            _recoveryBullets.OnNext(Unit.Default); //RecoveryPoint�ʉ߂̒ʒm
        }
    }

    private void OnCollisionEnter2D(UnityEngine.Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trap") || collision.gameObject.CompareTag("Enemy"))
        {
            Damage();
        }
    }

}
