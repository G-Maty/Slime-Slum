using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/*
 * 開閉壁の処理
 * FungusFlowchartコマンドで呼び出し
 */

public class door : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer[] doorBlockSprite;

    // Start is called before the first frame update
    void Start()
    {
        foreach(SpriteRenderer spr in doorBlockSprite)
        {
            spr.gameObject.SetActive(false);
            spr.color = new Color(255,255,255,0);          
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void open_door()
    {
        StartCoroutine(StartOpenDoor());
    }

    public void close_door()
    {
        StartCoroutine(StartCloseDoor());
    }

    IEnumerator StartOpenDoor()
    {
        
        for (int i = doorBlockSprite.Length-1; i > -1; i--)
        {
            doorBlockSprite[i].DOFade(0, 1).SetLink(gameObject);
            yield return new WaitForSeconds(0.5f);
        }
        for(int j = 0; j < doorBlockSprite.Length; j++)
        {
            doorBlockSprite[j].gameObject.SetActive(false);
        }
    }

    IEnumerator StartCloseDoor()
    {
        foreach (SpriteRenderer spr in doorBlockSprite)
        {
            spr.gameObject.SetActive(true);
        }

        foreach (SpriteRenderer spr in doorBlockSprite)
        {
            spr.DOFade(1, 1).SetLink(gameObject);
            yield return new WaitForSeconds(0.5f);
        }
    }

    
}
