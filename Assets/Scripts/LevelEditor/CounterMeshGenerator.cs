using System.Collections.Generic;
using UnityEngine;

namespace TinyChef.LevelEditor
{
    public static class CounterMeshGenerator
    {
        // Builds a single wedge that is 360/segmentCount of a ring.
        // The mesh contains two submeshes:
        //   submesh 0 = base slab (from y=0 to y=baseHeight)
        //   submesh 1 = top slab  (from y=baseHeight to y=baseHeight+topThickness)
        public static Mesh BuildWedge(CounterProfile profile, int segmentCount)
        {
            if (segmentCount < 3) return null;
            float inner = Mathf.Max(0.0001f, profile.innerRadius);
            float outerBase = Mathf.Max(inner + 0.0001f, profile.outerRadius);
            float outerTop = outerBase + profile.topOverhangOuter;
            float innerTop = Mathf.Max(0.0001f, inner - profile.topOverhangInner);

            // Apply radial gap (shrink outer, grow inner)
            if (profile.radialGap > 0f)
            {
                outerBase = Mathf.Max(inner + 0.0001f, outerBase - profile.radialGap);
                outerTop = Mathf.Max(innerTop + 0.0001f, outerTop - profile.radialGap);
                inner = Mathf.Max(0.0001f, inner + profile.radialGap);
                innerTop = Mathf.Max(0.0001f, innerTop + profile.radialGap);
            }
            float hBase = Mathf.Max(0.0001f, profile.baseHeight);
            float hTop = Mathf.Max(0.0001f, profile.topThickness);

            float fullStepDeg = 360f / segmentCount;
            float spanDeg = Mathf.Max(0f, fullStepDeg - profile.angleGapDegrees);
            float angleOffsetDeg = profile.flatTopOrientation ? -spanDeg * 0.5f : 0f;
            float a0 = Mathf.Deg2Rad * (angleOffsetDeg);
            float a1 = Mathf.Deg2Rad * (angleOffsetDeg + spanDeg);

            // Base slab prism (submesh 0)
            var baseVerts = new List<Vector3>();
            var baseUVs = new List<Vector2>();
            var baseTris = new List<int>();

            // Top slab prism (submesh 1)
            var topVerts = new List<Vector3>();
            var topUVs = new List<Vector2>();
            var topTris = new List<int>();

            // Helper to add a quad as two triangles (v0,v1,v2,v3) with Unity's default clockwise front-face
            void AddQuad(List<int> tris, int i0, int i1, int i2, int i3)
            {
                tris.Add(i0); tris.Add(i2); tris.Add(i1);
                tris.Add(i0); tris.Add(i3); tris.Add(i2);
            }

            // Compute ring wedge 2D points on XZ
            Vector2 o0 = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * outerBase;
            Vector2 o1 = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * outerBase;
            Vector2 i0 = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * inner;
            Vector2 i1 = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * inner;

            Vector2 ot0 = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * outerTop;
            Vector2 ot1 = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * outerTop;
            Vector2 it0 = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * innerTop;
            Vector2 it1 = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * innerTop;

            // Build base slab (five quads: outer, inner, sideA, sideB, top cap, bottom cap)
            // Outer face (CCW when viewed from outside)
            int bvi = baseVerts.Count;
            baseVerts.Add(new Vector3(o0.x, 0f, o0.y));
            baseVerts.Add(new Vector3(o1.x, 0f, o1.y));
            baseVerts.Add(new Vector3(o1.x, hBase, o1.y));
            baseVerts.Add(new Vector3(o0.x, hBase, o0.y));
            baseUVs.AddRange(new[]
            {
                new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)
            });
            AddQuad(baseTris, bvi, bvi + 1, bvi + 2, bvi + 3);

            // Inner face (CCW when viewed from inside)
            bvi = baseVerts.Count;
            baseVerts.Add(new Vector3(i1.x, 0f, i1.y));
            baseVerts.Add(new Vector3(i0.x, 0f, i0.y));
            baseVerts.Add(new Vector3(i0.x, hBase, i0.y));
            baseVerts.Add(new Vector3(i1.x, hBase, i1.y));
            baseUVs.AddRange(new[]
            {
                new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)
            });
            AddQuad(baseTris, bvi, bvi + 1, bvi + 2, bvi + 3);

