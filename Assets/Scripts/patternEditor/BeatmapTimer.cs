using System.Collections.Generic;
using UnityEngine;

public class TimingPoint
{
    public int measureIndex;
    public double bpm;
    public double startTime;
}

public class BeatmapTimer : MonoBehaviour
{
    [SerializeField] private int beatsPerMeasure = 4;

    private readonly List<TimingPoint> timingPoints = new List<TimingPoint>();

    public void SetTimingPoints(List<GameObject> measures)
    {
        timingPoints.Clear();

        double currentBpm = -1.0;

        foreach (GameObject measureObj in measures)
        {
            if (measureObj == null)
                continue;

            Measure measure = measureObj.GetComponent<Measure>();

            if (measure == null)
                continue;

            double measureBpm = measure.Getbpm();

            if (measureBpm <= 0.0)
                continue;

            if (measureBpm != currentBpm)
            {
                currentBpm = measureBpm;

                timingPoints.Add(new TimingPoint
                {
                    measureIndex = measure.GetIndex(),
                    bpm = measureBpm,
                    startTime = 0.0
                });
            }
        }

        if (timingPoints.Count == 0)
        {
            timingPoints.Add(new TimingPoint
            {
                measureIndex = 0,
                bpm = 120.0,
                startTime = 0.0
            });
        }

        CalculateTimingPoints();
    }

    public void SetSingleBpm(double bpm)
    {
        timingPoints.Clear();

        if (bpm <= 0.0)
            bpm = 120.0;

        timingPoints.Add(new TimingPoint
        {
            measureIndex = 0,
            bpm = bpm,
            startTime = 0.0
        });
    }

    public void CalculateTimingPoints()
    {
        if (timingPoints.Count == 0)
            return;

        timingPoints[0].startTime = 0.0;

        for (int i = 1; i < timingPoints.Count; i++)
        {
            TimingPoint prev = timingPoints[i - 1];
            TimingPoint curr = timingPoints[i];

            int deltaMeasure = curr.measureIndex - prev.measureIndex;
            double timePassed = deltaMeasure * 60.0 * beatsPerMeasure / prev.bpm;

            curr.startTime = prev.startTime + timePassed;
        }
    }

    public double GetMeasureTime(int targetMeasure)
    {
        if (timingPoints.Count == 0)
            return 0.0;

        TimingPoint currentPoint = timingPoints[0];

        for (int i = 0; i < timingPoints.Count; i++)
        {
            if (timingPoints[i].measureIndex <= targetMeasure)
                currentPoint = timingPoints[i];
            else
                break;
        }

        int deltaMeasure = targetMeasure - currentPoint.measureIndex;
        double additionalTime = deltaMeasure * 60.0 * beatsPerMeasure / currentPoint.bpm;

        return currentPoint.startTime + additionalTime;
    }

    public double GetMeasureProgressByTime(double songTime)
    {
        if (timingPoints.Count == 0)
            return 0.0;

        TimingPoint currentPoint = timingPoints[0];

        for (int i = 0; i < timingPoints.Count; i++)
        {
            if (timingPoints[i].startTime <= songTime)
                currentPoint = timingPoints[i];
            else
                break;
        }

        double elapsedTime = songTime - currentPoint.startTime;
        double secondsPerMeasure = 60.0 * beatsPerMeasure / currentPoint.bpm;

        if (secondsPerMeasure <= 0.0)
            return currentPoint.measureIndex;

        double progressedMeasure = elapsedTime / secondsPerMeasure;

        return currentPoint.measureIndex + progressedMeasure;
    }
}