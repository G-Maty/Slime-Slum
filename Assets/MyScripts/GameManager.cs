using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject PlayerPref;
    private GameObject OperatingPlayer;
    private Player_Move player_move;
    private Stack<Transform> RespawnPointStack = new Stack<Transform>();
    private bool iswarp = false;

    /*
     * GameManager
     * ��ɃQ�[���i�s�Ɋւ������
     * �`�F�b�N�|�C���g����
     */

    // Start is called before the first frame update
    void Start()
    {
        OperatingPlayer = GameObject.FindGameObjectWithTag("Player");
        player_move = OperatingPlayer.GetComponent<Player_Move>();
        player_move.checkPoint_Update = checkPoint_Update;
        player_move.warpCheckpoint = warpCheckpoint; //�֐���o�^(�f���Q�[�g)
    }

    // Update is called once per frame
    void Update()
    {
        if (iswarp) //���[�v���V�K�v���C���[�̏����擾
        {
            iswarp = false;
            OperatingPlayer = GameObject.FindGameObjectWithTag("Player");
            player_move = OperatingPlayer.GetComponent<Player_Move>();
            player_move.checkPoint_Update = checkPoint_Update;
            player_move.warpCheckpoint = warpCheckpoint; //�֐���o�^(�f���Q�[�g)
        }
    }

    public void checkPoint_Update(GameObject checkpoint) //�`�F�b�N�|�C���g�̃A�b�v�f�[�g����
    {
        checkpoint.transform.GetChild(0).gameObject.SetActive(true); //�ŐV�|�C���g�̓_��
        if (RespawnPointStack.Count != 0 && checkpoint != RespawnPointStack.Peek().gameObject)
        {
            RespawnPointStack.Peek().GetChild(0).gameObject.SetActive(false); //�Â��|�C���g�̏���
        }
        RespawnPointStack.Push(checkpoint.transform); //�`�F�b�N�|�C���g�̈ʒu���X�^�b�N�փv�b�V��
        //Debug.Log("�`�F�b�N�|�C���g�X�V");
    }

    public void warpCheckpoint() //�Â��v���C���[���������A�V�K�v���C���[�𓊓�
    {
        iswarp = true;
        Vector2 warppoint = new Vector2(RespawnPointStack.Peek().position.x, RespawnPointStack.Peek().position.y + 0.5f);

        Destroy(OperatingPlayer);
        //player_move.gameObject.transform.position = RespawnPointStack.Peek().position;
        Instantiate(PlayerPref,warppoint,transform.rotation); //�ʒu�̓X�^�b�N����|�b�v
        //Debug.Log("�`�F�b�N�|�C���g��");
        //Debug.Log(RespawnPointStack.Peek().position);
    }
}
