using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class SoldierCardDisplay : MonoBehaviour,IPointerClickHandler
{
    public Card _card;
    public Image background;
    public Text name;
    public Text num;
    public void Show()
    {
        name.text = _card.Name;
        num.text = Setting.GetEventSystem().SoldierNum.ToString();
    }
    public void AddOneSoldier()
    {
        Setting.GetEventSystem().SoldierNum +=  1;
        num.text = Setting.GetEventSystem().SoldierNum.ToString();
        Setting.GetEventSystem().UpdateCardNum();
    }
    public void AddSoldier(int Num)
    {
        Setting.GetEventSystem().SoldierNum +=  Num;
        num.text = Setting.GetEventSystem().SoldierNum.ToString();
        Setting.GetEventSystem().UpdateCardNum();
    }
    public void ReduceSoldier(int Num)
    {
        Setting.GetEventSystem().SoldierNum -=  Num;
        num.text = Setting.GetEventSystem().SoldierNum.ToString();
        Setting.GetEventSystem().UpdateCardNum();
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (Setting.GetEventSystem().SoldierNum == 0) return;
        ReduceSoldier(1);
    }
}
