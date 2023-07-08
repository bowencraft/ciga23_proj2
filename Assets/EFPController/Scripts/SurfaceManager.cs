using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace EFPController
{

	public class SurfaceManager : MonoBehaviour 
	{

		[System.Serializable]
		public struct RegisteredMaterial 
		{
			public int surfaceIndex;
			public Texture[] textures;
		}

		public static SurfaceManager instance => m_instance;
		private static SurfaceManager m_instance;

		public RegisteredMaterial[] registeredTextures;

		private void Awake()
		{
			if (m_instance != null)
			{
				Destroy(gameObject);
				return;
			}
			m_instance = this;
		}

		private void OnDestroy()
		{
			if (m_instance == this)
			{
				m_instance = null;
			}
		}

		public int GetSurfaceIndex(Ray ray, Collider col, Vector3 worldPos)
		{
			string textureName = string.Empty;

			if(col.GetType() == typeof(TerrainCollider)) {
				Terrain terrain = col.GetComponent<Terrain>();
				TerrainData terrainData = terrain.terrainData;
				float[] textureMix = GetTerrainTextureMix(worldPos, terrainData, terrain.GetPosition());
				int textureIndex = GetTextureIndex(textureMix);
				textureName = terrainData.terrainLayers[textureIndex].diffuseTexture.name;
			} else {
				textureName = GetMeshMaterialAtPoint(worldPos, ray);
			}

			IEnumerable<int> all = registeredTextures.Where(x => x.textures.Any(y => y.name == textureName)).Select(x => x.surfaceIndex);
			if (all.Count() > 0)
			{
				return all.First();
			} else {
				return -1;
			}
		}

		public int GetSurfaceIndex(Collider col, Vector3 worldPos)
		{
			string textureName = "";

			if(col.GetType() == typeof(TerrainCollider)) {
				Terrain terrain = col.GetComponent<Terrain>();
				TerrainData terrainData = terrain.terrainData;
				float[] textureMix = GetTerrainTextureMix(worldPos, terrainData, terrain.GetPosition());
				int textureIndex = GetTextureIndex(textureMix);
				textureName = terrainData.terrainLayers[textureIndex].diffuseTexture.name;
			} else {
				textureName = GetMeshMaterialAtPoint(worldPos, new Ray(Vector3.zero, Vector3.zero));
			}

			IEnumerable<int> all = registeredTextures.Where(x => x.textures.Any(y => y.name == textureName)).Select(x => x.surfaceIndex);
			if (all.Count() > 0)
			{
				return all.First();
			} else {
				return -1;
			}
		}

		string GetMeshMaterialAtPoint(Vector3 worldPosition, Ray ray)
		{
			if(ray.direction == Vector3.zero) 
			{
				ray = new Ray(worldPosition + Vector3.up * 0.01f, Vector3.down);
			}

			RaycastHit hit;

			if (!Physics.Raycast(ray, out hit)) return string.Empty;

			Renderer r = hit.collider.GetComponent<Renderer>();
			MeshCollider mc = hit.collider as MeshCollider;

			if (r == null || r.sharedMaterial == null || r.sharedMaterial.mainTexture == null)
			{
				return string.Empty;
			} else if(!mc || mc.convex) {
				return r.material.mainTexture.name;
			}

			int materialIndex = -1;
			Mesh m = mc.sharedMesh;

			if (!m.isReadable)
			{
				return string.Empty;
			}

			int triangleIdx = hit.triangleIndex;
			int lookupIdx1 = m.triangles[triangleIdx * 3];
			int lookupIdx2 = m.triangles[triangleIdx * 3 + 1];
			int lookupIdx3 = m.triangles[triangleIdx * 3 + 2];
			int subMeshesNr = m.subMeshCount;

			for(int i = 0;i < subMeshesNr;i ++) 
			{
				int[] tr = m.GetTriangles(i);
				for(int j = 0;j < tr.Length;j += 3)
				{
					if (tr[j] == lookupIdx1 && tr[j+1] == lookupIdx2 && tr[j+2] == lookupIdx3)
					{
						materialIndex = i;
						break;
					}
				}

				if (materialIndex != -1) break;
			}

			string textureName = r.materials[materialIndex].mainTexture.name;

			return textureName;
		}

		float[] GetTerrainTextureMix(Vector3 worldPos, TerrainData terrainData, Vector3 terrainPos)
		{
			int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
			int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);
			float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);
			float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];
			for(int n = 0;n < cellMix.Length;n++) {
				cellMix[n] = splatmapData[0, 0, n];
			}
			return cellMix;
		}

		int GetTextureIndex(float[] textureMix)
		{
			float maxMix = 0;
			int maxIndex = 0;
			for(int n = 0; n < textureMix.Length; n++)
			{
				if (textureMix[n] > maxMix)
				{
					maxIndex = n;
					maxMix = textureMix[n];
				}
			}
			return maxIndex;
		}

	}

}