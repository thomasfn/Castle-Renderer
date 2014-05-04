#include <iostream>
#include <fstream>
#include <vector>
#include <map>

#include <fbxsdk.h>
#include "SBM.hpp"

using namespace std;

void pause();

template <class T1, class T2>
bool ReverseLookup(map<T1, T2> themap, T2 obj, T1 *key)
{
	for (map<T1, T2>::iterator it = themap.begin(); it != themap.end(); it++)
	{
		if (it->second == obj)
		{
			key = it->first;
			return true;
		}
	}
	return false;
}

int main(int argc, char **argv)
{
	// Header
	cout << "FBX to SBM converter" << endl;

	// Check file
	if (argc <= 1)
	{
		cout << "ERROR: Filename not specified!" << endl;
		pause();
		return 0;
	}

	// Validate file
	const char* filename = argv[1];
	//cout << filename << endl;
	{
		ifstream strm;
		strm.open(filename);
		if (!strm.is_open())
		{
			cout << "ERROR: File not found!" << endl;
			pause();
			return 0;
		}
		strm.close();
	}

	// Create FBX manager
	FbxManager *sdkmanager = FbxManager::Create();

	// Create IO settings
	FbxIOSettings *ios = FbxIOSettings::Create(sdkmanager, IOSROOT);
	sdkmanager->SetIOSettings(ios);

	// Create the importer
	FbxImporter *importer = FbxImporter::Create(sdkmanager, "");

	// Initialise the importer
	cout << "Importing scene..." << endl;
	if (!importer->Initialize(filename, -1, sdkmanager->GetIOSettings()))
	{
		cout << "ERROR: Failed to initialise importer!" << endl;
		cout << importer->GetStatus().GetErrorString();
		pause();
		return 0;
	}

	// Import the file into a scene
	FbxScene *scene = FbxScene::Create(sdkmanager, "scene");
	importer->Import(scene);
	importer->Destroy();

	// Find all meshes
	vector<FbxNode*> meshes;
	FbxNode *rootnode = scene->GetRootNode();
	if (rootnode)
	{
		for (int i = 0; i < rootnode->GetChildCount(); i++)
		{
			FbxNode *node = rootnode->GetChild(i);
			//cout << "- " << node->GetName() << endl;
			for (int j = 0; j < node->GetNodeAttributeCount(); j++)
			{
				FbxNodeAttribute *attr = node->GetNodeAttributeByIndex(j);
				if (attr->GetAttributeType() == FbxNodeAttribute::eMesh)
				{
					meshes.push_back(node);
				}
			}
		}
	}
	else
	{
		cout << "ERROR: No root node found!" << endl;
		pause();
		return 0;
	}

	// Check there are meshes to export
	if (meshes.size() == 0)
	{
		cout << "ERROR: No meshes found!" << endl;
		pause();
		return 0;
	}

	// Define the material map
	map<string, int> materialmap;
	int nextmaterialid = 0;

	// Loop each mesh
	cout << "Converting to SBM..." << endl;
	vector<FbxNode*>::iterator it;
	for (it = meshes.begin(); it != meshes.end(); it++)
	{
		// Get the node
		FbxNode *node = *it;
		
		// Get material count
		int num_mats = node->GetMaterialCount();
		
		// Loop each material
		for (int i = 0; i < num_mats; i++)
		{
			// Get the material
			FbxSurfaceMaterial *mat = node->GetMaterial(i);

			// Add to map
			string name = string(mat->GetName());
			map<string, int>::iterator searchit = materialmap.find(name);
			if (searchit == materialmap.end())
			{
				materialmap[name] = nextmaterialid++;
			}
		}
	}

	// Create the SBM header
	SBM::Header header;
	memcpy(header.magic, "SBM", 4);
	header.version = 1;
	header.num_materials = materialmap.size();
	header.num_meshes = meshes.size();

	// Create arrays for further use
	SBM::MeshHeader *meshheaders = new SBM::MeshHeader[meshes.size()];
	SBM::MeshStagingData *stagingdata = new SBM::MeshStagingData[meshes.size()];

	// Loop all meshes
	int curmesh = 0;
	for (it = meshes.begin(); it != meshes.end(); it++)
	{
		// Get the node and mesh
		FbxNode *node = *it;
		FbxMesh *mesh = node->GetMesh();

		// Get material and poly count
		int num_mats = node->GetMaterialCount();
		int num_polys = mesh->GetPolygonCount();

		// Define submesh and vertices vectors
		vector<SBM::Vertex> vertices;
		vector<unsigned int> *submeshes = new vector<unsigned int>[num_mats];

		// Define submesh headers
		SBM::SubmeshHeader *submeshheaders = new SBM::SubmeshHeader[num_mats];

		// Get control points
		int numcontrolpoints = mesh->GetControlPointsCount();
		FbxVector4 *controlpoints = mesh->GetControlPoints();

		// Get material indices
		FbxLayerElementArrayTemplate<int> *matindices;
		if (!mesh->GetMaterialIndices(&matindices))
		{
			cout << "ERROR: Failed to get material indices for mesh!" << endl;
			pause();
			return 0;
		}
		int numindices = matindices->GetCount();
		/*if (numindices < num_polys)
		{
			cout << "ERROR: Not enough material indices for all polygons!" << endl;
			pause();
			return 0;
		}*/

		// Get normals
		/*FbxLayerElementArrayTemplate<FbxVector4> *normals;
		if (!mesh->GetNormals(&normals))
		{
			cout << "ERROR: Failed to get normals for mesh!" << endl;
			pause();
			return 0;
		}
		FbxLayerElementArrayTemplate<int> *normalindices;
		if (!mesh->GetNormalsIndices(&normalindices))
		{
			cout << "ERROR: Failed to get normals indices for mesh!" << endl;
			pause();
			return 0;
		}*/

		// Get tangents
		/*FbxLayerElementArrayTemplate<FbxVector4> *tangents;
		if (!mesh->GetTangents(&tangents))
		{
			cout << "ERROR: Failed to get tangents for mesh!" << endl;
			pause();
			return 0;
		}
		FbxLayerElementArrayTemplate<int> *tangentsindices;
		if (!mesh->GetTangentsIndices(&tangentsindices))
		{
			cout << "ERROR: Failed to get tangents indices for mesh!" << endl;
			pause();
			return 0;
		}*/


		// Make tangents		
		//cout << "Generating tangent data..." << endl;
		bool success = mesh->GenerateTangentsData(0, false);
		FbxGeometryElementTangent *tangentdata = mesh->GetElementTangent();
		if (!success || tangentdata == nullptr)
		{
			cout << "Failed to generate tangent data!" << endl;
		}
		if (tangentdata->GetMappingMode() != FbxLayerElement::eByPolygonVertex)
		{
			cout << "Invalid tangent mapping mode!" << endl;
		}
		FbxLayerElement::EReferenceMode tangentrefmode = tangentdata->GetReferenceMode();
		/*if (!mesh->GetTangents(&tangents))
		{
			//FbxGeometryElementTangent *tangentdata = mesh->CreateElementTangent();
			//if (tangentdata == nullptr)
			//{
				cout << "Even though we generated tangents, we can't find them!" << endl;
			//}
		}
		else
		{
			if (!mesh->GetTangentsIndices(&tangentindices))
			{
				tangentcount = tangents->GetCount();
				cout << "Even though we generated tangents, we can't find the indices for them!" << endl;
			}
			else
			{
				tangentcount = tangentindices->GetCount();
				cout << "Tangent data is now present." << endl;
			}
		}*/
			
		

		// Get texcoords
		FbxLayerElementArrayTemplate<FbxVector2> *texcoords;
		if (!mesh->GetTextureUV(&texcoords))
		{
			cout << "ERROR: Failed to get texcoords for mesh!" << endl;
			pause();
			return 0;
		}

		// Loop each material
		for (int k = 0; k < num_mats; k++)
		{
			// Get reference to our submesh
			vector<unsigned int>& indices = submeshes[k];

			// Loop each polygon, generate vertices and indices
			for (int i = 0; i < num_polys; i++)
			{
				// Get the material index and check it
				int matindex = (*matindices)[i % numindices];
				if (matindex == k)
				{
					// Get the polygon index and number of vertices in the polygon
					int numindices = mesh->GetPolygonSize(i);
					int polygonvertexindex = mesh->GetPolygonVertexIndex(i);

					// Check the polygon is sane
					if (numindices > 2)
					{
						// Loop each vertex in the polygon
						int baseidx = vertices.size();
						for (int j = 0; j < numindices; j++)
						{
							// Setup vertex struct
							SBM::Vertex vertex;
							memset(&vertex, 0, sizeof(SBM::Vertex));
							int vertexindex = polygonvertexindex + j;

							// Get position
							int controlpointidx = mesh->GetPolygonVertex(i, j);
							vertex.position[0] = controlpoints[controlpointidx].mData[0];
							vertex.position[1] = controlpoints[controlpointidx].mData[1];
							vertex.position[2] = controlpoints[controlpointidx].mData[2];

							// Get normal
							FbxVector4 normal;
							mesh->GetPolygonVertexNormal(i, j, normal);
							vertex.normal[0] = normal.mData[0];
							vertex.normal[1] = normal.mData[1];
							vertex.normal[2] = normal.mData[2];

							// Get texture coord
							int uvindex = mesh->GetTextureUVIndex(i, j);
							FbxVector2 uv = (*texcoords)[uvindex];
							vertex.texcoord[0] = uv.mData[0];
							vertex.texcoord[1] = uv.mData[1];

							// Get tangent
							if (tangentdata != nullptr)
							{
								FbxVector4 tangent;
								if (tangentrefmode == FbxLayerElement::eDirect)
								{
									 tangent = tangentdata->GetDirectArray().GetAt(vertexindex);
								}
								else if (tangentrefmode == FbxLayerElement::eIndexToDirect)
								{
									
								}
								vertex.tangent[0] = tangent[0];
								vertex.tangent[1] = tangent[1];
								vertex.tangent[2] = tangent[2];
							}

							// Add to vertices array
							vertices.push_back(vertex);
						}


						// Triangulate into the indices array for our submesh
						for (int j = 1; j < numindices - 1; j++)
						{
							indices.push_back(baseidx);
							indices.push_back(baseidx + j);
							indices.push_back(baseidx + j + 1);
						}
					}
				}
			}

			// Create the submesh header
			SBM::SubmeshHeader submeshheader;
			string materialname = string(node->GetMaterial(k)->GetName());
			map<string, int>::iterator matit = materialmap.find(materialname);
			if (matit == materialmap.end())
			{
				cout << "ERROR: Failed to find material when generating submeshes! (This error should never be seen)" << endl;
				pause();
				return 0;
			}
			submeshheader.material_index = matit->second;
			submeshheader.num_indices = indices.size();
			submeshheaders[k] = submeshheader;
		}
		
		// Create the mesh header
		int meshindex = curmesh++;
		SBM::MeshHeader meshheader;
		meshheader.num_vertices = vertices.size();
		meshheader.num_submeshes = num_mats;
		meshheaders[meshindex] = meshheader;

		// Create the staging data
		SBM::MeshStagingData meshstagingdata;
		meshstagingdata.submeshheaders = submeshheaders;
		meshstagingdata.vertices = new SBM::Vertex[vertices.size()];
		memcpy(meshstagingdata.vertices, &vertices[0], vertices.size() * sizeof(SBM::Vertex)); // This probably breaks like 10 rules but whatever
		meshstagingdata.indices = new unsigned int*[num_mats];
		for (int i = 0; i < num_mats; i++)
		{
			// This is just getting silly now
			meshstagingdata.indices[i] = new unsigned int[submeshes[i].size()];
			memcpy(meshstagingdata.indices[i], &submeshes[i][0], submeshes[i].size() * sizeof(unsigned int));
		}
		stagingdata[meshindex] = meshstagingdata;
	}

	// Output to file
	ofstream strm;
	strm.open("output.sbm", ios_base::binary | ios_base::trunc);
	if (!strm.is_open())
	{
		cout << "ERROR: Could not open output file!" << endl;
		pause();
		return 0;
	}
	cout << "Exporting SBM file..." << endl;

	// Output SBM header
	strm.write((char*)&header, sizeof(header));

	// Output each material in order of index
	string *materials = new string[materialmap.size()];
	for (map<string, int>::iterator matit = materialmap.begin(); matit != materialmap.end(); matit++)
	{
		materials[matit->second] = matit->first;
	}
	for (int i = 0; i < materialmap.size(); i++)
		strm.write(materials[i].c_str(), materials[i].size() + 1);
	delete[] materials;

	// Loop each mesh
	for (int i = 0; i < header.num_meshes; i++)
	{
		// Output the mesh header
		SBM::MeshHeader& meshheader = meshheaders[i];
		strm.write((char*)&meshheader, sizeof(SBM::MeshHeader));

		// Get the staging data
		SBM::MeshStagingData& meshstagingdata = stagingdata[i];
		
		// Output the vertices array and clean up
		strm.write((char*)meshstagingdata.vertices, meshheader.num_vertices * sizeof(SBM::Vertex));
		delete[] meshstagingdata.vertices;

		// Loop each submesh
		for (int j = 0; j < meshheader.num_submeshes; j++)
		{
			// Output the submesh header
			SBM::SubmeshHeader& submeshheader = meshstagingdata.submeshheaders[j];
			strm.write((char*)&submeshheader, sizeof(SBM::SubmeshHeader));

			// Output the indices array and clean up
			strm.write((char*)meshstagingdata.indices[j], submeshheader.num_indices * sizeof(unsigned int));
			delete meshstagingdata.indices[j];
		}

		// Clean up
		delete[] meshstagingdata.submeshheaders;
		delete[] meshstagingdata.indices;
	}

	// Clean up
	delete[] stagingdata;
	delete[] meshheaders;
	sdkmanager->Destroy();

	// Finish writing
	strm.close();

	// Success
	cout << "Finished." << endl;
	pause();
	return 0;
}

void pause()
{
	cin.sync();
	cin.ignore();
}