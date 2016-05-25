using System.Collections;
using UnityEngine;

public interface IBurnable
{
    void Burn(BurningFlame sender);
}

[RequireComponent(typeof(Animator))]

public class BurningFlame : MonoBehaviour
{
    public float radius = 1;
    public bool skipSpawnAnimation;

    
    Animator _animator;
    Renderer _animRenderer;
    protected bool RenderingEnabled { get { return _animRenderer.enabled; } set { _animRenderer.enabled = value; } }


    void Awake()
    {
        _animator = GetComponent<Animator>();
        _animRenderer = _animator.GetComponent<Renderer>();

        if (skipSpawnAnimation)
        {
            RenderingEnabled = false;
            _animator.SetTrigger("SkipSpawn");
        }
        else
        {
            RenderingEnabled = true;
        }
    }
    IEnumerator Start()
    {
        if (skipSpawnAnimation)
        {
            yield return new WaitForSeconds(0.01f);

            RenderingEnabled = true;
        }

        if(WorldInfo.Instance.IsInDungeon)
        {
            ActivateLightsInDungeonRoom();
        }
    }

    void ActivateLightsInDungeonRoom()
    {
        DungeonRoom dr = CommonObjects.Player_C.GetOccupiedDungeonRoom();
        if (dr != null && !dr.IsNpcRoom)
        {
            dr.ActivateTorchLights();
        }
    }


    void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider hit in colliders)
        {
            if (hit == null) { continue; }

            GameObject g = hit.gameObject;

            IBurnable b = g.GetComponent<IBurnable>();
            if (b != null)
            {
                Burn(b);
            }
        }
    }

    void Burn(IBurnable b)
    {
        if (b == null)
            return;
        b.Burn(this);
    }
}