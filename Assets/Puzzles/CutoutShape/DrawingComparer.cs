using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class DrawingComparer : MonoBehaviour
{
    public LineRenderer Work;
    public LineRenderer Template;
    public float Deviation0;
    public float Deviation100;
    public int ResamplingResolution = 64;

    public UnityEvent<string> OnDebugOutput;

    public readonly struct NormalizedDrawing
    {
        public readonly List<Vector2> Points;
        public readonly float Length;

        public NormalizedDrawing(List<Vector2> points, float length)
        {
            Points = points;
            Length = length;
        }

        public static NormalizedDrawing From(LineRenderer source, int resamplingResolution)
        {
            var pointCount = source.positionCount;
            var points = new Vector3[pointCount];
            source.GetPositions(points);
            return From(points, resamplingResolution);
        }

        public static NormalizedDrawing From(Vector3[] points, int resamplingResolution)
        {
            var samples = new List<Vector2>(resamplingResolution);
            var length = points.Zip(points.Skip(1), Vector3.Distance).Sum();

            var sampleDistance = 0.0f;
            var lengthOfPreviousSamples = 0.0f;
            var distancePerSample = length / resamplingResolution;
            var currentSegment = 0;
            while (true)
            {
                if (samples.Count >= resamplingResolution) break;

                var segmentStart = points[currentSegment];
                var segmentEnd = points[currentSegment + 1];
                var segmentLength = Vector3.Distance(segmentStart, segmentEnd);

                var sampleDistanceInSegment = sampleDistance - lengthOfPreviousSamples;
                if (sampleDistanceInSegment >= segmentLength)
                {
                    lengthOfPreviousSamples += segmentLength;
                    currentSegment++;
                    if (currentSegment >= points.Length)
                    {
                        // If we reached at or beyond the end of the input, just sample the final point
                        samples.Add(points[^1]);
                        break;
                    }

                    continue;
                }

                var t = sampleDistanceInSegment / segmentLength;
                var point = Vector3.Lerp(segmentStart, segmentEnd, t);
                samples.Add(point);
                sampleDistance += distancePerSample;
            }

            // Normalise position
            var center = samples.Aggregate((a, b) => a + b) / samples.Count;
            for (int i = 0; i < samples.Count; i++) samples[i] -= center;

            return new NormalizedDrawing(samples, length);
        }
    }

    public void PerformComparison()
    {
        var work = NormalizedDrawing.From(Work, ResamplingResolution);
        var template = NormalizedDrawing.From(Template, ResamplingResolution);

        var deviation = Mathf.Sqrt(work.Points.Zip(template.Points, (a, b) => (a - b).SqrMagnitude()).Sum());
        var deviationReverse = Mathf.Sqrt(work.Points.Reverse<Vector2>().Zip(template.Points, (a, b) => (a - b).SqrMagnitude()).Sum());
        // Ensure it won't matter which point is the start or end of the drawing path
        deviation = Mathf.Min(deviation, deviationReverse);

        var precision = Mathf.InverseLerp(Deviation0, Deviation100, deviation);
        OnDebugOutput.Invoke($"Deviation: {deviation * (Math.Abs(work.Length - template.Length) + 1.0f)}, Precision: {precision * 100.0f:F2}%");
    }
}
