using UnityEngine;

public class ConfirmationView : MonoBehaviour 
{
    [SerializeField]
    GameObject _okayButton, _cancelButton;
    [SerializeField]
    GameObject _okayButtonSelected, _cancelButtonSelected;


    GameObject _selectedButton;


	void Start () 
	{ 
        _selectedButton = _cancelButton;
    }
}
