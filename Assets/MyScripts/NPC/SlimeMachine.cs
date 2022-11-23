using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using Arbor;

public class SlimeMachine : MonoBehaviour
{
    private int initialHP;
    private ParameterContainer parameter;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform shotpoint;

    // Start is called before the first frame update
    void Start()
    {
        parameter = GetComponent<ParameterContainer>();
        initialHP = parameter.GetInt("HP", 0);
    }

    public void SlimeMachine_shot() //�O������̌Ăяo��
    {
        Instantiate(bulletPrefab, shotpoint.position, shotpoint.rotation);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerBullet")) //�_���[�W����
        {
            parameter.SetInt("HP",initialHP--);
            Destroy(collision.gameObject); //���������e�ۂ�����
        }
    }
}
