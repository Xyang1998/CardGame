using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public Vector2 StartPos;
    public Vector2 EndPos;
    public RectTransform rect;

    private void Start()
    {
        rect = transform.GetComponent<RectTransform>();
        
    }

    public void Show(Vector2 start)
    {
        rect = transform.GetComponent<RectTransform>();
        StartPos = start;
        gameObject.SetActive(true);
    }

    public void ToTarget(Vector2 end)
    {
        Vector2 ArrowPos = (StartPos + end) / 2;
        float ArrowLen = Vector2.Distance(StartPos, end);
        float ArrowAngle = Mathf.Atan2(end.y - StartPos.y, end.x - StartPos.x);
        transform.position = ArrowPos;
        rect.sizeDelta = new Vector2(ArrowLen, rect.sizeDelta.y);
        rect.eulerAngles = new Vector3(0,0,(ArrowAngle*180)/Mathf.PI);
        
    }

    public void Hide()
    {
        rect.sizeDelta = new Vector2(0, rect.sizeDelta.y);
        gameObject.SetActive(false);
    }

    public void LengthenToEnd(Vector2 endpos,GameObject takego,int take=1,int loss=0)
    {
        StartCoroutine(LengthenToEndIE(endpos,takego,take,loss));
    }

    public IEnumerator LengthenToEndIE(Vector2 endpos,GameObject takego,int take=1,int loss=0) //take=0攻下，take=1未攻下
    {
        Debug.Log("敌方攻击pos"+endpos);
        Debug.Log(StartPos);
        transform.position = StartPos;
        float ArrowAngle = Mathf.Atan2(endpos.y - StartPos.y, endpos.x - StartPos.x);
        rect.eulerAngles = new Vector3(0,0,(ArrowAngle*180)/Mathf.PI);
        float Distance= Vector2.Distance(StartPos, endpos);
        float i = 0;
        while (rect.sizeDelta.x<Distance)
        {
            rect.sizeDelta = new Vector2((Distance*i)/50, rect.sizeDelta.y);
            transform.position = StartPos + (endpos - StartPos)*i/2/50;
            i++;
            yield return new WaitForSeconds(0.01f);
        }
        Setting.GetBattleEventSystem().AIController.UpdateSlodierNum();
        Setting.GetBattleEventSystem().UpdatePlayerSlodierNum();
        takego.GetComponent<CityBlock>().SlodierLoss(loss);
        if(take==0)Setting.GetBattleEventSystem().TakeCity(takego.GetComponent<CityBlock>().cardGO,1);
        Hide();
    }

}
