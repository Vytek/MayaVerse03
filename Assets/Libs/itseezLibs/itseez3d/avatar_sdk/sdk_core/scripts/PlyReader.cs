/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ItSeez3D.AvatarSdk.Core
{
	/// <summary>
	/// Collection of utilities for .ply 3D mesh format.
	/// </summary>
	public static class PlyReader
	{
		/// <summary>
		/// In .ply uv-coordinates are stored per face (three pairs of coordinates for each of three vertices of the
		/// triangle.
		/// </summary>
		public struct FaceUv
		{
			public Vector2[] uv;
		}

		/// <summary>
		/// Read text header into list of strings using binary reader. A little messy, but works.
		/// Better than mixing binary and text readers.
		/// </summary>
		/// <returns>List of ply header lines.</returns>
		private static List<string> ReadHeaderLines (BinaryReader reader)
		{
			List<string> lines = new List<string> ();
			List<byte> lineBytes = new List<byte> ();

			while (true) {
				var b = reader.ReadByte ();
				if (b == '\n') {
					var line = Encoding.UTF8.GetString (lineBytes.ToArray ());
					lineBytes.Clear ();

					line = line.Trim (new char[]{ '\r', '\n' });
					lines.Add (line);

					if (line == "end_header")
						break;
				} else {
					lineBytes.Add (b);
				}
			}

			return lines;
		}

		/// <summary>
		/// Parses the .ply header.
		/// Note: this is not a generic .ply reader, it supports only simple meshes that come from itSeez3D SDK.
		/// E.g. normals or vertex colors are not supported.
		/// </summary>
		/// <param name="header">.ply header lines.</param>
		/// <param name="numVertices">Output: number of vertices in 3D mesh.</param>
		/// <param name="numFaces">Output: number of faces.</param>
		public static void ParseHeader (List<string> header, out int numVertices, out int numFaces)
		{
			numVertices = numFaces = 0;
			foreach (var line in header) {
				var tokens = line.Split (' ');
				if (line.StartsWith ("element vertex"))
					numVertices = int.Parse (tokens [tokens.Length - 1]);
				else if (line.StartsWith ("element face"))
					numFaces = int.Parse (tokens [tokens.Length - 1]);
			}
			Debug.LogFormat ("Ply header vertices: {0}, faces: {1}", numVertices, numFaces);
		}

		/// <summary>
		/// Reads the data from .ply format.
		/// </summary>
		/// <param name="reader">.NET Binary reader.</param>
		/// <param name="vertices">Array of vertices.</param>
		/// <param name="triangles">Triangles, just like in Unity's mesh.</param>
		/// <param name="faceUv">Uv-coordinates per face (see FaceUv comments).</param>
		/// <param name="useLeftHandedCoordinates">In case if Left-Handed coordinate system is used in 3D viewer (should always be true for Unity)</param>
		public static void ReadData (
			BinaryReader reader, Vector3[] vertices, int[] triangles = null, FaceUv[] faceUv = null, bool useLeftHandedCoordinates = true
		)
		{
			for (int i = 0; i < vertices.Length; ++i) {
				vertices [i] = new Vector3 ((-1.0f) * reader.ReadSingle (), reader.ReadSingle (), reader.ReadSingle ());
				if (!useLeftHandedCoordinates)
					vertices [i].x *= -1.0f;
			}

			if (triangles != null && faceUv != null) {
				int numFaces = triangles.Length / 3;
				for (int faceIdx = 0; faceIdx < numFaces; ++faceIdx) {
					int numFaceVertices = reader.ReadByte ();  // must be 3
					if (useLeftHandedCoordinates) {
						for (int j = numFaceVertices - 1; j >= 0; --j)
							triangles [faceIdx * 3 + j] = reader.ReadInt32 ();
					} else {
						for (int j = 0; j < numFaceVertices; ++j)
							triangles [faceIdx * 3 + j] = reader.ReadInt32 ();
					}

					reader.ReadByte ();  // number of uv coords, must be 6

					faceUv [faceIdx].uv = new Vector2[3];
					if (useLeftHandedCoordinates) {
						for (int j = numFaceVertices - 1; j >= 0; --j)
							faceUv [faceIdx].uv [j] = new Vector2 (reader.ReadSingle (), reader.ReadSingle ());
					} else {
						for (int j = 0; j < numFaceVertices; ++j)
							faceUv [faceIdx].uv [j] = new Vector2 (reader.ReadSingle (), reader.ReadSingle ());
					}
				}
			}
		}

		/// <summary>
		/// Ply stores uv coordinates per face while Unity works with one uv per vertex.
		/// This function converts between these representations.
		/// </summary>
		public static void ConvertToUnityFormat (
			Vector3[] originalVertices,
			int[] triangles,
			FaceUv[] faceUv,
			out Vector3[] vertices,
			out Vector2[] uv,
			out int[] indexMap
		)
		{
			// If different uv coordinates correspond to single vertex we need to
			// duplicate this vertex in order to comply with Unity mesh format.

			// originalVertcies may contain vertices that doesn't belong to any faces,
			// so maximum number of vertcices after transformation is number of faces * 3 (which is triangles.Length) + originalVertcies.Length
			int maxVerticesCount = triangles.Length + originalVertices.Length;

			// This array holds indices of created duplicates.
			int[] duplicate = new int[maxVerticesCount];
			for (int i = 0; i < duplicate.Length; ++i)
				duplicate [i] = -1;

			// each vertex in each face has unique uv coord pair
			Vector2[] vertexUv = new Vector2[maxVerticesCount];
			Vector2 uninitialized = new Vector2 (-1, -1);
			for (int i = 0; i < vertexUv.Length; ++i)
				vertexUv [i] = uninitialized;

			int numFaces = triangles.Length / 3, numVertices = originalVertices.Length;
			for (int faceIdx = 0; faceIdx < numFaces; ++faceIdx) {
				for (int j = 0; j < 3; ++j) {
					int vertexIdx = triangles [faceIdx * 3 + j];
					Vector2 currentUv = faceUv [faceIdx].uv [j];

					// Iterate over duplicates of this vertex until we find copy with exact same uv.
					// Create new duplicate vertex if none were found.
					while (vertexUv [vertexIdx] != uninitialized && vertexUv [vertexIdx] != currentUv) {
						if (duplicate [vertexIdx] == -1)
							duplicate [vertexIdx] = numVertices++;  // "allocate" new vertex and save link to it
						vertexIdx = duplicate [vertexIdx];
					}

					vertexUv [vertexIdx] = currentUv;
					triangles [faceIdx * 3 + j] = vertexIdx;
				}
			}

			vertices = new Vector3[numVertices];
			indexMap = new int[numVertices];
			originalVertices.CopyTo (vertices, 0);
			for (int i = 0; i < originalVertices.Length; ++i)
				indexMap [i] = i;

			for (int i = 0; i < originalVertices.Length; ++i) {
				var duplicateIdx = duplicate [i];
				while (duplicateIdx != -1) {
					vertices [duplicateIdx] = vertices [i];
					indexMap [duplicateIdx] = i;
					Debug.AssertFormat (
						indexMap [duplicateIdx] < originalVertices.Length,
						"Original index must be less than {0} but is actually {1}",
						originalVertices.Length, indexMap [duplicateIdx]
					);

					duplicateIdx = duplicate [duplicateIdx];
				}
			}

			uv = new Vector2[numVertices];
			Array.Copy (vertexUv, uv, numVertices);

			Debug.LogFormat ("Before transformation: {0} vertices, after: {1} vertices", originalVertices.Length, vertices.Length);
		}

		/// <summary>
		/// Reads the mesh data from ply into Unity mesh format.
		/// </summary>
		/// <param name="plyPath">Path to the .ply file.</param>
		/// <param name="vertices">Output: vertices.</param>
		/// <param name="triangles">Output: triangles.</param>
		/// <param name="uv">Output: uv coordinates per vertex.</param>
		/// <param name="indexMap">Output: conversion of uv coordinates requires duplicating points. The mapping
		/// between the original indices and vertex indices in the final mesh is stored in this array.</param>
		public static void ReadMeshDataFromPly(string plyPath, out Vector3[] vertices, out int[] triangles, out Vector2[] uv, out int[] indexMap)
		{
			using (var stream = new FileStream(plyPath, FileMode.Open, FileAccess.Read))
				ReadMeshDataFromPly(stream, out vertices, out triangles, out uv, out indexMap);
		}

		/// <summary>
		/// Reads the mesh data from ply into Unity mesh format.
		/// </summary>
		/// <param name="plyBytes">Bytes of the original .ply file.</param>
		/// <param name="vertices">Output: vertices.</param>
		/// <param name="triangles">Output: triangles.</param>
		/// <param name="uv">Output: uv coordinates per vertex.</param>
		/// <param name="indexMap">Output: conversion of uv coordinates requires duplicating points. The mapping
		/// between the original indices and vertex indices in the final mesh is stored in this array.</param>
		public static void ReadMeshDataFromPly(byte[] plyBytes, out Vector3[] vertices, out int[] triangles, out Vector2[] uv, out int[] indexMap)
		{
			using (var stream = new MemoryStream(plyBytes))
				ReadMeshDataFromPly(stream, out vertices, out triangles, out uv, out indexMap);
		}

		/// <summary>
		/// Reads the mesh data from ply into Unity mesh format.
		/// </summary>
		/// <param name="inputStream">Stream to read ply data.</param>
		/// <param name="vertices">Output: vertices.</param>
		/// <param name="triangles">Output: triangles.</param>
		/// <param name="uv">Output: uv coordinates per vertex.</param>
		/// <param name="indexMap">Output: conversion of uv coordinates requires duplicating points. The mapping
		/// between the original indices and vertex indices in the final mesh is stored in this array.</param>
		private static void ReadMeshDataFromPly(Stream inputStream, out Vector3[] vertices, out int[] triangles, out Vector2[] uv, out int[] indexMap)
		{
			Vector3[] originalVertices;
			FaceUv[] faceUv;

			using (var reader = new BinaryReader(inputStream))
			{
				var headerLines = ReadHeaderLines(reader);

				int numVertices, numFaces;
				ParseHeader(headerLines, out numVertices, out numFaces);

				originalVertices = new Vector3[numVertices];
				triangles = new int[numFaces * 3];
				faceUv = new FaceUv[numFaces];

				ReadData(reader, originalVertices, triangles, faceUv);
			}

			ConvertToUnityFormat(originalVertices, triangles, faceUv, out vertices, out uv, out indexMap);
		}

		/// <summary>
		/// Same as ReadMeshDataFromPly, but reads only 3D point coordinates.
		/// </summary>
		public static void ReadPointCloudFromPly (byte[] plyBytes, out Vector3[] points)
		{
			using (var stream = new MemoryStream (plyBytes)) {
				using (var reader = new BinaryReader (stream)) {
					var headerLines = ReadHeaderLines (reader);

					int numPoints, notNeeded;
					ParseHeader (headerLines, out numPoints, out notNeeded);

					points = new Vector3[numPoints];
					ReadData (reader, points);
				}
			}
		}
	}
}