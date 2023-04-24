using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


static class Setting
{
    public static float[] CPersonATKBuff = new float[]{0,1,2,3,4,5,6,7,8,9,10};
    public static int[] GPersonLeadNumber = new int[] {0,9, 19, 29, 39, 49, 59, 69, 79, 89, 99 };
    private static CardSetEvent EventSystem;
    public static float MoveSpeed=0.05f; //抽牌移动速度
    public static float CardSpeed = 10f;//手牌展示移动速度
    public static string[] TurnButtonPath = {"other/ButtonImage/TurnButton/YourTurn","other/ButtonImage/TurnButton/CardChange","other/ButtonImage/TurnButton/Attack","other/ButtonImage/TurnButton/Defence","other/ButtonImage/TurnButton/Enemy"};
    public static CardSetEvent GetEventSystem()
    {
        if (!EventSystem)
        {
            EventSystem = GameObject.FindObjectOfType<CardSetEvent>().GetComponent<CardSetEvent>();
            return EventSystem;
        }

        return EventSystem;

    }
    private static BattleEventSystem BattleEventSystem;
    public static BattleEventSystem GetBattleEventSystem()
    {
        if (!BattleEventSystem)
        {
            BattleEventSystem = GameObject.FindObjectOfType<BattleEventSystem>().GetComponent<BattleEventSystem>();
            return BattleEventSystem;
        }

        return BattleEventSystem;

    }
}
