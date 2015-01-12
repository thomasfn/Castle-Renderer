
// http://gamedevelop.eu/en/tutorials/dual-paraboloid-shadow-mapping.htm

float4 CalculateParaboloid(float4 viewpos, out float clipdepth)
{
	viewpos /= viewpos.w;

	// for the back-map z has to be inverted
	viewpos.z *= PBDir;

	// because the origin is at 0 the proj-vector
	// matches the vertex-position
	float fLength = length(viewpos.xyz);

	// normalize
	viewpos /= fLength;

	// save for clipping 	
	clipdepth = viewpos.z;

	// calc "normal" on intersection, by adding the 
	// reflection-vector(0,0,1) and divide through 
	// his z to get the texture coords
	viewpos.z += 1.0;
	viewpos.x /= viewpos.z;
	viewpos.y /= viewpos.z;

	// set z for z-buffering and neutralize w
	viewpos.z = (fLength - PBNear) / (PBFar - PBNear);
	viewpos.w = 1.0f;

	// DP-depth
	//Out.Depth = Out.Pos.z;
	return viewpos;
}