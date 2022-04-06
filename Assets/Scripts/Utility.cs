using System;
using System.Collections;
using System.Collections.Generic;

//多用途
public static class Utility
{
    //洗牌算法打乱数组
    public static T[] ShuffleArray<T>(T[] array,int seed)
    {
        //系统随机数生成对象 加入种子seed
        System.Random prng=new System.Random(seed);

        //The Fisher–Yates shuffle算法打乱数组
        for (int i = 0; i < array.Length - 1; i++)
        {
            int randomIndex = prng.Next(i, array.Length);//从i到数组长度随机
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }
        return array;
    }

}
