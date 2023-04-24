using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TextAsset = UnityEngine.TextCore.Text.TextAsset;
using UnityEngine.EventSystems;
public class SelectedCardDisplay : MonoBehaviour,IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler
{
   public Card _card;
   public Image background;
   public Text name;
   public Image stars;
   public Text starsnum;
   public void show()
   {
      if (_card != null)
      {
         Sprite sp=Resources.Load<Sprite>( "CardImage/" + _card.ID);
         background.sprite = sp;
         name.text = _card.Name;
         starsnum.text = "1";
      }

      
   }
   public void OnPointerClick(PointerEventData pointerEventData)
   {
      Delete();
   }

   public void Delete()
   {
      Debug.Log(gameObject.name);
      if (_card is StoryCard)
      {
      }
      else
      {
         List<int> list = Setting.GetEventSystem().PersonStoryDict[_card.ID];
         foreach (var item in list)
         {
            if (Setting.GetEventSystem().SelectedIDCardDict.ContainsKey(item))
            {
               Setting.GetEventSystem().SelectedPoolList.Remove(item);
               Setting.GetEventSystem().SelectedIDCardDict[item].GetComponent<SelectedCardDisplay>().Delete();
            }
         }
      }
      Setting.GetEventSystem().SelectedPoolList.Remove(_card.ID);
      Setting.GetEventSystem().SelectedIDCardDict.Remove(_card.ID);
      GameObject GO = Setting.GetEventSystem().ShowDetailCard;
      GO.SetActive(false);
      if (_card is StoryCard)
      {
         
      }
      else
      {
         Setting.GetEventSystem().UpdateCanUseStoryCard();
         Setting.GetEventSystem().ResetPSPage();
         if (Setting.GetEventSystem().SelectState == 0)
         {
            
            Setting.GetEventSystem().ShowPersonCard(1);
         }
         else
         {
            Setting.GetEventSystem().ShowStoryCard(1);
         }
         
      }
      Setting.GetEventSystem().UpdateCardNum();
      Setting.GetEventSystem().DeleteSelectedCard(gameObject);
   }
   public void OnPointerEnter(PointerEventData pointerEventData)
   {
      GameObject GO = Setting.GetEventSystem().ShowDetailCard;
      GO.SetActive(true);
      GO.GetComponent<PersonStoryCardDisplay>()._card = _card;
      GO.transform.position = new Vector3(transform.position.x-300, transform.position.y, 0);
      GO.GetComponent<PersonStoryCardDisplay>().Show();

   }

   public void OnPointerExit(PointerEventData pointerEventData)
   {
      GameObject GO = Setting.GetEventSystem().ShowDetailCard;
      GO.SetActive(false);
      
   }
}
