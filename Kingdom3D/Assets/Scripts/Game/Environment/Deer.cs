using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Deer : CreatureBase
{
    enum State
    {
        Idle,
        Graze,
        Observe1,
        Observe2,
        Wander,
        Run,
        Die,
    }

    [SerializeField, Min(0.1f)] float minDist;
    [SerializeField, Min(0.1f)] float maxDist;
    [SerializeField, Min(0.1f)] float spotDistance;

    float reassessTime;
    Rigidbody rig;
    Transform player;

    State _state;

    private State CurrentState { get => _state; set { ExitState(); _state = value; EnterState(); } }

    void Awake()
    {
        health = 3;
        if (maxDist < minDist)
            maxDist = minDist;
        rig = GetComponent<Rigidbody>();
        player = GameController.instance.player;
    }

    private void Update()
    {
        reassessTime -= Time.deltaTime;

        switch (CurrentState)
        {
            case State.Graze:
                break;
            case State.Observe1:
                if (reassessTime <= 0f)
                {
                    if (Vector3.Distance(transform.position, player.position) < spotDistance && player.GetComponent<Rigidbody>().velocity.sqrMagnitude > 1)
                    {
                        CurrentState = State.Run;
                        float x = Random.Range(-1f, 1f);
                        float angle = (Quaternion.FromToRotation(transform.forward * -1, player.position - transform.position) * transform.rotation).eulerAngles.y + 150f * Mathf.Pow(x, 5) / Mathf.Abs(x);
                        transform.DORotate(angle * Vector3.up, .6f);
                    }
                    else
                    {
                        CurrentState = State.Graze;
                    }
                }
                break;
            case State.Observe2:
                if (reassessTime <= 0f)
                {
                    if (Vector3.Distance(transform.position, player.position) < spotDistance && player.GetComponent<Rigidbody>().velocity.sqrMagnitude > 1)
                    {
                        CurrentState = State.Run;
                        float x = Random.Range(-1f, 1f);
                        float angle = (Quaternion.FromToRotation(transform.forward * -1, player.position - transform.position) * transform.rotation).eulerAngles.y + 150f * Mathf.Pow(x, 5) / Mathf.Abs(x);
                        transform.DORotate(angle * Vector3.up, .6f);
                    }
                    else
                    {
                        CurrentState = State.Wander;
                    }
                }
                break;
            case State.Wander:
                if (reassessTime <= 0f)
                {
                    CurrentState = State.Idle;
                }
                else
                {
                    Vector3 vel = transform.forward * moveSpeed;
                    vel.y = rig.velocity.y;
                    rig.velocity = vel;
                }
                break;
            case State.Run:
                if (reassessTime <= 0f)
                {
                    if (Vector3.Distance(transform.position, player.position) < spotDistance && player.GetComponent<Rigidbody>().velocity.sqrMagnitude > 1)
                    {
                        CurrentState = State.Run;
                        float x = Random.Range(-1f, 1f);
                        float angle = (Quaternion.FromToRotation(transform.forward * -1, player.position - transform.position) * transform.rotation).eulerAngles.y + 150f * Mathf.Pow(x, 5) / Mathf.Abs(x);
                        transform.DORotate(angle * Vector3.up, .6f);
                    }
                    else
                    {
                        CurrentState = State.Idle;
                    }
                }
                else
                {
                    Vector3 vel = 1.5f * moveSpeed * transform.forward;
                    vel.y = rig.velocity.y;
                    rig.velocity = vel;
                }

                break;
            default:
                break;
        }

    }

    public override void Damage(float damage, Vector3 source)
    {
        health -= damage;

        if (health <= 0)
        {
            Destroy(rig);
            Destroy(GetComponent<Collider>());
            CurrentState = State.Die;
            isAlive = false;
            transform.DOScaleY(0, 0.3f).OnComplete(Die);
        }
        else
        {
            CurrentState = State.Run;
            float angle = (Quaternion.FromToRotation(transform.forward * -1, source - transform.position) * transform.rotation).eulerAngles.y;
            transform.DORotate(angle * Vector3.up, .6f);
        }
    }

    protected override void Die()
    {
        Instantiate(GameController.instance.coinObj, transform.position, Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up));
        Instantiate(GameController.instance.coinObj, transform.position, Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up));
        Instantiate(GameController.instance.coinObj, transform.position, Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up));
        Destroy(gameObject);
    }

    void EnterState()
    {
        switch (CurrentState)
        {
            case State.Idle:
                rig.velocity = Vector3.up * rig.velocity.y;
                reassessTime = Random.Range(3f, 6f);
                break;
            case State.Graze:
                reassessTime = 4f;
                break;
            case State.Observe1:
                reassessTime = 2f;
                break;
            case State.Observe2:
                reassessTime = 2f;
                break;
            case State.Wander:
                transform.DORotate(Random.Range(0, 360) * Vector3.up, .6f);
                reassessTime = Random.Range(minDist, maxDist) / moveSpeed;
                break;
            case State.Run:
                reassessTime = 5f;
                break;
            default:
                break;
        }
    }

    void ExitState()
    {
        switch (CurrentState)
        {
            case State.Idle:
                break;
            case State.Graze:
                break;
            case State.Observe1:
                break;
            case State.Observe2:
                break;
            case State.Wander:
                break;
            case State.Run:
                break;
            default:
                break;
        }
    }
}
