using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI_Wizzrobe_Red : EnemyAI
{
    const float OFFSCREEN_OFFSET = -30;      // How far to offset the Wizzrobe's y position when it is "invisible"


    public FlickerEffect flickerEffect;
    bool FlickeringEnabled
    {
        get { return flickerEffect.enabled; }
        set
        {
            flickerEffect.enabled = value;
            GetComponent<Collider>().enabled = !value;
        }
    }

    public int tpDistanceToPlayer = 3;      // How close to the player the Wizzrobe will teleport


    struct WizzrobeRed_State
    {
        readonly public StateEnum state;
        readonly public float duration;
        readonly public float durationVariance;
        readonly public Action activateState_Action;

        public WizzrobeRed_State(StateEnum state, float duration, float durationVariance, Action activateState_Action)
        {
            this.state = state;
            this.duration = duration;
            this.durationVariance = durationVariance;
            this.activateState_Action = activateState_Action;
        }
    }
    WizzrobeRed_State[] _wizzrobeRed_States;

    enum StateEnum
    {
        InvisibleIdle,
        FadingIn,
        Attacking,
        FadingAway,
    }
    WizzrobeRed_State _state;
    void SetState (WizzrobeRed_State value)
    {
        if (value.state == _state.state) { return; }
        _state = value;

        _state.activateState_Action();

        float v = _state.durationVariance;
        float duration = _state.duration + UnityEngine.Random.Range(-v, v);
        StartCoroutine(WaitThenSetState(GetNextState(), duration));
    }
    WizzrobeRed_State GetNextState()
    {
        int i = (int)_state.state + 1;
        if (i > _wizzrobeRed_States.Length - 1)
        {
            i = 0;
        }
        return _wizzrobeRed_States[i];
    }
    IEnumerator WaitThenSetState(WizzrobeRed_State state, float delay)
    {
        yield return new WaitForSeconds(delay);

        SetState(state);
    }


    override protected void Awake()
    {
        base.Awake();

        InitStates();
    }
    void InitStates()
    {
        _wizzrobeRed_States = new WizzrobeRed_State[]
        {
            new WizzrobeRed_State(StateEnum.InvisibleIdle, 3.0f, 0.5f, InvisibleIdle),
            new WizzrobeRed_State(StateEnum.FadingIn, 0.7f, 0, FadeIn),
            new WizzrobeRed_State(StateEnum.Attacking, 1.5f, 0, Attack),
            new WizzrobeRed_State(StateEnum.FadingAway, 0.7f, 0, FadeAway)
        };
    }
    
    void Start()
    {
        SetState(_wizzrobeRed_States[1]);
    }


    void InvisibleIdle()
    {
        FlickeringEnabled = false;
        Disappear();
    }
    void FadeIn()
    {
        Reappear();
        FlickeringEnabled = true;
    }
    void Attack()
    {
        FlickeringEnabled = false;
        if (_doUpdate && !IsPreoccupied)
        {
            _enemy.Attack();
        }
    }
    void FadeAway()
    {
        FlickeringEnabled = true;
    }


    float _storedPosY;
    void Disappear()
    {
        AnimatorInstance.gameObject.SetActive(false);
        GetComponent<Collider>().enabled = false;

        _storedPosY = transform.position.y;
        transform.AddToY(OFFSCREEN_OFFSET);  // Move offscreen to prevent collision with player
    }
    void Reappear()
    {
        AnimatorInstance.gameObject.SetActive(true);
        GetComponent<Collider>().enabled = true;
        transform.SetY(_storedPosY);

        transform.position = GetRandomTeleportPosition();

        Vector3 dir = DirectionToPlayer;
        if (Mathf.Abs(dir.x / dir.z) < 1)
        {
            dir.x = 0;
        }
        else
        {
            dir.z = 0;
        }
        transform.forward = dir;
    }

    Vector3 GetRandomTeleportPosition()
    {
        Vector2 c = Actor.TileToPosition_Center(Player.Tile);
        List<Vector2> pp = new List<Vector2>();     // possible positions to teleport to

        pp.Add(new Vector2(c.x + tpDistanceToPlayer, c.y));
        pp.Add(new Vector2(c.x - tpDistanceToPlayer, c.y));
        pp.Add(new Vector2(c.x, c.y + tpDistanceToPlayer));
        pp.Add(new Vector2(c.x, c.y - tpDistanceToPlayer));

        for (int i = pp.Count - 1; i >= 0; i--)
        {
            Vector2 v2 = pp[i];

            if (!DoesBoundaryAllowPosition(v2))
            {
                pp.RemoveAt(i);
            }
        }

        if (pp.Count == 0)
        {
            return transform.position;
        }

        int randIdx = UnityEngine.Random.Range(0, pp.Count);
        Vector2 p = pp[randIdx];
        return new Vector3(p.x, transform.position.y, p.y);
    }
}