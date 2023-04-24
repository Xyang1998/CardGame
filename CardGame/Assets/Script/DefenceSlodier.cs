using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class DefenceSlodier : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
{
    private GameObject Arrow;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Arrow == null)
        {
            Arrow = Setting.GetBattleEventSystem().Arrow;
        }
        if (Setting.GetBattleEventSystem().roundstate == RoundState.Defense &&
            Setting.GetBattleEventSystem().player.SlodierInCityNum <
            Setting.GetBattleEventSystem().player.PlayerSlodierNum)
        {
           
        }
        Arrow.GetComponent<Arrow>().Show(transform.position);
        Arrow.GetComponent<Arrow>().ToTarget(Input.mousePosition);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Arrow == null)
        {
            Arrow = Setting.GetBattleEventSystem().Arrow;
        }
        Arrow.GetComponent<Arrow>().ToTarget(Input.mousePosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (Arrow == null)
        {
            Arrow = Setting.GetBattleEventSystem().Arrow;
        }
        if (Setting.GetBattleEventSystem().roundstate == RoundState.Defense &&
            Setting.GetBattleEventSystem().player.SlodierInCityNum <
            Setting.GetBattleEventSystem().player.PlayerSlodierNum)
        {
            if (Setting.GetBattleEventSystem().PlayerCityBlock.Contains(eventData.pointerEnter))
            {
                Setting.GetBattleEventSystem().DefenceSlodierConfirm.SetActive(true);
                Debug.Log("驻守");
                Setting.GetBattleEventSystem().TargetBlock = eventData.pointerEnter;
            }
            
        }
        Arrow.GetComponent<Arrow>().Hide();
    }

}
