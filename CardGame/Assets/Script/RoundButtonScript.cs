using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class RoundButtonScript : MonoBehaviour
{
    private int ImageIndex = 0;
    private RectTransform rect;
    private void Start()
    {
        rect = GetComponent<RectTransform>();
        Sprite sp = Resources.Load<Sprite>(Setting.TurnButtonPath[ImageIndex]);
        GetComponent<Image>().sprite = sp;
    }

    public void ImageChange()
    {
        ImageIndex++;
        if (ImageIndex == Setting.TurnButtonPath.Length)
        {
            ImageIndex = 0;
        }
        GetComponent<Button>().enabled = false;
        StartCoroutine(ImageChangeIE());
    }

    private IEnumerator ImageChangeIE()
    {
        Sprite nextsp = Resources.Load<Sprite>(Setting.TurnButtonPath[ImageIndex]);
        float x = 0;
        float y = rect.rotation.eulerAngles.y;
        float z=rect.rotation.eulerAngles.z;
        while (rect.rotation.eulerAngles.x<90)
        {
            rect.rotation=Quaternion.Euler(x,y,z);
            x += 6;
            yield return new WaitForSeconds(0.02f);
        }
        GetComponent<Image>().sprite = nextsp;
        y += 180;
        z += 180;
        while (rect.rotation.eulerAngles.x>0.1)
        {
            rect.rotation=Quaternion.Euler(x,y,z);
            x += 6;
            yield return new WaitForSeconds(0.02f);
        }
        GetComponent<Button>().enabled = true;

    }
        
    
    
}
