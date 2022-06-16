using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonProxy : MonoBehaviour
{//this script is used to make buttons better at finding objects when instantiated.
    ChessControllerND Controller;
    public void pressed(int i)
    {
        if (Controller == null)
        {
            GameObject temp = GameObject.Find("Main Controller");
            if (temp != null)
                Controller = temp.GetComponent<ChessControllerND>();
        }
        if (Controller != null)
        {
            switch(i)
            {
                case 1:
                    Controller.StartMiniMaxAI();
                    break;
                case 2:
                    Controller.UndoButton();
                    break;
            }
        }
    }
}
