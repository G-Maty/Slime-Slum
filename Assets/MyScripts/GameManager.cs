using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UniRx;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    //�`�F�b�N�|�C���g�֌W
    public GameObject PlayerPref;
    private GameObject OperatingPlayer;
    private Player_Move player_move;
    //private Stack<Transform> RespawnPointStack = new Stack<Transform>();
    private Transform RespawnPoint_memory;
    private CinemachineVirtualCamera RespawnPointVC = null;
    private bool iswarp = false;

    private CinemachineBrain cmBrain;

    //�c�e���Ǘ��֌W
    [SerializeField] private int MaxBullets = 10; //�ő�c�e��
    [SerializeField] private int RemainingBullets = 0; //�c�e��(�v���C���[�X�V�ł������Ȃ�)


    /*
     * GameManager
     * ��ɃQ�[���i�s�Ɋւ������
     * �`�F�b�N�|�C���g����
     * �v���C���[�̎c�e���Ǘ�(�v���C���[����サ����V�����I�u�W�F�N�g�ɂȂ邽��)
     */

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        OperatingPlayer = GameObject.FindGameObjectWithTag("Player"); //�v���C���[���擾
        cmBrain = Camera.main.GetComponent<CinemachineBrain>();
        player_move = OperatingPlayer.GetComponent<Player_Move>();
        player_move.checkPoint_Update = checkPoint_Update; //�֐���o�^(�f���Q�[�g)
        player_move.warpCheckpoint = warpCheckpoint; //�֐���o�^(�f���Q�[�g)

        player_move.remainingBullets = 0; //�񕜒n�_���ӂ܂Ȃ�����c�e��[�Ȃ�

        shotNotification_Subscribe(); //���˒ʒm�̍w��
        
    }

    // Update is called once per frame
    void Update()
    {
        player_Update(); //�`�F�b�N�|�C���g�փ��[�v���V�K�v���C���[�̏����擾


    }

    public void checkPoint_Update(GameObject checkpoint) //�`�F�b�N�|�C���g�̃A�b�v�f�[�g����
    {
        checkpoint.transform.GetChild(0).gameObject.SetActive(true); //�ŐV�|�C���g�̓_��
        if(RespawnPoint_memory != null && checkpoint != RespawnPoint_memory.gameObject)
        {
            RespawnPoint_memory.GetChild(0).gameObject.SetActive(false); //�Â��|�C���g�̏���
        }
        RespawnPoint_memory = checkpoint.transform; //�`�F�b�N�|�C���g�̈ʒu���L��
        CinemachineVirtualCamera current = cmBrain.ActiveVirtualCamera as CinemachineVirtualCamera; //�L���X�g���K�v
        RespawnPointVC = current;�@//���X�|�[���n�_��VC���L��
    }

    public void warpCheckpoint() //�Â��v���C���[���������A�V�K�v���C���[�𓊓�
    {
        iswarp = true;
        Vector2 warppoint = new Vector2(RespawnPoint_memory.position.x, RespawnPoint_memory.position.y + 0.3f);

        CinemachineVirtualCamera current = cmBrain.ActiveVirtualCamera as CinemachineVirtualCamera; //�L���X�g���K�v
        current.Priority = 10;
        RespawnPointVC.Priority = 100; //���X�|�[���n�_��VC��L����
        Destroy(OperatingPlayer);
        Instantiate(PlayerPref,warppoint,transform.rotation); //�ʒu�̓X�^�b�N����|�b�v
    }

    private void player_Update()
    {
        if (iswarp) //�`�F�b�N�|�C���g�փ��[�v���V�K�v���C���[�̏����擾
        {
            iswarp = false;
            OperatingPlayer = GameObject.FindGameObjectWithTag("Player");
            player_move = OperatingPlayer.GetComponent<Player_Move>();
            player_move.checkPoint_Update = checkPoint_Update;
            player_move.warpCheckpoint = warpCheckpoint; //�֐���o�^(�f���Q�[�g)

            player_move.remainingBullets = RemainingBullets; //���݂̎c�e���������p��

            shotNotification_Subscribe();
        }
    }

    private void shotNotification_Subscribe()
    {
        //�w��(���˒ʒm�̎󂯎��)
        player_move.shot_observable.Subscribe(
            x =>
            {
                RemainingBullets = RemainingBullets - x;
                player_move.remainingBullets = RemainingBullets;
                Debug.Log("�c�e�F" + RemainingBullets + "��");
            }).AddTo(this);
        //�w�ǁi�c�e��[�ʒm�̎󂯎��j
        player_move.recovery_observable.Subscribe(
            _ =>
            {
                RemainingBullets = MaxBullets;
                player_move.remainingBullets = RemainingBullets;
                Debug.Log("�c�e��[");
            }).AddTo(this);
    }
}
