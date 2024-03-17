using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hawki;

public class ScoreSystem : Singleton<ScoreSystem>
{

    private int totalScore;


    public void ResetScore()
    {
        totalScore = 0;
    }    
    public int getPointShortNode()
    {
        return 3;
    }

    public int getPointLongNode()
    {
        return 3;
    }    


    public int TotalScore()
    {
        return totalScore;
    }
    public PointData CaculatePoint(float offSetTime)
    {
        PointData pointData = new PointData();
        var p = 1;
        if(offSetTime < 0.4f)
        {
            p = 3;
        } else    
        if(offSetTime < 0.6f)
        {
            p = 2;
        }

        totalScore += p;

        pointData.caculatedPoint = p;
        pointData.totalPoint = totalScore;

        return pointData;
    }

}

public class PointData
{
    public int caculatedPoint;
    public int totalPoint;
}