using UnityEngine;
using EFPController.Utils;

namespace EFPController.Extras
{

    public sealed class Ladder : MonoBehaviour
    {

        [Header("Ladder Path")]
        public float pathLength = 10f;
        public Vector3 pathOffset = new Vector3(0f, 0f, -0.5f);

        [Header("Anchor Points")]
        public Transform topPoint;
        public Transform bottomPoint;
        public Transform centerPoint;

        public AudioClip climbSound;
        public AudioClip climbingSound;

        public Vector3 bottomAnchorPoint => transform.position + transform.TransformVector(pathOffset);
        public Vector3 topAnchorPoint => bottomAnchorPoint + transform.up * pathLength;

        public float activeLength { get; private set; }
        public Vector3 topPointOnPath { get; private set; }
        public Vector3 bottomPointOnPath { get; private set; }

        private void Start()
        {
            topPointOnPath = GameUtils.GetClosestPointOnLine(topPoint.position, topAnchorPoint, bottomAnchorPoint);
            bottomPointOnPath = GameUtils.GetClosestPointOnLine(bottomPoint.position, topAnchorPoint, bottomAnchorPoint);
            activeLength = Vector3.Distance(topPointOnPath, bottomPointOnPath);
        }

        public Vector3 ClosestPointOnPath(Vector3 position, out float pathPosition)
        {
            Vector3 result = GameUtils.GetClosestPointOnLine(position, topAnchorPoint, bottomAnchorPoint);
            pathPosition = Vector3.Distance(result, bottomPointOnPath) / activeLength;
            return result;
        }

        public Transform GetClosestPointByPosition(Vector3 position)
        {
            Vector3 pathPoint = ClosestPointOnPath(position, out _);
            Transform result = topPoint;
            if (Vector3.Distance(topPoint.position, pathPoint) > Vector3.Distance(bottomPoint.position, pathPoint))
            {
                result = bottomPoint;
            }
            return result;
        }

        public Transform GetFarthestPointByPosition(Vector3 position)
        {
            Vector3 pathPoint = ClosestPointOnPath(position, out _);
            Transform result = topPoint;
            if (Vector3.Distance(topPoint.position, pathPoint) < Vector3.Distance(bottomPoint.position, pathPoint))
            {
                result = bottomPoint;
            }
            return result;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(bottomAnchorPoint, topAnchorPoint);
            if (bottomPoint != null) Gizmos.DrawWireSphere(bottomPoint.position, 0.2f);
            if (topPoint != null) Gizmos.DrawWireSphere(topPoint.position, 0.2f);
            Gizmos.color = Color.blue;
            if (centerPoint != null) Gizmos.DrawWireSphere(centerPoint.position, 0.2f);
        }

    }

}