using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TextAsset = UnityEngine.TextCore.Text.TextAsset;
using System.IO;

public class CityCardLibraryDisplay : MonoBehaviour,IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler
{
    public GameObject parent;
    public CityCard _card;
    public Image BackGround;
    public Image Icon;
    public Text Name;
    public Text Description;
    public float MaxSize = 1.1f;
    private float MinSize = 1.0f;
    private float CurSize = 1.0f;
    public bool Ontable = false;
    

    public void Start()
    {
        if (Ontable != false)
        {
            Setting.GetBattleEventSystem().CardTOTable.AddListener(ToTable);
        }
    }

    public void Show()
    {
        if (_card is not null)
        {
            


            Sprite sp=Resources.Load<Sprite>( "CardImage/" + _card.ID);
            GetComponent<Image>().sprite = sp;
            
        }
                
            //Icon=
            Name.text = _card.Name;
            Description.text = _card.Text;
        
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (Ontable == false)
        {
            if (Setting.GetEventSystem().CheckExist(_card.ID))
            {

                Setting.GetEventSystem().AddSelectedCityCard(_card);
                Setting.GetEventSystem().SelectedCityCardShow();
            }
        }
        else
        {
            
        }
    }

    public IEnumerator ToParent(Vector3 Start)
    {
        transform.SetParent(Setting.GetBattleEventSystem().Canvas.transform);
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        //Setting.GetBattleEventSystem().ResetCityPos(parent.GetComponent<CityBlock>().owner==Owner.Player?0:1,gameObject);
        transform.position = Start;
        Vector3 End = parent.transform.position;
        while (Vector2.Distance(new Vector2(transform.position.x, transform.position.y),
                   End) > 5)
        {
            transform.position = Vector3.Lerp(transform.position, End, Setting.MoveSpeed);
            yield return new WaitForFixedUpdate();
        }
        transform.position = End;
        transform.SetParent(parent.transform);
        Setting.GetBattleEventSystem().CheckVictory();
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        transform.SetAsLastSibling();
        if (Ontable == false)
        {
            
            StopCoroutine(nameof(Narrow));
            StartCoroutine(nameof(Enlarge));
        }
        else
        {
            
        }
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if (Ontable == false)
        {
            StopCoroutine(nameof(Enlarge));
            StartCoroutine(nameof(Narrow));
        }
        else
        {
            
        }
    }

    IEnumerator Enlarge()
    {
        
        while (CurSize <= MaxSize)
        {
            
            CurSize += (MaxSize - MinSize) * 0.1f;
            transform.localScale = new Vector3(CurSize, CurSize, 1);
            yield return new WaitForSeconds(0.02f);
        }
    }
    IEnumerator Narrow()
    {
        while (CurSize >= MinSize)
        {
            
            CurSize -= (MaxSize - MinSize) * 0.1f;
            transform.localScale = new Vector3(CurSize, CurSize, 1);
            yield return new WaitForSeconds(0.02f);
        }
    }

    void ToTable()
    {
        Ontable = true;
    }
    

}
