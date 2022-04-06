using System;
using System.Collections;
using System.Collections.Generic;

//����;
public static class Utility
{
    //ϴ���㷨��������
    public static T[] ShuffleArray<T>(T[] array,int seed)
    {
        //ϵͳ��������ɶ��� ��������seed
        System.Random prng=new System.Random(seed);

        //The Fisher�CYates shuffle�㷨��������
        for (int i = 0; i < array.Length - 1; i++)
        {
            int randomIndex = prng.Next(i, array.Length);//��i�����鳤�����
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }
        return array;
    }

}
