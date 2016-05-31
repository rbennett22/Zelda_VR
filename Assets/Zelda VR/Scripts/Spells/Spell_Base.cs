using UnityEngine;
using System.Collections.Generic;

public class Spell_Base : MonoBehaviour 
{
    public Actor caster;
    public float duration = 1.0f;


    protected Player PlayerC { get { return CommonObjects.Player_C; } }

    public virtual void Cast(GameObject target = null) { }
}