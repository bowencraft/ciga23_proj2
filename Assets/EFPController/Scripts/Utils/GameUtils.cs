using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EFPController.Utils
{

    public static class GameUtils
    {

        const float floatMinTolerance = 0.0000000001f;
        const double doubleMinTolerance = 9.99999943962493E-11;

        public static float ClampAngle(float angle, float min, float max)
        {
            angle = angle % 360f;
            if ((angle >= -360f) && (angle <= 360f))
            {
                if (angle < -360f) angle += 360f;
                if (angle > 360f) angle -= 360f;
            }
            return Mathf.Clamp(angle, min, max);
        }

        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                T temp = list[i];
                int randomIndex = UnityEngine.Random.Range(i, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
            return list;
        }

        public static IList<T> ShuffleCopy<T>(this IList<T> list)
        {
            List<T> newList = new List<T>(list);
            return newList.Shuffle();
        }

        public static int ToLayer(this int bitmask)
        {
            int result = bitmask > 0 ? 0 : 31;
            while (bitmask > 1)
            {
                bitmask = bitmask >> 1;
                result++;
            }
            return result;
        }

        public static float Clamp01MinMax(float min, float max, float value)
	    {
		    return Mathf.Clamp01((value - min) / (max - min));
	    }

        public static bool HasLayer(this LayerMask layerMask, int layer)
        {
            return layerMask == (layerMask | (1 << layer));
        }

        public static bool IsLayer(this int layerMask, int layer)
        {
            return layerMask == (layerMask | (1 << layer));
        }

        public static bool IsLayer(this GameObject go, int layer)
        {
            return go.layer == layer;
        }

        public static bool HasLayer(this GameObject go, LayerMask layerMask)
        {
            return layerMask == (layerMask | (1 << go.layer));
        }

        public static void SetLayer(this GameObject go, int layer, bool includeChilds = false)
        {
            go.layer = layer;
            if (!includeChilds) return;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                Transform child = t.GetChild(i);
                child.gameObject.layer = layer;
                SetLayer(child.gameObject, layer, true);
            }
        }

        public static void TryPlay(this AudioSource source, AudioClip clip, float volume)
        {
            if (clip == null || source == null) return;
            source.PlayOneShot(clip, volume);
        }

        public static void TryPlay(this AudioSource source, AudioClip clip)
        {
            source.TryPlay(clip, 1f);
        }

        public static void TryPlay(this AudioSource source, AudioClip[] clip, float volume)
        {
            if (clip.Length <= 0) return;
            source.TryPlay(clip.Random(), volume);
        }

        public static void TryPlay(this AudioSource source, AudioClip[] clip, Vector2 volume)
        {
            source.TryPlay(clip, volume.Random());
        }

        public static int RandomIndex<T>(this T[] arr)
        {
            return UnityEngine.Random.Range(0, arr.Length);
        }
    
        public static T Random<T>(this T[] arr)
        {
            if (arr.Length > 0)
            {
                return arr[UnityEngine.Random.Range(0, arr.Length)];
            }
            return default;
        }

        public static T Random<T>(this List<T> arr)
        {
            if (arr.Count > 0)
            {
                return arr[UnityEngine.Random.Range(0, arr.Count)];
            }
            return default;
        }

        public static float Random(this Vector2 vector)
        {
            return UnityEngine.Random.Range(vector.x, vector.y);
        }
    
        public static int Random(this Vector2Int vector)
        {
            return UnityEngine.Random.Range(vector.x, vector.y);
        }

        public static Coroutine WaitAndCall(this MonoBehaviour objectRef, float time, System.Action callback)
	    {
            if (objectRef == null) return null;
            if (time <= 0f)
            {
                callback?.Invoke();
                return null;
            }
		    return objectRef.StartCoroutine(WaitAndCallCoroutine(time, callback));
	    }

	    public static IEnumerator WaitAndCallCoroutine(float time, System.Action callback)
	    {
		    yield return new WaitForSeconds(time);
		    callback();
	    }

        public static Coroutine WaitForEndOfFrameAndCall(this MonoBehaviour objectRef, System.Action callback)
        {
            if (objectRef == null) return null;
            return objectRef.StartCoroutine(WaitForEndOfFrameCoroutine(callback));
        }

        public static IEnumerator WaitForEndOfFrameCoroutine(System.Action callback)
        {
            yield return new WaitForEndOfFrame();
            callback();
        }

        public static Coroutine WaitUntilAndCall(this MonoBehaviour objectRef, System.Func<bool> predicate, System.Action callback)
        {
            if (objectRef == null) return null;
            return objectRef.StartCoroutine(WaitUntilCoroutine(predicate, callback));
        }

        public static IEnumerator WaitUntilCoroutine(System.Func<bool> predicate, System.Action callback)
        {
            yield return new WaitUntil(predicate);
            callback();
        }

        public static Coroutine WaitWhileAndCall(this MonoBehaviour objectRef, System.Func<bool> predicate, System.Action callback)
        {
            if (objectRef == null) return null;
            return objectRef.StartCoroutine(WaitWhileCoroutine(predicate, callback));
        }

        public static IEnumerator WaitWhileCoroutine(System.Func<bool> predicate, System.Action callback)
        {
            yield return new WaitWhile(predicate);
            callback();
        }

        public static bool HasParameter(this Animator animator, string paramName)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName) return true;
            }
            return false;
        }

        public static bool IsPlaying(this Animator animator)
        {
            return animator.GetCurrentAnimatorStateInfo(0).length > animator.GetCurrentAnimatorStateInfo(0).normalizedTime/* && !animator.IsInTransition(0)*/;
        }

        public static bool IsPlaying(this Animator animator, string stateName)
        {
            return animator.IsPlaying() && animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
        }

        public static Coroutine SmoothVolume(this MonoBehaviour objectRef, AudioSource asrc, float volume, float time, System.Action callback = null)
        {
            if (objectRef == null) return null;
            return objectRef.StartCoroutine(SmoothVolumeCoroutine(asrc, volume, time, callback));
        }

        public static IEnumerator SmoothVolumeCoroutine(AudioSource asrc, float volume, float time, System.Action callback = null)
        {
            float startVolume = asrc.volume;
            float elapsedTime = 0f;
            while (elapsedTime < time)
            {
                elapsedTime += Time.deltaTime;
                asrc.volume = Mathf.Lerp(startVolume, volume, elapsedTime / time);
                yield return null;
            }
            callback?.Invoke();
        }

        public static float Remap(this float input, float oldLow, float oldHigh, float newLow, float newHigh)
        {
            float t = Mathf.InverseLerp(oldLow, oldHigh, input);
            return Mathf.Lerp(newLow, newHigh, t);
        }

        public static bool IsZero(this float value)
        {
            return Mathf.Abs(value) < floatMinTolerance;
        }

        public static bool IsZero(this Vector2 vector2)
        {
            return vector2.sqrMagnitude < doubleMinTolerance;
        }

        public static bool IsZero(this Vector2Int vector2)
        {
            return vector2.sqrMagnitude < doubleMinTolerance;
        }

        public static bool IsZero(this Vector3 vector3)
        {
            return vector3.sqrMagnitude < doubleMinTolerance;
        }

        public static bool IsZero(this Vector3Int vector3)
        {
            return vector3.sqrMagnitude < doubleMinTolerance;
        }

        public static Vector3 GetVelocityAtPoint(this Rigidbody rigidbody, Vector3 worldPoint)
        {
            Vector3 angularVelocity = rigidbody.angularVelocity;
            if (angularVelocity.IsZero()) return rigidbody.velocity;
            Vector3 centerOfMass = rigidbody.worldCenterOfMass;
            Quaternion q = Quaternion.Euler(angularVelocity * Mathf.Rad2Deg * Time.deltaTime);
            Vector3 rotatedPoint = centerOfMass + q * (worldPoint - centerOfMass);
            Vector3 tangentialVelocity = (rotatedPoint - worldPoint) / Time.deltaTime;
            return rigidbody.velocity + tangentialVelocity;
        }

        public static float EaseInOut(float time, float duration)
        {
            return -0.5f * (Mathf.Cos(Mathf.PI * time / duration) - 1f);
        }

        public static Vector3 GetClosestPointOnLine(Vector3 point, Vector3 line_start, Vector3 line_end)
        {
            Vector3 line_direction = line_end - line_start;
            float line_length = line_direction.magnitude;
            line_direction.Normalize();
            float project_length = Mathf.Clamp(Vector3.Dot(point - line_start, line_direction), 0f, line_length);
            return line_start + line_direction * project_length;
        }

        public static Vector3 GetClosestPointOnRay(Vector3 point, Vector3 ray_origin, Vector3 ray_direction)
        {
            return ray_origin + Vector3.Project(point - ray_origin, ray_direction);
        }

        public static Vector3 GetPointOnLine(Vector3 line_start, Vector3 line_end, float distDelta)
        {
            return (line_start + distDelta * (line_end - line_start));
        }

        public static void RotateAroundPivot(this Transform transform, Vector3 pivotPoint, Vector3 axis, float angle)
        {
            Quaternion rot = Quaternion.AngleAxis(angle, axis);
            transform.position = rot * (transform.position - pivotPoint) + pivotPoint;
            transform.rotation = rot * transform.rotation;
        }

        public static void ScaleAround(this Transform transform, Vector3 pivot, Vector3 newScale)
        {
            Vector3 pivotDelta = transform.localPosition - pivot;
            Vector3 scaleFactor = new Vector3(
                newScale.x / transform.localScale.x,
                newScale.y / transform.localScale.y,
                newScale.z / transform.localScale.z);
            pivotDelta.Scale(scaleFactor);
            transform.localPosition = pivot + pivotDelta;
            transform.localScale = newScale;
        }

        public static void ScaleAroundLerp(this Transform transform, Vector3 pivot, Vector3 newScale, float t)
        {
            Vector3 pivotDelta = transform.localPosition - pivot;
            Vector3 scaleFactor = new Vector3(
                newScale.x / transform.localScale.x,
                newScale.y / transform.localScale.y,
                newScale.z / transform.localScale.z);
            pivotDelta.Scale(scaleFactor);
            transform.localPosition = Vector3.Lerp(transform.localPosition, pivot + pivotDelta, t);
            transform.localScale = Vector3.Lerp(transform.localScale, newScale, t);
        }

    #if UNITY_EDITOR

        public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }

    #endif

    }

}