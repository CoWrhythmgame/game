using System.Collections.Generic;
using UnityEngine;

public class TimingPoint
{
    public int measureIndex; // 변속이 시작되는 마디 (n_E)
    public double bpm;       // 이 구간의 새로운 BPM
    public double startTime; // 이 마디에 도달했을 때의 절대 시간 (Time_E)
}

public class BeatmapTimer : MonoBehaviour
{
    // 곡의 모든 변속 정보가 담긴 리스트 (반드시 measureIndex 순으로 정렬되어야 함)
    private List<TimingPoint> _timingPoints = new List<TimingPoint>();
    private int _beatsPerMeasure = 4; // 4/4 박자 기준

    // n번째 마디의 정확한 시간을 가져오는 함수
    public double GetMeasureTime(int targetMeasure)
    {
        if (_timingPoints.Count == 0)
            return 0.0;

        // 1. 목표 마디보다 작거나 같은 '가장 최근의 변속 포인트'를 찾음
        TimingPoint currentPoint = _timingPoints[0];

        for (int i = 0; i < _timingPoints.Count; i++)
        {
            if (_timingPoints[i].measureIndex <= targetMeasure)
            {
                currentPoint = _timingPoints[i];
            }
            else
            {
                break;
            }
        }

        // 2. 해당 변속 지점으로부터 몇 마디가 지났는지(Delta) 계산
        int deltaMeasure = targetMeasure - currentPoint.measureIndex;

        // 3. 변속 지점의 시간(Anchor)에 추가된 마디만큼의 시간을 절대 계산으로 더함
        double additionalTime = (deltaMeasure * 60.0 * _beatsPerMeasure) / currentPoint.bpm;

        return currentPoint.startTime + additionalTime;
    }

    public void CalculateTimingPoints()
    {
        if (_timingPoints.Count == 0)
            return;

        // 첫 번째 포인트는 무조건 0마디, 0초부터 시작
        _timingPoints[0].startTime = 0.0;

        for (int i = 1; i < _timingPoints.Count; i++)
        {
            TimingPoint prev = _timingPoints[i - 1];
            TimingPoint curr = _timingPoints[i];

            // 이전 포인트에서 현재 포인트까지 몇 마디인지 계산
            int delta = curr.measureIndex - prev.measureIndex;

            // 이전 포인트의 시간에, 이전 BPM으로 흘러간 시간을 더해 현재 startTime 확정
            double timePassed = delta * 60.0 * _beatsPerMeasure / prev.bpm;
            curr.startTime = prev.startTime + timePassed;
        }
    }

    public void SetTimingPoints(List<GameObject> measures)
    {
        double bpm = 0.0;
        _timingPoints.Clear();

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

            if (measureBpm != bpm)
            {
                bpm = measureBpm;

                _timingPoints.Add(new TimingPoint
                {
                    bpm = measureBpm,
                    measureIndex = measure.GetIndex(),
                    startTime = 0.0
                });
            }
        }

        if (_timingPoints.Count == 0)
        {
            _timingPoints.Add(new TimingPoint
            {
                bpm = 120.0,
                measureIndex = 0,
                startTime = 0.0
            });
        }

        CalculateTimingPoints();
    }

    // 곡 전체가 단일 BPM일 때 타이밍 포인트를 설정하는 함수
    public void SetSingleBpm(double bpm)
    {
        _timingPoints.Clear();

        if (bpm <= 0.0)
            bpm = 120.0;

        _timingPoints.Add(new TimingPoint
        {
            bpm = bpm,
            measureIndex = 0,
            startTime = 0.0
        });

        CalculateTimingPoints();
    }

    // 현재 노래 시간이 몇 번째 마디의 어느 지점인지 계산하는 함수
    public double GetMeasureProgressByTime(double songTime)
    {
        if (_timingPoints.Count == 0)
            return 0.0;

        // 1. 현재 시간보다 작거나 같은 '가장 최근의 변속 포인트'를 찾음
        TimingPoint currentPoint = _timingPoints[0];

        for (int i = 0; i < _timingPoints.Count; i++)
        {
            if (_timingPoints[i].startTime <= songTime)
            {
                currentPoint = _timingPoints[i];
            }
            else
            {
                break;
            }
        }

        // 2. 해당 변속 지점으로부터 몇 초가 지났는지 계산
        double elapsedTime = songTime - currentPoint.startTime;

        // 3. 현재 BPM 기준 한 마디가 몇 초인지 계산
        double secondsPerMeasure = 60.0 * _beatsPerMeasure / currentPoint.bpm;

        if (secondsPerMeasure <= 0.0)
            return currentPoint.measureIndex;

        // 4. 지난 시간을 마디 단위 진행도로 변환
        double progressedMeasure = elapsedTime / secondsPerMeasure;

        return currentPoint.measureIndex + progressedMeasure;
    }
}