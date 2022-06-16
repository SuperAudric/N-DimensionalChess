using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpUIEnabler : MonoBehaviour//Displays help when H is pressed.
{
    public GameObject myTextGameObject;
    private void Update()
    {
        if (myTextGameObject != null)
            if (Input.GetKey(KeyCode.H))//I could use Input.GetKeyDown/up, but this doesn't fail if frames are skipped.
            {
                myTextGameObject.SetActive(true);
            }
            else
            {
                myTextGameObject.SetActive(false);
            }
    }
}