            // Side A (radial at angle a0)
            bvi = baseVerts.Count;
            baseVerts.Add(new Vector3(i0.x, 0f, i0.y));
            baseVerts.Add(new Vector3(o0.x, 0f, o0.y));
            baseVerts.Add(new Vector3(o0.x, hBase, o0.y));
            baseVerts.Add(new Vector3(i0.x, hBase, i0.y));
            baseUVs.AddRange(new[]
            {
                new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)
            });
            AddQuad(baseTris, bvi, bvi + 1, bvi + 2, bvi + 3);

            // Side B (radial at angle a1)
            bvi = baseVerts.Count;
            baseVerts.Add(new Vector3(o1.x, 0f, o1.y));
            baseVerts.Add(new Vector3(i1.x, 0f, i1.y));
            baseVerts.Add(new Vector3(i1.x, hBase, i1.y));
            baseVerts.Add(new Vector3(o1.x, hBase, o1.y));
            baseUVs.AddRange(new[]
            {
                new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)
            });
            AddQuad(baseTris, bvi, bvi + 1, bvi + 2, bvi + 3);

            // Top cap of base
            bvi = baseVerts.Count;
            baseVerts.Add(new Vector3(i0.x, hBase, i0.y));
            baseVerts.Add(new Vector3(i1.x, hBase, i1.y));
            baseVerts.Add(new Vector3(o1.x, hBase, o1.y));
            baseVerts.Add(new Vector3(o0.x, hBase, o0.y));
            // Planar UVs based on XZ
            baseUVs.Add(new Vector2(i0.x, i0.y));
            baseUVs.Add(new Vector2(i1.x, i1.y));
            baseUVs.Add(new Vector2(o1.x, o1.y));
            baseUVs.Add(new Vector2(o0.x, o0.y));
            AddQuad(baseTris, bvi, bvi + 1, bvi + 2, bvi + 3);

            // Bottom cap of base
            bvi = baseVerts.Count;
            baseVerts.Add(new Vector3(o0.x, 0f, o0.y));
            baseVerts.Add(new Vector3(o1.x, 0f, o1.y));
            baseVerts.Add(new Vector3(i1.x, 0f, i1.y));
            baseVerts.Add(new Vector3(i0.x, 0f, i0.y));
            baseUVs.Add(new Vector2(o0.x, o0.y));
            baseUVs.Add(new Vector2(o1.x, o1.y));
            baseUVs.Add(new Vector2(i1.x, i1.y));
            baseUVs.Add(new Vector2(i0.x, i0.y));
            AddQuad(baseTris, bvi, bvi + 1, bvi + 2, bvi + 3);

            // Build top slab: same as base but heights offset and outer radius may include overhang
            Vector2 to0 = ot0;
            Vector2 to1 = ot1;
            Vector2 ti0 = it0;
            Vector2 ti1 = it1;
            float y0 = hBase;
            float y1 = hBase + hTop;

            // Outer face
            int tvi = topVerts.Count;
            topVerts.Add(new Vector3(to0.x, y0, to0.y));
            topVerts.Add(new Vector3(to1.x, y0, to1.y));
            topVerts.Add(new Vector3(to1.x, y1, to1.y));
            topVerts.Add(new Vector3(to0.x, y1, to0.y));
            topUVs.AddRange(new[]
            {
                new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)
            });
            AddQuad(topTris, tvi, tvi + 1, tvi + 2, tvi + 3);

            // Inner face
            tvi = topVerts.Count;
            topVerts.Add(new Vector3(ti1.x, y0, ti1.y));
            topVerts.Add(new Vector3(ti0.x, y0, ti0.y));
            topVerts.Add(new Vector3(ti0.x, y1, ti0.y));
            topVerts.Add(new Vector3(ti1.x, y1, ti1.y));
            topUVs.AddRange(new[]
            {
                new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)
            });
            AddQuad(topTris, tvi, tvi + 1, tvi + 2, tvi + 3);

            // Side A
            tvi = topVerts.Count;
            topVerts.Add(new Vector3(ti0.x, y0, ti0.y));
            topVerts.Add(new Vector3(to0.x, y0, to0.y));
            topVerts.Add(new Vector3(to0.x, y1, to0.y));
            topVerts.Add(new Vector3(ti0.x, y1, ti0.y));
            topUVs.AddRange(new[]
            {
                new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)
            });
            AddQuad(topTris, tvi, tvi + 1, tvi + 2, tvi + 3);

            // Side B
            tvi = topVerts.Count;
            topVerts.Add(new Vector3(to1.x, y0, to1.y));
            topVerts.Add(new Vector3(ti1.x, y0, ti1.y));
            topVerts.Add(new Vector3(ti1.x, y1, ti1.y));
            topVerts.Add(new Vector3(to1.x, y1, to1.y));
            topUVs.AddRange(new[]
            {
                new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)
            });
            AddQuad(topTris, tvi, tvi + 1, tvi + 2, tvi + 3);

            // Prepare bevel parameters and segments along Y
            float by0 = y1;
            float outerBevelR = outerTop;
            float innerBevelR = innerTop;
            int bevelSegs = Mathf.Max(0, profile.bevelSegments);
            if (bevelSegs >= 1 && profile.bevelSize > 0f)
            {
                by0 = y1 - Mathf.Min(profile.bevelSize, hTop * 0.99f);
                outerBevelR = Mathf.Max(0.0001f, outerTop - profile.bevelSize);
                innerBevelR = Mathf.Max(0.0001f, innerTop + profile.bevelSize);
            }

            // Build multi-segment beveled ring surfaces (outer and inner), then add top cap at by0
            if (bevelSegs >= 1 && profile.bevelSize > 0f)
            {
                for (int s = 0; s < bevelSegs; s++)
                {
                    float t0 = (float)s / bevelSegs;
                    float t1 = (float)(s + 1) / bevelSegs;
                    float yA = Mathf.Lerp(y1, by0, t0);
                    float yB = Mathf.Lerp(y1, by0, t1);
                    float rOutA = Mathf.Lerp(outerTop, outerBevelR, t0);
                    float rOutB = Mathf.Lerp(outerTop, outerBevelR, t1);
                    float rInA = Mathf.Lerp(innerTop, innerBevelR, t0);
                    float rInB = Mathf.Lerp(innerTop, innerBevelR, t1);

                    // Outer bevel strip (CCW winding facing outward)
                    Vector2 oA0 = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * rOutA;
                    Vector2 oA1 = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * rOutA;
                    Vector2 oB0 = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * rOutB;
                    Vector2 oB1 = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * rOutB;
                    tvi = topVerts.Count;
                    topVerts.Add(new Vector3(oA0.x, yA, oA0.y));
                    topVerts.Add(new Vector3(oA1.x, yA, oA1.y));
                    topVerts.Add(new Vector3(oB1.x, yB, oB1.y));
                    topVerts.Add(new Vector3(oB0.x, yB, oB0.y));
                    topUVs.AddRange(new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up });
                    AddQuad(topTris, tvi, tvi + 1, tvi + 2, tvi + 3);

                    // Inner bevel strip (CCW winding facing inward hole)
                    Vector2 iA0 = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * rInA;
                    Vector2 iA1 = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * rInA;
                    Vector2 iB0 = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * rInB;
                    Vector2 iB1 = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * rInB;
                    tvi = topVerts.Count;
                    topVerts.Add(new Vector3(iA0.x, yA, iA0.y));
                    topVerts.Add(new Vector3(iA1.x, yA, iA1.y));
                    topVerts.Add(new Vector3(iB1.x, yB, iB1.y));
                    topVerts.Add(new Vector3(iB0.x, yB, iB0.y));
                    topUVs.AddRange(new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up });
                    AddQuad(topTris, tvi, tvi + 1, tvi + 2, tvi + 3);
                }
            }

            // Top cap at beveled ring height
            Vector2 tob0 = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * outerBevelR;
            Vector2 tob1 = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * outerBevelR;
            Vector2 tib0 = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * innerBevelR;
            Vector2 tib1 = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * innerBevelR;
            tvi = topVerts.Count;
            topVerts.Add(new Vector3(tib0.x, by0, tib0.y));
            topVerts.Add(new Vector3(tib1.x, by0, tib1.y));
            topVerts.Add(new Vector3(tob1.x, by0, tob1.y));
            topVerts.Add(new Vector3(tob0.x, by0, tob0.y));
            topUVs.Add(new Vector2(tib0.x, tib0.y));
            topUVs.Add(new Vector2(tib1.x, tib1.y));
            topUVs.Add(new Vector2(tob1.x, tob1.y));
            topUVs.Add(new Vector2(tob0.x, tob0.y));
            AddQuad(topTris, tvi, tvi + 1, tvi + 2, tvi + 3);

            // Bottom cap of top slab
            tvi = topVerts.Count;
            topVerts.Add(new Vector3(to0.x, y0, to0.y));
            topVerts.Add(new Vector3(to1.x, y0, to1.y));
            topVerts.Add(new Vector3(ti1.x, y0, ti1.y));
            topVerts.Add(new Vector3(ti0.x, y0, ti0.y));
            topUVs.Add(new Vector2(to0.x, to0.y));
            topUVs.Add(new Vector2(to1.x, to1.y));
            topUVs.Add(new Vector2(ti1.x, ti1.y));
            topUVs.Add(new Vector2(ti0.x, ti0.y));
            AddQuad(topTris, tvi, tvi + 1, tvi + 2, tvi + 3);

            // Optional simple bevel on top slab edges (single chamfer when bevelSegments >= 1)
            if (profile.bevelSegments >= 1 && profile.bevelSize > 0f)
            {
                // Outer bevel ring: connect (outerBevelR @ by0) to (outerTop @ y1)
                tvi = topVerts.Count;
                topVerts.Add(new Vector3(tob0.x, by0, tob0.y));
                topVerts.Add(new Vector3(tob1.x, by0, tob1.y));
                topVerts.Add(new Vector3(ot1.x, y1, ot1.y));
                topVerts.Add(new Vector3(ot0.x, y1, ot0.y));
                topUVs.AddRange(new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up });
                AddQuad(topTris, tvi, tvi + 1, tvi + 2, tvi + 3);

                // Inner bevel ring: connect (innerTop @ y1) to (innerBevelR @ by0)
                tvi = topVerts.Count;
                topVerts.Add(new Vector3(it0.x, y1, it0.y));
                topVerts.Add(new Vector3(it1.x, y1, it1.y));
                topVerts.Add(new Vector3(tib1.x, by0, tib1.y));
                topVerts.Add(new Vector3(tib0.x, by0, tib0.y));
                topUVs.AddRange(new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up });
                AddQuad(topTris, tvi, tvi + 1, tvi + 2, tvi + 3);
            }

            // Assemble mesh with two submeshes
            var mesh = new Mesh();
            mesh.name = $"Counter_Wedge";

            // Combine vertex streams
            var verts = new List<Vector3>(baseVerts.Count + topVerts.Count);
            verts.AddRange(baseVerts);
            verts.AddRange(topVerts);
            var uvs = new List<Vector2>(baseUVs.Count + topUVs.Count);
            uvs.AddRange(baseUVs);
            uvs.AddRange(topUVs);

            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);

            // Submesh 0 = base
            mesh.subMeshCount = 2;
            if (profile.doubleSided)
            {
                var baseDoubled = new List<int>(baseTris.Count * 2);
                baseDoubled.AddRange(baseTris);
                // reversed copy
                for (int t = 0; t < baseTris.Count; t += 3)
                {
                    baseDoubled.Add(baseTris[t]);
                    baseDoubled.Add(baseTris[t + 2]);
                    baseDoubled.Add(baseTris[t + 1]);
                }
                mesh.SetTriangles(baseDoubled, 0, true);
            }
            else
            {
                mesh.SetTriangles(baseTris, 0, true);
            }

            // Submesh 1 = top (offset triangle indices by base vertex count)
            int baseVertexCount = baseVerts.Count;
            var topTrisOffset = new int[topTris.Count];
            for (int t = 0; t < topTris.Count; t++)
            {
                topTrisOffset[t] = topTris[t] + baseVertexCount;
            }
            if (profile.doubleSided)
            {
                var topDoubled = new List<int>(topTrisOffset.Length * 2);
                topDoubled.AddRange(topTrisOffset);
                for (int t = 0; t < topTrisOffset.Length; t += 3)
                {
                    topDoubled.Add(topTrisOffset[t]);
                    topDoubled.Add(topTrisOffset[t + 2]);
                    topDoubled.Add(topTrisOffset[t + 1]);
                }
                mesh.SetTriangles(topDoubled, 1, true);
            }
            else
            {
                mesh.SetTriangles(topTrisOffset, 1, true);
            }

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}


