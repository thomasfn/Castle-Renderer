
#define CBUFFER_CLIP(_reg_) cbuffer Clip : register(_reg_) \
{ \
	float ClipEnabled; \
	float4 ClipPlane; \
}

#define CBUFFER_OBJECT_TRANSFORM(_reg_) cbuffer ObjectTransform : register(_reg_) \
{ \
	row_major float4x4 ModelMatrix; \
}

#define CBUFFER_CAMERA(_reg_) cbuffer Camera : register(_reg_) \
{ \
	float3 CameraPosition; \
	float3 CameraForward; \
}

#define CBUFFER_CAMERA_TRANSFORM(_reg_) cbuffer CameraTransform : register(_reg_) \
{ \
	row_major float4x4 ProjectionMatrix; \
	row_major float4x4 ViewMatrix; \
	row_major float4x4 ViewMatrixRotOnly; \
}

#define CBUFFER_PHYSICAL_PROPERTIES(_reg_) cbuffer PhysicalProperties : register(_reg_) \
{ \
	float Roughness; \
	float Reflectivity; \
}

#define CBUFFER_TEXTURE_TRANSFORM(_reg_) cbuffer TextureTransform : register(_reg_) \
{ \
	float2 TextureOffset; \
	float2 TextureScale; \
}