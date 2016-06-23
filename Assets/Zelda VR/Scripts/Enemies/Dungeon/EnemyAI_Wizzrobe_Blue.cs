using System.Collections.Generic;
using UnityEngine;
using Immersio.Utility;

public class EnemyAI_Wizzrobe_Blue : EnemyAI
{
    const int GHOST_GLIDE_DISTANCE = 2;
    const float CHANCE_TO_GHOST_GLIDE = 0.2f;


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

    public EnemyAI_Random enemyAI_Random;


    void Start()
    {
        _enemyMove.targetPositionReached_Callback += OnTargetPositionReached;
        
        enemyAI_Random.enabled = true;
    }


    void OnTargetPositionReached(EnemyMove sender, Vector3 moveDirection)
    {
        GhostGlideEnabled = Extensions.FlipCoin(CHANCE_TO_GHOST_GLIDE);
    }

    bool _ghostGlideEnabled;
    public bool GhostGlideEnabled {
        get { return _ghostGlideEnabled; }
        set {
            _ghostGlideEnabled = value;

            enemyAI_Random.enabled = !_ghostGlideEnabled;
            FlickeringEnabled = _ghostGlideEnabled;
            if (_ghostGlideEnabled)
            {
                TargetPosition = DetermineGhostGlidePosition();
            }
        }
    }

    Vector3 DetermineGhostGlidePosition()
    {
        Vector2 c = Actor.TileToPosition_Center(_enemy.Tile);
        List<Vector2> pp = new List<Vector2>();     // possible positions to ghost-glide to

        int d = GHOST_GLIDE_DISTANCE;
        pp.Add(new Vector2(c.x + d, c.y + d));
        pp.Add(new Vector2(c.x - d, c.y + d));
        pp.Add(new Vector2(c.x + d, c.y - d));
        pp.Add(new Vector2(c.x - d, c.y - d));

        // TODO
        /*for (int i = pp.Count - 1; i >= 0; i--)
        {
            Vector2 v2 = pp[i];

            if (!DoesBoundaryAllowPosition(v2))
            {
                pp.RemoveAt(i);
            }
        }*/

        if (pp.Count == 0)
        {
            return transform.position;
        }

        int randIdx = Random.Range(0, pp.Count);
        Vector2 p = pp[randIdx];
        return new Vector3(p.x, transform.position.y, p.y);
    }
}