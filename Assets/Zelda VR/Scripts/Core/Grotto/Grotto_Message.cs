using UnityEngine;

public class Grotto_Message : GrottoExtension_Base
{
    // TODO  (we return true here for now so that message gets displayed)
    override public bool ShouldShowTheGoods { get { return true; } }     
}