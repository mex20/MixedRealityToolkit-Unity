﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Experimental.UI.BoundsControlTypes;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Experimental.UI.BoundsControl
{
    /// <summary>
    /// Helper class providing some static utility functions for <see cref="BoundsControl"/> <see cref="BoundsControlHandlesBase">handles</see>/>
    /// </summary>
    internal class VisualUtils
    {

        internal static void HandleIgnoreCollider(Collider handlesIgnoreCollider, List<Transform> handles, bool ignore = true)
        {
            if (handlesIgnoreCollider != null)
            {
                foreach (Transform handle in handles)
                {
                    Collider[] colliders = handle.gameObject.GetComponents<Collider>();
                    foreach (Collider collider in colliders)
                    {
                        UnityEngine.Physics.IgnoreCollision(collider, handlesIgnoreCollider, ignore);
                    }
                }
            }
        }

        internal static float GetMaxComponent(Vector3 vec)
        {
            return Mathf.Max(Mathf.Max(vec.x, vec.y), vec.z);
        }

        internal static Bounds GetMaxBounds(GameObject g)
        {
            var b = new Bounds();
            Mesh currentMesh;
            foreach (MeshFilter r in g.GetComponentsInChildren<MeshFilter>())
            {
                if ((currentMesh = r.sharedMesh) == null) { continue; }

                if (b.size == Vector3.zero)
                {
                    b = currentMesh.bounds;
                }
                else
                {
                    b.Encapsulate(currentMesh.bounds);
                }
            }
            return b;
        }

        internal static void ApplyMaterialToAllRenderers(GameObject root, Material material)
        {
            if (material != null)
            {
                Renderer[] renderers = root.GetComponentsInChildren<Renderer>();

                for (int i = 0; i < renderers.Length; ++i)
                {
                    renderers[i].material = material;
                }
            }
        }

        /// <summary>
        /// Add all common components to a corner or rotate affordance.
        /// </summary>
        internal static void AddComponentsToAffordance(GameObject afford, Bounds bounds, HandlePrefabCollider colliderType,
            CursorContextInfo.CursorAction cursorType, Vector3 colliderPadding, Transform parent, bool drawTetherWhenManipulating)
        {
            if (colliderType == HandlePrefabCollider.Box)
            {
                BoxCollider collider = afford.AddComponent<BoxCollider>();
                collider.size = bounds.size;
                collider.center = bounds.center;
                collider.size += colliderPadding;
            }
            else
            {
                SphereCollider sphere = afford.AddComponent<SphereCollider>();
                sphere.center = bounds.center;
                sphere.radius = bounds.extents.x;
                sphere.radius += GetMaxComponent(colliderPadding);
            }

            // In order for the affordance to be grabbed using near interaction we need
            // to add NearInteractionGrabbable;
            var g = afford.EnsureComponent<NearInteractionGrabbable>();
            g.ShowTetherWhenManipulating = drawTetherWhenManipulating;
            g.IsBoundsHandles = true;

            var contextInfo = afford.EnsureComponent<CursorContextInfo>();
            contextInfo.CurrentCursorAction = cursorType;
            contextInfo.ObjectCenter = parent;
        }

        /// <summary>
        /// Creates the default material for bounds control handles.
        /// </summary>
        internal static Material CreateDefaultMaterial()
        {
            return Resources.Load<Material>("BoundsControlHandleDefault");
        }

        /// <summary>
        /// Calculates an array of corner points out of the given bounds.
        /// </summary>
        /// <param name="bounds">Bounds of the box.</param>
        /// <param name="positions">Calculated corner points.</param>
        static internal void GetCornerPositionsFromBounds(Bounds bounds, ref Vector3[] positions)
        {
            const int numCorners = 1 << 3;
            if (positions == null || positions.Length != numCorners)
            {
                positions = new Vector3[numCorners];
            }

            // Permutate all axes using minCorner and maxCorner.
            Vector3 minCorner = bounds.center - bounds.extents;
            Vector3 maxCorner = bounds.center + bounds.extents;
            for (int c = 0; c < numCorners; c++)
            {
                positions[c] = new Vector3(
                    (c & (1 << 0)) == 0 ? minCorner[0] : maxCorner[0],
                    (c & (1 << 1)) == 0 ? minCorner[1] : maxCorner[1],
                    (c & (1 << 2)) == 0 ? minCorner[2] : maxCorner[2]);
            }
        }

        /// <summary>
        /// Flattens the given extents according to the passed flattenAxis. The flattenAxis value will be replaced by flattenValue.
        /// </summary>
        /// <param name="extents">The original extents (unflattened).</param>
        /// <param name="flattenAxis">The axis to flatten.</param>
        /// <param name="flattenValue">The value to flatten the flattenAxis to.</param>
        /// <returns>New extents with flattened axis.</returns>
        static internal Vector3 FlattenBounds(Vector3 extents, FlattenModeType flattenAxis, float flattenValue = 0.0f)
        {
            Vector3 boundsExtents = extents;
            if (boundsExtents != Vector3.zero)
            {
                if (flattenAxis == FlattenModeType.FlattenAuto)
                {
                    if (boundsExtents.x < boundsExtents.y && boundsExtents.x < boundsExtents.z)
                    {
                        flattenAxis = FlattenModeType.FlattenX;
                    }
                    else if (boundsExtents.y < boundsExtents.z)
                    {
                        flattenAxis = FlattenModeType.FlattenY;
                    }
                    else
                    {
                        flattenAxis = FlattenModeType.FlattenZ;
                    }
                }

                boundsExtents.x = (flattenAxis == FlattenModeType.FlattenX) ? flattenValue : boundsExtents.x;
                boundsExtents.y = (flattenAxis == FlattenModeType.FlattenY) ? flattenValue : boundsExtents.y;
                boundsExtents.z = (flattenAxis == FlattenModeType.FlattenZ) ? flattenValue : boundsExtents.z;
            }

            return boundsExtents;
        }

        /// <summary>
        /// Util function for retrieving a position for the given edge index of a box.
        /// This method makes sure all visual components are having the same definition of edges / corners.
        /// </summary>
        /// <param name="linkIndex">Index of the edge the position is queried for.</param>
        /// <param name="cornerPoints">Corner points array of the box.</param>
        /// <returns>Center position of link.</returns>
        static internal Vector3 GetLinkPosition(int linkIndex, ref Vector3[] cornerPoints)
        {
            Debug.Assert(cornerPoints != null && cornerPoints.Length == 8, "Invalid corner points array passed");
            if (cornerPoints != null && cornerPoints.Length == 8)
            {
                switch (linkIndex)
                {
                    case 0:
                        return (cornerPoints[0] + cornerPoints[1]) * 0.5f;
                    case 1:
                        return (cornerPoints[0] + cornerPoints[2]) * 0.5f;
                    case 2:
                        return (cornerPoints[3] + cornerPoints[2]) * 0.5f;
                    case 3:
                        return (cornerPoints[3] + cornerPoints[1]) * 0.5f;
                    case 4:
                        return (cornerPoints[4] + cornerPoints[5]) * 0.5f;
                    case 5:
                        return (cornerPoints[4] + cornerPoints[6]) * 0.5f;
                    case 6:
                        return (cornerPoints[7] + cornerPoints[6]) * 0.5f;
                    case 7:
                        return (cornerPoints[7] + cornerPoints[5]) * 0.5f;
                    case 8:
                        return (cornerPoints[0] + cornerPoints[4]) * 0.5f;
                    case 9:
                        return (cornerPoints[1] + cornerPoints[5]) * 0.5f;
                    case 10:
                        return (cornerPoints[2] + cornerPoints[6]) * 0.5f;
                    case 11:
                        return (cornerPoints[3] + cornerPoints[7]) * 0.5f;
                }
            }
            return Vector3.zero;
        }

        /// <summary>
        /// Util function for retrieving a position for the given face index of a box.
        /// This method makes sure all visual components are having the same definition of face centers.
        /// </summary>
        /// <param name="faceIndex">Index of the face the position is queried for.</param>
        /// <param name="cornerPoints">Corner points array of the box.</param>
        /// <returns>Center position of face.</returns>
        static internal Vector3 GetFaceCenterPosition(int faceIndex, ref Vector3[] cornerPoints)
        {
            Debug.Assert(cornerPoints != null && cornerPoints.Length == 8, "Invalid corner points array passed");
            if (cornerPoints != null && cornerPoints.Length == 8)
            {
                switch(faceIndex)
                {
                    case 0:
                        return (cornerPoints[0] + cornerPoints[2] + cornerPoints[4] + cornerPoints[6]) * 0.25f;
                    case 1:
                        return (cornerPoints[1] + cornerPoints[3] + cornerPoints[5] + cornerPoints[7]) * 0.25f;
                    case 2:
                        return (cornerPoints[0] + cornerPoints[1] + cornerPoints[2] + cornerPoints[3]) * 0.25f;
                    case 3:
                        return (cornerPoints[4] + cornerPoints[5] + cornerPoints[6] + cornerPoints[7]) * 0.25f;
                    case 4:
                        return (cornerPoints[0] + cornerPoints[1] + cornerPoints[4] + cornerPoints[5]) * 0.25f;
                    case 5:
                        return (cornerPoints[2] + cornerPoints[3] + cornerPoints[6] + cornerPoints[7]) * 0.25f;
                }
            }
            return Vector3.zero;
        }

        static readonly int[] flattenedIndicesX = new int[] { 0, 4, 2, 6 };
        static readonly int[] flattenedIndicesY = new int[] { 1, 3, 5, 7 };
        static readonly int[] flattenedIndicesZ = new int[] { 9, 10, 8, 11 };
        /// <summary>
        /// Returns the flatten indices to the corresponding flattenAxis mode.
        /// </summary>
        /// <param name="flattenAxis">Flatten axis mode that should be converted to indices.</param>
        /// <returns>Flattened indices.</returns>
        internal static int[] GetFlattenedIndices(FlattenModeType flattenAxis)
        {
            if (flattenAxis == FlattenModeType.FlattenX)
            {
                return flattenedIndicesX;
            }
            else if (flattenAxis == FlattenModeType.FlattenY)
            {
                return flattenedIndicesY;
            }
            else if (flattenAxis == FlattenModeType.FlattenZ)
            {
                return flattenedIndicesZ;
            }

            return null;
        }
    }
}
