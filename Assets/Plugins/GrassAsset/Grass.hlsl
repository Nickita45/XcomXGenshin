 // This describes a vertex on the generated mesh
struct DrawVertex
{
   float3 positionWS; // The position in world space
   float2 uv;
}; 
    // A triangle on the generated mesh
struct DrawTriangle
{
   float3 normalOS; 
   float3 diffuseColor;
   DrawVertex vertices[3]; // The three points on the triangle
};
    
// A buffer containing the generated mesh
StructuredBuffer<DrawTriangle> _DrawTriangles;
float _OrthographicCamSizeTerrain;
float3 _OrthographicCamPosTerrain;

//get the data from the compute shader
void GetComputeData_float(float vertexID, out float3 worldPos, out float3 normal, out float2 uv, out float3 col)
{
      DrawTriangle tri = _DrawTriangles[vertexID / 3];
      DrawVertex input = tri.vertices[vertexID % 3];
      worldPos = input.positionWS;
      normal =  tri.normalOS;   
      uv = input.uv;      
      col = tri.diffuseColor;
  
}

// world space uv for blending
void GetWorldUV_float(float3 worldPos, out float2 worldUV)
{
      float2 uv =worldPos.xz - _OrthographicCamPosTerrain.xz;
      uv = uv / (_OrthographicCamSizeTerrain * 2);
      uv += 0.5;
      worldUV = uv;
}