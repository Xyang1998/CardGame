using System.Collections;
using System.Collections.Generic;
using Mono.CompilerServices.SymbolWriter;
using UnityEngine;

public class Card
{
  public int ID;
  public string Name;
  public string Text;
  public int CardNumber = 1;

  public Card(int _id,string _text,string _name)
  {
    ID =_id;
    Text = _text;
    Name = _name;
  }
}

public class CityCard : Card
{
  public CityCard(int _id, string _text, string _name) : base(_id, _text, _name)
  {
    
  }
}

public class PersonCard : Card
{
  public string Area;
  public string Title;
  public int Stars;

  public  PersonCard(int _id,string _text,string _name,string _area,string _title,int _stars): base(_id, _text,_name)
  {
    Area = _area;
    Title = _title;
    Stars = _stars;
  }
}
public class Civil_Officials : PersonCard
{
  public float ATKBuff;

  public Civil_Officials(int _id, string _text,string _name, string _area, string _title, int _stars, float _atkBuff) : base(_id,
    _text, _name,_area, _title, _stars)
  {
    ATKBuff = _atkBuff;
  }
}
public class General : PersonCard
{
  public int LeadNumber;
  public int AttackCount = 1;
  public General(int _id, string _text, string _name, string _area, string _title, int _stars, int _leadNumber) : base(_id, _text,_name,
    _area, _title, _stars)
  {
    LeadNumber = _leadNumber;
  }
}
public class StoryCard : Card
{
  public string MatchPerson;
  public string MatchTitle;

  public StoryCard(int _id, string _text, string _name, string _matchPerson, string _matchTitle) : base(_id, _text,_name)
  {
    MatchPerson = _matchPerson;
    MatchTitle = _matchTitle;
  }
}
public class SoldierCard : Card
{
  
  public SoldierCard(int _id,string _text,string _name):base(_id, _text, _name)
  {


  }
}