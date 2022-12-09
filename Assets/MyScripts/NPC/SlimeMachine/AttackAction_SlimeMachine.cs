using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Arbor;
using Arbor.BehaviourTree;
using System.Net.Mail;

[AddComponentMenu("")]
public class AttackAction_SlimeMachine: ActionBehaviour {

    private Animator anim;
    private SlimeMachine slimemachine;
    private bool actionCompleted = false; //アクションが一通り終了したらtrue
    [SerializeField]
    private int shot_count;
    [SerializeField]
    private float shot_cooltime;

    protected override void OnAwake() {
    }

    protected override void OnStart() {
        anim = GetComponent<Animator>();
        slimemachine = GetComponent<SlimeMachine>();
        StartCoroutine(attack());
    }

    protected override void OnExecute() {
        if (actionCompleted)
        {
            actionCompleted = false;
            FinishExecute(true);
        }
    }

    protected override void OnEnd() {
	}

    IEnumerator attack()
    {
        anim.SetBool("attack", true);
        yield return new WaitForSeconds(1);
        for (int i = 0; i < shot_count; i++)
        {
            slimemachine.SlimeMachine_shot();
            slimemachine.SEplayOneShot("shot");
            yield return new WaitForSeconds(shot_cooltime);
        }
        actionCompleted = true;
    }
}
