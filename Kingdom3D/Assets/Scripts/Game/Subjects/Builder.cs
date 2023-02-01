using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Builder : SubjectBase
{
    public enum States
    {
        Idle,
        Roaming,
        Running,
        Building,
        Locked // catapult and ballista
    }


    private States _currentState;
    States CurrentState { get { return _currentState; } set { ExitState(); _currentState = value; EnterState(); } }

    public Transform targetBuilding;
    public Vector3 targetPos;
    public BuildJob job;
    public Vector3[] path;
    private float reassessTime;

    protected override void Awake()
    {
        base.Awake();

        role = Roles.Builder;
    }

    void Update()
    {
        switch (CurrentState)
        {
            case States.Idle:
                if (reassessTime <= 0)
                {
                    if (LevelController.GetJob(out job) || Random.Range(0, 5) == 0) // 20% chance to start roaming
                    {
                        CurrentState = States.Roaming;
                    }
                    reassessTime = 2f;
                }
                break;
            case States.Roaming:
                break;
            case States.Running:
                break;
            default:
                break;
        }

        reassessTime -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameController.Instance.enemyMask.Contains(other.gameObject.layer))
        {
            CurrentState = States.Running;
        }
    }

    void EnterState()
    {
        switch (CurrentState)
        {
            case States.Idle:
                targetBuilding = null;
                path = null;
                break;
            case States.Roaming:
                if (targetBuilding == null)
                {
                    path = AStarAlgo.instance.FindPath(transform.position, transform.position);
                }
                break;
            case States.Running:
                break;
            default:
                break;
        }
    }

    void ExitState()
    {

    }
}
