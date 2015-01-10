
#define CBUFFER_CLIP(_reg_) cbuffer Clip : register(_reg_) \
{ \
	float Enabled; \
	float4 Plane; \
}

#define CBUFFER_OBJECT_TRANSFORM(_reg_) cbuffer ObjectTransform : register(_reg_) \
{ \
	float4x4 ModelMatrix; \
}

#define CBUFFER_CAMERA(_reg_) cbuffer Camera : register(_reg_) \
{ \
	float3 Position; \
	float3 Forward; \
}

#define CBUFFER_CAMERA_TRANSFORM(_reg_) cbuffer CameraTransform : register(_reg_) \
{ \
	float4x4 Projection; \
	float4x4 View; \
}