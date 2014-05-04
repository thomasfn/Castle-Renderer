
#ifndef __SBM_HPP__
#define __SBM_HPP__

namespace SBM
{
	struct Header
	{
		char magic[4];
		int version;

		int num_materials;
		int num_meshes;
	};

	struct MeshHeader
	{
		int num_vertices;
		int num_submeshes;
	};

	struct SubmeshHeader
	{
		int num_indices;
		int material_index;
	};

	struct Vertex
	{
		float position[3];
		float normal[3];
		float texcoord[2];
		float tangent[3];
	};

	struct MeshStagingData
	{
		SubmeshHeader *submeshheaders;
		Vertex *vertices;
		unsigned int **indices;
	};

}

#endif