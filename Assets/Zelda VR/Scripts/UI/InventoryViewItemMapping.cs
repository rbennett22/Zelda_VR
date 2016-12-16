using UnityEngine;

public class InventoryViewItemMapping : MonoBehaviour
{
    public enum TypeEnum
    {
        Passive = 0,
        Active = 1,
        Auxillary = 2,
        Triforce = 3,
        Bow = 4
    }


    [SerializeField]
    TypeEnum _type;

    public TypeEnum Type { get { return _type; } }

    [SerializeField]
    int _row = -1, _column = -1, _aux = -1;
    [SerializeField]
    bool _isBow;

    public int Row { get { return _row; } }
    public int Column { get { return _column; } }
    public int Aux { get { return _aux; } }
}