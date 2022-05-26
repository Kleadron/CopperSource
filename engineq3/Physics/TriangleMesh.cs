using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KSoft.Client.Physics //XboxTest.General
{
    public class TriangleMesh
    {
        Vector3[] vertices;
        int[] indices;

        public TriangleMesh(Vector3[] vertices, int[] indices)
        {
            this.vertices = vertices;
            this.indices = indices;
        }

        public float? CheckRay(Ray ray)
        {
            float? result = null;

            for (int i = 0; i < indices.Length; i += 3)
            {
                float? localResult;
                PhysUtil.RayIntersectsTriangle(ref ray,
                    ref vertices[indices[i]],
                    ref vertices[indices[i + 1]],
                    ref vertices[indices[i + 2]],
                    out localResult);

                if (result != null)
                {
                    if (localResult != null)
                    {
                        if (localResult < result)
                        {
                            result = localResult;
                        }
                    }
                }
                else
                {
                    if (localResult != null)
                    {
                        result = localResult;
                    }
                }
            }

            return result;
        }

        public bool CheckRay2(Ray ray, out RayResult result)
        {
            result = new RayResult();

            int closestResultIndex = -1;
            float? closestResultDistance = null;

            for (int i = 0; i < indices.Length; i += 3)
            {
                float? localResult;
                PhysUtil.RayIntersectsTriangle(ref ray,
                    ref vertices[indices[i]],
                    ref vertices[indices[i + 1]],
                    ref vertices[indices[i + 2]],
                    out localResult);

                if (closestResultDistance != null)
                {
                    if (localResult != null)
                    {
                        if (localResult < closestResultDistance)
                        {
                            closestResultDistance = localResult;
                            closestResultIndex = i;
                        }
                    }
                }
                else
                {
                    if (localResult != null)
                    {
                        closestResultDistance = localResult;
                        closestResultIndex = i;
                    }
                }
            }

            if (closestResultIndex != -1)
            {
                result.distance = (float)closestResultDistance;
                //result.hitPosition = ray.Position + (ray.Direction * result.distance);

                Vector3 firstvec = vertices[indices[closestResultIndex + 1]] - vertices[indices[closestResultIndex]];
                Vector3 secondvec = vertices[indices[closestResultIndex]] - vertices[indices[closestResultIndex + 2]];
                Vector3 normal = Vector3.Cross(firstvec, secondvec);
                normal.Normalize();

                result.surfaceNormal = normal;
                return true;
            }
            return false;
        }
    }
}
