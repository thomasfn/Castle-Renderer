
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