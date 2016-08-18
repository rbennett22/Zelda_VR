using UnityEngine;

public class Grotto_Medicine : Grotto_Shop
{
    override public bool ShouldShowTheGoods { get { return Inventory.Instance.HasDeliveredLetterToOldWoman; } }


    public void DeliverLetter()
    {
        Inventory.Instance.HasDeliveredLetterToOldWoman = true;

        PlaySound_Secret();

        ShowTheGoods();
    }


    void PlaySound_Secret()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.secret);
    }
}