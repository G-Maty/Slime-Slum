using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Arbor;
using Arbor.BehaviourTree;

[AddComponentMenu("")]
public class IdleAction_SlimeMachine : ActionBehaviour {

    private Animator anim;
    private bool actionCompleted = false; //アクションが一通り終了したらtrue
    [SerializeField]
    private float idle_second;

    protected override void OnAwake() {
    }

    protected override void OnStart() {
        anim = GetComponent<Animator>();
        StartCoroutine(idle());
    }

    protected override void OnExecute() {
        if (actionCompleted)
        {
            actionCompleted = false;
            FinishExecute(true); //ノードを終了する（必須）
        }
    }

    protected override void OnEnd() {
	}

    IEnumerator idle()
    {
        anim.SetBool("walk", false);
        anim.SetBool("attack", false);
        yield return new WaitForSeconds(idle_second);
        actionCompleted = true;
    }
}
