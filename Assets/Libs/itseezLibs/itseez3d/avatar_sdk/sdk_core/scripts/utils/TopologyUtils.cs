/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017
*/

using UnityEngine;
using System.Collections.Generic;

namespace ItSeez3D.AvatarSdk.Core
{
	public static class TopologyUtils
	{
		public class Edge
		{
			public int v1, v2;

			public Edge (int vertexIdx1, int vertexIdx2)
			{
				Debug.Assert (vertexIdx1 != vertexIdx2);

				// let's keep them sorted
				if (vertexIdx1 < vertexIdx2) {
					v1 = vertexIdx1;
					v2 = vertexIdx2;
				} else {
					v2 = vertexIdx1;
					v1 = vertexIdx2;
				}
			}

			public static Edge Get (int vertexIdx1, int vertexIdx2)
			{
				return new Edge (vertexIdx1, vertexIdx2);
			}

			public bool Equals (Edge e)
			{
				if (e.v1 == v1 && e.v2 == v2) {
					Debug.Assert (e.GetHashCode () == GetHashCode ());
					return true;
				} else {
					Debug.Assert (e.GetHashCode () != GetHashCode ());
					return false;
				}
			}

			public override bool Equals (object o)
			{
				return this.Equals (o as Edge);
			}

			public override int GetHashCode ()
			{
				return (v1 << 16) | (v2);  // should be okay, right?
			}
		}

		public static List<List<Edge>> GetBorderEdgeLoops (Mesh mesh)
		{
			var borderEdgeLoops = new List<List<Edge>> ();

			var edgesTriangles = new Dictionary<Edge, int> ();
			var adjacency = new List<int>[mesh.vertices.Length];

			var vertices = mesh.vertices;
			for (int i = 0; i < vertices.Length; ++i)
				adjacency [i] = new List<int> ();

			var triangles = mesh.triangles;
			for (int i = 0; i < triangles.Length; i += 3) {
				var a = triangles [i];
				var b = triangles [i + 1];
				var c = triangles [i + 2];

				adjacency [a].Add (b);
				adjacency [a].Add (c);

				adjacency [b].Add (a);
				adjacency [b].Add (c);

				adjacency [c].Add (a);
				adjacency [c].Add (b);

				var e1 = Edge.Get (a, b);
				var e2 = Edge.Get (b, c);
				var e3 = Edge.Get (c, a);

				int value;
				edgesTriangles [e1] = edgesTriangles.TryGetValue (e1, out value) ? value + 1 : 1;
				edgesTriangles [e2] = edgesTriangles.TryGetValue (e2, out value) ? value + 1 : 1;
				edgesTriangles [e3] = edgesTriangles.TryGetValue (e3, out value) ? value + 1 : 1;
			}

			var edgesVisited = new HashSet<Edge> ();
			foreach (var item in edgesTriangles) {
				var edge = item.Key;
				var numTrianglesForEdge = item.Value;
				if (numTrianglesForEdge <= 0 || numTrianglesForEdge > 2) {
					Debug.Assert (numTrianglesForEdge > 0 && numTrianglesForEdge <= 2);  // don't expect ill-formed meshes
				}

				// edges with one adjacent triangle are "border" edges
				if (numTrianglesForEdge != 1)
					continue;

				if (edgesVisited.Contains (edge))
					continue;

				// don't care what direction we go, let's choose v1
				int startingVertex = edge.v1;
				int currentEdgeLoopVertex = startingVertex;
				Edge currentEdge;

				List<Edge> border = new List<Edge> ();

				do {
					currentEdge = null;
					foreach (var adjacentVertex in adjacency[currentEdgeLoopVertex]) {
						var loopEdge = Edge.Get (currentEdgeLoopVertex, adjacentVertex);
						if (edgesVisited.Contains (loopEdge))
							continue;
						if (edgesTriangles [loopEdge] != 1)
							continue;

						currentEdge = loopEdge;
						currentEdgeLoopVertex = adjacentVertex;
						break;
					}

					if (currentEdge == null) {
						// could not find the next border edge
						break;
					}

					edgesVisited.Add (currentEdge);
					border.Add (currentEdge);
				} while (currentEdgeLoopVertex != startingVertex);

				borderEdgeLoops.Add (border);
			}

			return borderEdgeLoops;
		}
	}
}

