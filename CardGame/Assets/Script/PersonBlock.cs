using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;



public class PersonBlock : Block,IPointerEnterHandler,IPointerExitHandler,IBeginDragHandler,IDragHandler,IEndDragHandler
{
    public int AttackCount;
    private GameObject Arrow;
    void Start()
    {
        AttackCount = 1;
    }
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (cardGO != null)
        {
            Setting.GetBattleEventSystem().DetailCard.GetComponent<DetailCard>()._card =
                cardGO.GetComponent<HandCardDisplay>()._card;
            Setting.GetBattleEventSystem().DetailCard.GetComponent<DetailCard>()
                .Show(Int32.Parse(cardGO.GetComponent<HandCardDisplay>().starsnum.text));
            Setting.GetBattleEventSystem().DetailCard.SetActive(true);
            Setting.GetBattleEventSystem().DetailCard.transform.position =
                new Vector3(transform.position.x + 280, transform.position.y, 0);
        }

    }
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if (cardGO != null)
        {
            Setting.GetBattleEventSystem().DetailCard.SetActive(false);
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Arrow == null)
        {
            Arrow = Setting.GetBattleEventSystem().Arrow;
        }
        if (cardGO != null)
        {
            if (owner == Owner.Player && Setting.GetBattleEventSystem().roundstate == RoundState.Battle &&
                cardGO.GetComponent<HandCardDisplay>()._card is General)
            {
                Arrow.GetComponent<Arrow>().Show(transform.position);
                Arrow.GetComponent<Arrow>().ToTarget(Input.mousePosition);

            }
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (Arrow == null)
        {
            Arrow = Setting.GetBattleEventSystem().Arrow;
        }
        if (cardGO != null)
        {
            if (owner == Owner.Player && Setting.GetBattleEventSystem().roundstate == RoundState.Battle &&
                cardGO.GetComponent<HandCardDisplay>()._card is General)
            {
                Arrow.GetComponent<Arrow>().ToTarget(Input.mousePosition);

            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (Arrow == null)
        {
            Arrow = Setting.GetBattleEventSystem().Arrow;
        }
        if (cardGO != null)
        {
            if (owner == Owner.Player && Setting.GetBattleEventSystem().roundstate == RoundState.Battle &&
                cardGO.GetComponent<HandCardDisplay>()._card is General)
            {
                if (Setting.GetBattleEventSystem().EnemyCityBlock.Contains(eventData.pointerEnter))
                {
                    Debug.Log(eventData.pointerEnter);
                    Debug.Log(AttackCount);
                    if (AttackCount > 0)
                    {
                        Setting.GetBattleEventSystem().ChoosingBlock = gameObject;
                        Setting.GetBattleEventSystem().TargetBlock = eventData.pointerEnter;
                        Setting.GetBattleEventSystem().AttackConfirm.SetActive(true);
                        Setting.GetBattleEventSystem().roundstate = RoundState.Choosing;
                    }
                }
            }
        }
        Arrow.GetComponent<Arrow>().Hide();
    }
    
}
