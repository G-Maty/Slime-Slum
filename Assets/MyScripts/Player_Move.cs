using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *　移動・ジャンプ・接近攻撃の見本コード
 */

public class Player_Move: MonoBehaviour
{
    public float moveSpeed = 5.5f;
    public float jumpForce = 200f;
    private bool isJump = false; //ジャンプ中かどうか
    private bool isup = false; //張り付き中かどうか
    private bool damage = false;


    [SerializeField] private LayerMask groundLayer; //for GroundCheck



    //レイヤーにEnemyを追加
    public LayerMask enemyLayer; //特定のレイヤーでのみコライダーを検知するフィルター(敵か敵じゃないかの区別)
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
        if (Input.GetKeyDown(KeyCode.Space) && !isJump) //スペースが押されて、ジャンプ中じゃなければ
        {
        }
        Movement(); //動く
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
        Jump(); //ジャンプ
        anim.SetFloat("run", Mathf.Abs(x)); //runアニメーション
        rb.velocity = new Vector2(x * moveSpeed, rb.velocity.y); //移動の実行
    }

    void Jump()
    {
        bool isGround = CheckGround();
        bool isground_up = checkground_up();
        bool isground_down = checkground_down();


        if(!isground_up && isground_down) //下接地
        {
            if (Input.GetKeyDown(KeyCode.W) && !isup) //空中ではジャンプできない
            {
                anim.SetBool("landing_down", false);
                isJump = true;
                anim.SetTrigger("jump");
                rb.gravityScale = -3f;
                rb.AddForce(transform.up * jumpForce); //力を加えてジャンプ
                isup = true;
            }
            if (!isup)
            {
                anim.SetBool("landing_down",true);
            }
        }
        if(isground_up && !isground_down) //上接地
        {
            if (Input.GetKeyDown(KeyCode.S) && isup) //空中ではジャンプできない
            {
                anim.SetBool("landing_up",false);
                isJump = true;
                anim.SetTrigger("jump");
                rb.gravityScale = 3f;
                rb.AddForce(transform.up * -jumpForce); //力を加えてジャンプ
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
        //既にダメージ状態（＝無敵時間中）なら終了
        if (damage)
        {
            return;
        }
        StartCoroutine("DamageTimer");
        /*
         * ダメージ処理
         */
    }

    //ダメージを受けた瞬間の無敵時間のタイマー
    private IEnumerator DamageTimer()
    {
        //既にダメージ状態なら終了
        if (damage)
        {
            yield break;
        }
        damage = true;
        //anim.SetTrigger("damage");
        //無敵時間中の点滅
        for (int i = 0; i < 10; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(0.05f);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(0.05f);
        }
        damage = false;
    }

    //接地チェック
    private bool CheckGround()
    {
        bool check_down = checkground_down();
        bool check_up = checkground_up();

        if (check_down && check_up) //接地していたらtrue
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
        //光線の距離
        float raydown_distance = 0.5f;
        //float rayRadius = 0.2f;
        //光線衝突時、衝突オブジェクトの情報を格納(地面レイヤに限定)
        RaycastHit2D hitrayinfo_down;
        hitrayinfo_down = Physics2D.Raycast(transform.position,  Vector2.down, raydown_distance, groundLayer);
        //デバッグ用
        Debug.DrawRay(transform.position, Vector3.down * raydown_distance, Color.red);
        //光線が地面レイヤのコライダとぶつかったときにtrueを返す
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
        //光線の距離
        float rayup_distance = 0.5f;
        //float rayRadius = 0.2f;
        //光線衝突時、衝突オブジェクトの情報を格納(地面レイヤに限定)
        RaycastHit2D hitrayinfo_up;
        hitrayinfo_up = Physics2D.Raycast(transform.position,  Vector2.up, rayup_distance, groundLayer);
        //デバッグ用
        Debug.DrawRay(transform.position, Vector3.up * rayup_distance, Color.blue);
        //光線が地面レイヤのコライダとぶつかったときにtrueを返す
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
