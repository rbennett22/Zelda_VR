using UnityEngine;

public class SaleItemView : MonoBehaviour 
{
    [SerializeField]
    ZeldaText _priceText;


    public void UpdatePriceText(string str)
    {
        _priceText.Text = str;
    }
}