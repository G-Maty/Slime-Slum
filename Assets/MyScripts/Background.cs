using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    [SerializeField]
    private float deletepos_y;
    [SerializeField]
    private Vector3 createpos;
    [SerializeField]
    private float rollspeed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(0,rollspeed,0);
        if (transform.position.y < deletepos_y)
        {
            transform.position = createpos;
        }
    }
}
