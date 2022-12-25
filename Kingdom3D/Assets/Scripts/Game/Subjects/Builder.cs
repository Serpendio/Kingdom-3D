using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Builder : SubjectBase
{
    public enum States
    {
        Idle,
        Roaming,
        Building,
        Locked // catapult and ballista
    }


    private States _currentState;
    States CurrentState { get { return _currentState; } set { ExitState(); _currentState = value; EnterState(); } }

    public BuildingBase targetBuilding;

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
                break;
            case States.Roaming:
                break;
            default:
                break;
        }
    }

    void EnterState()
    {

    }

    void ExitState()
    {

    }
}
