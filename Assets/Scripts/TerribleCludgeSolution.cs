using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//this file exists 
public class TerribleCludgeSolution : MonoBehaviour
{
    bool activated = false;
    int[] toBeDimensions = new int[0];
    public UnityEngine.UI.InputField myInputField;
    public int[] ParseDimensions(string input)
    {
        List<int> understoodDimensions = new List<int>();
        bool inNumber = false;
        for (int i = 0; i < input.Length; i++)
        {
            if ((int)input[i] >= (int)'0' && (int)input[i] <= (int)'9')
            {
                if (inNumber)
                {
                    understoodDimensions[understoodDimensions.Count - 1] *= 10;
                    understoodDimensions[understoodDimensions.Count - 1] += ((int)input[i] - (int)'0');
                }
                else
                {
                    understoodDimensions.Add((int)input[i] - (int)'0');
                }
                inNumber = true;
            }
            else
                inNumber = false;
        }


        return understoodDimensions.ToArray();
    }

    // Update is called once per frame
    void Update()
    {

        if (!activated)
        {
            if (myInputField != null)
                if (myInputField.text != null)
                    toBeDimensions = ParseDimensions(myInputField.text);
            Debug.Log(ChessControllerND.ArrayToString(toBeDimensions));

        }
    }
    public void poke()
    {
        if (GameObject.Find("Main Controller") != null)
        {
            activated = true;
            if (toBeDimensions.Length > 0)
                GameObject.Find("Main Controller").GetComponent<ChessControllerND>().boardDimensions = toBeDimensions;
        }
    }
}
