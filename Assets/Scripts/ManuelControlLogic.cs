using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManuelControlLogic : MonoBehaviour//misspelled, because of course it is
{
    public Text myText;
    public Slider mySlider;
    public Toggle myToggle;
    private ChessControllerND myNDController;
    // Start is called before the first frame update
    void Start()
    {

        myNDController = GameObject.Find("Main Controller").GetComponent<ChessControllerND>();
        if (myNDController == null)
        {
            Debug.LogError("Cannot find controller");
            return;
        }

    }
    public void ChangeToggleState()
    {
        if(myNDController)
            if (myToggle != null)
            {
                myNDController.EnableManualControl(myToggle.isOn);
            }
    }

    public void ChangeSliderMode()
    {
        int mode = (int) mySlider.value;
        switch (mode)
        {
            case 0:
                myText.text = "Mode: Skip";
                break;
            case 1:
                myText.text = "Mode: Teleporter";
                break;
            case 2:
                myText.text = "Mode: Create Black Pawn";
                break;
            case 3:
                myText.text = "Mode: Create Black Rook";
                break;
            case 4:
                myText.text = "Mode: Create Black Knight";
                break;
            case 5:
                myText.text = "Mode: Create Black Bishop";
                break;
            case 6:
                myText.text = "Mode: Create Black King";
                break;
            case 7:
                myText.text = "Mode: Create Black Queen";
                break;
            case 8:
                myText.text = "Mode: Create White Pawn";
                break;
            case 9:
                myText.text = "Mode: Create White Rook";
                break;
            case 10:
                myText.text = "Mode: Create White Knight";
                break;
            case 11:
                myText.text = "Mode: Create White Bishop";
                break;
            case 12:
                myText.text = "Mode: Create White King";
                break;
            case 13:
                myText.text = "Mode: Create White Queen";
                break;
            case 14:
                myText.text = "Mode: Vaporizer";
                break;
            default:
                myText.text = "Mode Error";
                break;
        }
        if (myNDController)
            myNDController.SetManualControlMode(mode);

    }
}
