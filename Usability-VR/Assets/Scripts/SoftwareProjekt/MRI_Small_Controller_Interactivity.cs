using UnityEngine;

public class MRI_Small_Controller_Interactivity : MonoBehaviour
{
    public GameObject button_top;
    public GameObject button_bottom;
    public GameObject button_left;
    public GameObject button_right;
    public GameObject button_center;

    public delegate void ButtonPress();
    public event ButtonPress OnButtonTopPress;
    public event ButtonPress OnButtonBottomPress;
    public event ButtonPress OnButtonLeftPress;
    public event ButtonPress OnButtonRightPress;
    public event ButtonPress OnButtonCenterPress;

    public void ButtonActivity(GameObject sender, bool isButtonPressed)
    {
        if (sender == button_top)
        {
            if (isButtonPressed) OnButtonTopPress?.Invoke();
        }
        else if (sender == button_bottom)
        {
            if (isButtonPressed) OnButtonBottomPress?.Invoke();
        }
        else if (sender == button_left)
        {
            if (isButtonPressed) OnButtonLeftPress?.Invoke();
        }
        else if (sender == button_right)
        {
            if (isButtonPressed) OnButtonRightPress?.Invoke();
        }
        else if(sender == button_center)
        {
            if (isButtonPressed) OnButtonCenterPress?.Invoke();
        }
    }
}
