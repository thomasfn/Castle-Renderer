
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
	float Paraboloid; \
	float PBFar, PBNear, PBDir; \
}

#define CBUFFER_PHYSICAL_PROPERTIES(_reg_) cbuffer PhysicalProperties : register(_reg_) \
{ \
	float Roughness; \
	float Reflectivity; \
	float IndexOfRefraction; \
}

#define CBUFFER_TEXTURE_TRANSFORM(_reg_) cbuffer TextureTransform : register(_reg_) \
{ \
	float2 TextureOffset; \
	float2 TextureScale; \
}

#define CBUFFER_PPEFFECT_INFO(_reg_) cbuffer PPEffectInfo : register(_reg_) \
{ \
	float2 ImageSize; \
}

#define CBUFFER_REFLECTION_INFO(_reg_) cbuffer ReflectionInfo : register(_reg_) \
{ \
	row_major float4x4 ReflViewMatrix; \
}