using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Splines
{
    public static class BezierFunctions
    {
        public struct Bezier
        {
            public Vector3 p0, p1, p2, p3;
            public Bezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
            {
                this.p0 = p0;
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
            }
        }

        /// <summary>
        /// A function that takes a bezier curve (defined by four control points) and a ray
        /// and returns true if the ray intersects the curve within a given threshold
        /// </summary>
        /// <param name="threshold">The maximum allowed shortest distance from the ray to the curve, also affects accuracy</param>
        /// <param name="intersectPoint">The closest point on the curve to the ray (if returned true)</param>
        /// <returns></returns>
        public static bool IsRayIntersectingBezier(Bezier bezier, Ray ray, float threshold, out Vector3 intersectPoint)
        {
            static float SquaredDistance(Vector3 point, Ray ray)
            {
                // Project the point onto the ray
                float t = Vector3.Dot(point - ray.origin, ray.direction) / ray.direction.sqrMagnitude;
                // Clamp the projection to be positive
                t = Mathf.Max(t, 0f);
                // Find the closest point on the ray
                Vector3 closestPoint = ray.origin + ray.direction * t;
                // Return the squared distance between the point and the closest point
                return (point - closestPoint).sqrMagnitude;
            }

            if (threshold! > 0f)
            {
                intersectPoint = bezier.p0;
                return false;
            }

            int numSubdivisions = 8;
            float t = 0f;
            Vector3 closestPoint = bezier.p0;
            float closestDistSqr = Mathf.Infinity;
            float sqrThreshold = threshold * threshold;

            // A loop to iterate over the subdivisions
            for (int i = 0; i < numSubdivisions; i++)
            {
                // Calculate the point on the curve for the current t
                Vector3 point = BezierPoint(bezier, t);

                // Check if the squared distance between the point and the ray is less than the threshold
                float dist = SquaredDistance(point, ray);
                if (dist < sqrThreshold)
                {
                    // Return true if the ray intersects the curve
                    intersectPoint = point;
                    return true;
                }
                else if (dist < closestDistSqr)
                {
                    closestDistSqr = dist;
                    closestPoint = point;
                }

                // Increment t by the subdivision step
                t += 1f / numSubdivisions;
            }

            // Return false if none of the points intersected the ray
            intersectPoint = closestPoint;
            return false;
        }

        /// <summary>
        /// A slow function that returns true if the mouse cursor intersects the curve within a given threshold
        /// </summary>
        /// <param name="threshold">The maximum allowed shortest distance from the ray to the curve in pixels, also affects accuracy</param>
        /// <param name="intersectPoint">The closest point on the curve to the ray (if returned true)</param>
        /// <returns></returns>
        public static bool IsRayIntersectingBezier(Bezier bezier, float threshold, out Vector3 intersectPoint)
        {
            static float SquaredDistance(Vector3 point, Ray ray)
            {
                // Project the point onto the ray
                float t = Vector3.Dot(point - ray.origin, ray.direction) / ray.direction.sqrMagnitude;
                // Clamp the projection to be positive
                t = Mathf.Max(t, 0f);
                // Find the closest point on the ray
                Vector3 closestPoint = ray.origin + ray.direction * t;
                // Return the squared distance between the point and the closest point
                return (point - closestPoint).sqrMagnitude;
            }

            if (threshold! > 0f)
            {
                intersectPoint = bezier.p0;
                return false;
            }

            int numSubdivisions = 8;
            float t = 0f;
            Vector3 closestPoint = bezier.p0;
            float closestDistSqr = Mathf.Infinity;
            float sqrThreshold = threshold * threshold;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            // A loop to iterate over the subdivisions
            for (int i = 0; i < numSubdivisions; i++)
            {
                // Calculate the point on the curve for the current t
                Vector3 point = BezierPoint(bezier, t);

                // Check if the squared distance between the point and the ray is less than the threshold
                float dist = SquaredDistance(point, ray);
                if (dist < sqrThreshold)
                {
                    // Return true if the ray intersects the curve
                    intersectPoint = point;
                    return true;
                }
                else if (dist < closestDistSqr)
                {
                    closestDistSqr = dist;
                    closestPoint = point;
                }

                // Increment t by the subdivision step
                t += 1f / numSubdivisions;
            }

            // Return false if none of the points intersected the ray
            intersectPoint = closestPoint;
            return false;
        }


        /// <summary>
        /// A method to estimate the length of a cubic bezier curve 
        /// no research went into this function so may be slow or outright wrong
        /// </summary>
        /// <param name="threshold">the intended accuracy of the length in scene units</param>
        public static float EstimateLengthInaccurate(Bezier bezier, float threshold)
        {
            if (threshold <= 0)
                throw new ArgumentOutOfRangeException(nameof(threshold), "value must be greater than zero");
            
            // the maximum likely distance (p0->p1->p2->p3)
            float maxDist = (bezier.p3 - bezier.p2).magnitude + (bezier.p2 - bezier.p1).magnitude + (bezier.p1 - bezier.p0).magnitude; // could simplify, may not be worthwhile
            int divisions = Mathf.CeilToInt(maxDist / threshold) + 1; // plus 1 for good luck
            return (BezierPoint(bezier, 1f / divisions) - bezier.p0).magnitude * divisions;
        }

        public static float EstimateLength(Bezier bezier, int iterations)
        {
            return ((bezier.p3 - bezier.p2).magnitude + (bezier.p2 - bezier.p1).magnitude + (bezier.p1 - bezier.p0).magnitude + (bezier.p0 - bezier.p3).magnitude) / 4;
        }

        /// <summary>
        /// A helper function that returns the point on the bezier curve for a given parameter t
        /// </summary>
        /// <param name="t">the intended accuracy of the length in scene units</param>
        public static Vector3 BezierPoint(Bezier bezier, float t)
        {
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * oneMinusT * bezier.p0 +
                   3f * oneMinusT * oneMinusT * t * bezier.p1 +
                   3f * oneMinusT * t * t * bezier.p2 +
                   t * t * t * bezier.p3;
        }

        public static (Bezier, Bezier) Split(float t, Bezier bezier)
        {
            // Interpolate the control points
            Vector2 q0 = Vector2.Lerp(bezier.p0, bezier.p1, t);
            Vector2 q1 = Vector2.Lerp(bezier.p1, bezier.p2, t);
            Vector2 q2 = Vector2.Lerp(bezier.p2, bezier.p3, t);
            Vector2 r0 = Vector2.Lerp(q0, q1, t);
            Vector2 r1 = Vector2.Lerp(q1, q2, t);
            Vector2 s0 = Vector2.Lerp(r0, r1, t);

            // Return the two sub-curves
            return (new Bezier(bezier.p0, q0, r0, s0), new Bezier(s0, r1, q2, bezier.p3));
        }
    }
}
