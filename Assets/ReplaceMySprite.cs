using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplaceMySprite : MonoBehaviour
{
    public string spriteName = "";

    //This simple script will detect what it's on, and replace its sprite with the sprite matching the input name. 
    //This should work for graphics packs
    void Start()
    {
        if (spriteName == "")
            return;
        UnityEngine.UI.Image myImage = gameObject.GetComponent<UnityEngine.UI.Image>();
        if (myImage != null)
            myImage.sprite = AudioAndGraphicsSelector.GetSprite(spriteName);
    }
}
