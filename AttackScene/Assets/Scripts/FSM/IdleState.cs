﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : IState
{
    private FSM manager;
    private Parameter parameter;

    private float timer;
    public IdleState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        parameter.animator.Play("Idle");
    }

    public void OnUpdate()
    {
        timer += Time.deltaTime;

        if (parameter.getHit)
        {
            manager.TransitionState(StateType.Hit);
            return;
        }
        if (parameter.target != null &&
            parameter.target.position.x >= parameter.chasePoints[0].position.x &&
            parameter.target.position.x <= parameter.chasePoints[1].position.x)
        {
            manager.TransitionState(StateType.React);
            return;
        }
        if (timer >= parameter.idleTime)
        {
            manager.TransitionState(StateType.Patrol);
        }
    }

    public void OnExit()
    {
        timer = 0;
    }
}

public class PatrolState : IState
{
    private FSM manager;
    private Parameter parameter;

    private int patrolPosition;
    public PatrolState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        parameter.animator.Play("Walk");
    }

    public void OnUpdate()
    {
        manager.FlipTo(parameter.patrolPoints[patrolPosition]);

        manager.transform.position = Vector2.MoveTowards(manager.transform.position,
            parameter.patrolPoints[patrolPosition].position, parameter.moveSpeed * Time.deltaTime);

        if (parameter.getHit)
        {
            manager.TransitionState(StateType.Hit);
            return;
        }
        if (parameter.target != null &&
            parameter.target.position.x >= parameter.chasePoints[0].position.x &&
            parameter.target.position.x <= parameter.chasePoints[1].position.x)
        {
            manager.TransitionState(StateType.React);
            return;
        }
        if (Mathf.Abs(manager.transform.position.x - parameter.patrolPoints[patrolPosition].position.x) < .1f)
        {
            manager.TransitionState(StateType.Idle);
        }
    }

    public void OnExit()
    {
        patrolPosition++;

        if (patrolPosition >= parameter.patrolPoints.Length)
        {
            patrolPosition = 0;
        }
    }
}

public class ChaseState : IState
{
    private FSM manager;
    private Parameter parameter;

    private float chaseTimer;

    public ChaseState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        chaseTimer = 0;
        parameter.animator.Play("Walk");
    }

    public void OnUpdate()
    {
        manager.FlipTo(parameter.target);
        //Debug.Log(chaseTimer);
        if (chaseTimer > parameter.jumpChaseCD)
        {
            //Debug.Log("needJump");
            jumpToward();

            manager.FlipTo(parameter.target);
            chaseTimer = 0;
        }
        if (parameter.target)
            manager.transform.position = Vector2.MoveTowards(manager.transform.position,
            new Vector2(parameter.target.position.x, manager.transform.position.y), parameter.chaseSpeed * Time.deltaTime);

        if (parameter.getHit)
        {
            manager.TransitionState(StateType.Hit);
            return;
        }
        if (parameter.target == null ||
            manager.transform.position.x < parameter.chasePoints[0].position.x ||
            manager.transform.position.x > parameter.chasePoints[1].position.x)
        {
            manager.TransitionState(StateType.Idle);
            return;
        }
        if (Physics2D.OverlapCircle(parameter.attackPoint.position, parameter.attackArea, parameter.targetLayer))
        {
            manager.TransitionState(StateType.Attack);
        }
        chaseTimer += Time.deltaTime;
    }

    public void OnExit()
    {

    }

    public void jumpToward()
    {
        if (parameter.target)
        {
            if (Mathf.Abs(manager.transform.position.x - parameter.target.position.x) > parameter.jumpChaseDistance)
            {
                if (manager.transform.position.x > parameter.target.position.x)
                {
                    parameter.rigidbody.velocity = new Vector2(-parameter.jumpSpeed, 5);

                }
                else
                {
                    parameter.rigidbody.velocity = new Vector2(parameter.jumpSpeed, 5);

                }
            }
        }
    }
}

public class ReactState : IState
{
    private FSM manager;
    private Parameter parameter;

    private AnimatorStateInfo info;
    public ReactState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        parameter.animator.Play("React");
    }

    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);

        if (parameter.getHit)
        {
            manager.TransitionState(StateType.Hit);
            return;
        }
        if (info.normalizedTime >= .95f)
        {
            manager.TransitionState(StateType.Chase);
        }
    }

    public void OnExit()
    {

    }
}

public class AttackState : IState
{
    private FSM manager;
    private Parameter parameter;


    private AnimatorStateInfo info;
    public AttackState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {

        parameter.animator.Play("Attack");
    }

    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);



        if (parameter.getHit)
        {
            manager.TransitionState(StateType.Hit);
            return;
        }
        if (info.normalizedTime >= .95f)
        {
            manager.TransitionState(StateType.Chase);
        }
    }

    public void OnExit()
    {

    }

}

public class HitState : IState
{
    private FSM manager;
    private Parameter parameter;

    private AnimatorStateInfo info;
    public HitState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        parameter.animator.Play("Hit");
    }

    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);

        if (parameter.health <= 0)
        {
            manager.TransitionState(StateType.Death);
            return;
        }
        if (info.normalizedTime >= .95f)
        {
            //parameter.target = GameObject.FindWithTag("Player").transform;

            manager.TransitionState(StateType.Chase);
        }
    }

    public void OnExit()
    {
        parameter.getHit = false;
    }
}

public class DeathState : IState
{
    private FSM manager;
    private Parameter parameter;

    public DeathState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        parameter.animator.Play("Dead");
        parameter.collider.enabled = false;
        parameter.rigidbody.constraints = RigidbodyConstraints2D.FreezePosition;
    }

    public void OnUpdate()
    {

    }

    public void OnExit()
    {

    }
}