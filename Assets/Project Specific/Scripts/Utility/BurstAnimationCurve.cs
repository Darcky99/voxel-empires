using System;
using Unity.Collections;
using UnityEngine;

namespace Unity.Mathematics
{
    public struct BurstAnimationCurve
    {
        private NativeArray<Keyframe> _KeyFrames;

        public BurstAnimationCurve(Keyframe[] keyframes)
        {
            _KeyFrames = new NativeArray<Keyframe>(keyframes, Allocator.Persistent);
        }

        public float Evaluate(float time)
        {
            if (_KeyFrames.Length == 0)
            {
                return 0f;
            }
            // Handle edge cases: time outside the range of keyframes
            if (time <= _KeyFrames[0].time)
            {
                return _KeyFrames[0].value;
            }
            if (time >= _KeyFrames[_KeyFrames.Length - 1].time)
            {
                return _KeyFrames[_KeyFrames.Length - 1].value;
            }
            // Find the keyframe interval
            for (int i = 0; i < _KeyFrames.Length - 1; i++)
            {
                Keyframe start = _KeyFrames[i];
                Keyframe end = _KeyFrames[i + 1];

                if (time >= start.time && time <= end.time)
                {
                    // Normalize time within the interval
                    start.outTangent = math.clamp(start.outTangent, -1f, 1f);
                    end.inTangent = math.clamp(end.inTangent, -1f, 1f);
                    float t = InverseLerp(start.time, end.time, time);
                    float interpolationResult = HermiteInterpolate(start.value, end.value, start.outTangent, end.inTangent, t);
                    return interpolationResult;
                }
            }
            return 0f; // Fallback, should not reach here
        }

        private float InverseLerp(float a, float b, float time)
        {
            return (time - a) / (b - a);
        }

        private float HermiteInterpolate(float p0, float p1, float m0, float m1, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            float h00 = (2 * t3) - (3 * t2) + 1;
            float h10 = t3 - (2 * t2) + t;
            float h01 = (-2 * t3) + (3 * t2);
            float h11 = t3 - t2;
            float result = (h00 * p0) + (h10 * m0) + (h01 * p1) + (h11 * m1);
            return result;
        }
        private float HermiteInterpolate(float p0, float p1, float m0, float m1, float w0, float w1, float t)
        {
            // Apply weights to tangents
            m0 *= w0;
            m1 *= w1;

            float t2 = t * t;
            float t3 = t2 * t;
            float h00 = 2 * t3 - 3 * t2 + 1;
            float h10 = t3 - 2 * t2 + t;
            float h01 = -2 * t3 + 3 * t2;
            float h11 = t3 - t2;

            return h00 * p0 + h10 * m0 + h01 * p1 + h11 * m1;
        }
        public void Dispose()
        {
            if (_KeyFrames.IsCreated)
            {
                _KeyFrames.Dispose();
            }
        }
    }
}
