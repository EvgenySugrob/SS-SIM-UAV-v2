using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vmaya.Scene3D
{
    public class BoundsComponent : MonoBehaviour
    {
        public Vector3 center;
        public Vector3 size;

        internal Bounds bounds => new Bounds(center, size);

        private void OnValidate()
        {
            if (size.sqrMagnitude == 0) refreshBounds();
        }

        private void refreshBounds()
        {
            Bounds bounds = Utils.getRBounds(transform, false);
            size = bounds.size;
            center = bounds.center - transform.position;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(center, size);
        }

        public bool Contained(Vector3 pos)
        {
            return bounds.Contains(transform.InverseTransformPoint(pos));
        }
    }
}
