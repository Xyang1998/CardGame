using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectedCityCardDisplay : MonoBehaviour,IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler
{
    public CityCard _card;
    public Image BackGround;
    public Image Icon;
    public Text Name;
    public Text Description;    
    public float MaxSize = 1.5f;
    private float MinSize = 1.4f;
    private float CurSize = 1.4f;
    public void Show()
    {
        if (_card is not null)
        {
            Sprite sp=Resources.Load<Sprite>( "CardImage/" + _card.ID);
            GetComponent<Image>().sprite = sp;
            Name.text = _card.Name;
            Description.text = _card.Text;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        Setting.GetEventSystem().DeleteSelectedCityCard(_card.ID);
    }
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
      
        StopCoroutine(nameof(Narrow));
        StartCoroutine(nameof(Enlarge));
    }
    public void OnPointerExit(PointerEventData pointerEventData)
    {

        StopCoroutine(nameof(Enlarge));
        StartCoroutine(nameof(Narrow));
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
}
