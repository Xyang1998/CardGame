using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class CityBlock : Block,IPointerEnterHandler,IPointerExitHandler
{
    public float SlodierNum = 0;

    public void SlodierLoss(int num)
    {
        SlodierNum =SlodierNum-num<0?0:SlodierNum-num;
        cardGO.GetComponent<CityCardLibraryDisplay>().Description.text = "驻守人数：" + SlodierNum.ToString();
    }
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (cardGO != null)
        {
            
        }

    }
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if (cardGO != null)
        {
            
        }  
    }

    public void ResetSlodierNum()
    {
        SlodierNum = 0;
        cardGO.GetComponent<CityCardLibraryDisplay>().Description.text = "驻守人数：" + SlodierNum.ToString();
    }

}
