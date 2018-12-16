// OpenSubdivDotnet.cpp : メイン プロジェクト ファイルです。
#include "stdafx.h"
float M_PI = 3.141592f;

//#include "far\error.h"
//#include "far\topologyRefiner.h"
#include "far\topologyDescriptor.h"
#include "DescripterCs.h"
#include "AdaptiveOptionsCs.h"
#include "FVarChannelCs.h"

#define UVRefine 1

#define and &&
#define and_eq &=
#define bitand &
#define bitor |
#define compl ~
#define not !
#define not_eq !=
#define or ||
#define or_eq |=
#define xor ^
#define xor_eq ^=
#include <crtdbg.h>  

//#include <osd/cpuEvaluator.h>

using namespace OpenSubdiv;
using namespace System;
using Descriptor = Far::TopologyDescriptor;
using TopoRefiner = Far::TopologyRefiner;
using TopoFactory = TopologyRefinerFactory<Descriptor>;
using namespace System::Collections::Generic;

public ref struct Vec3
{
  Vec3(float x, float y, float z)
  {
    X = x;
    Y = y;
    Z = z;
  }

  ~Vec3()
  {
    this->!Vec3();
  }

  !Vec3()
  {
    Console::WriteLine("release Vec3");
  }

  virtual System::String ^ToString() override
  {
    return X.ToString() + " " + Y.ToString() + " " + Z.ToString();
  }

  float X;
  float Y;
  float Z;
};

using TexturedVert = Vec3;

struct Vertex {

  // Minimal required interface ----------------------
  Vertex() { }

  Vertex(Vertex const & src) {
    _position[0] = src._position[0];
    _position[1] = src._position[1];
    _position[2] = src._position[2];
  }

  Vertex(float x , float y , float z) {
    _position[0] = x;
    _position[1] = y;
    _position[2] = z;
  }

  ~Vertex()
  {
    //Console::WriteLine("release Vertex");
  }

  void Clear(void * = 0) {
    _position[0] = _position[1] = _position[2] = 0.0f;
  }

  void AddWithWeight(Vertex const & src, float weight) {
    _position[0] += weight * src._position[0];
    _position[1] += weight * src._position[1];
    _position[2] += weight * src._position[2];
  }

  // Public interface ------------------------------------
  void SetPosition(float x, float y, float z) {
    _position[0] = x;
    _position[1] = y;
    _position[2] = z;
  }

  const float * GetPosition() const {
    return _position;
  }

private:
  float _position[3];
};

struct FVarVertexUV
{

  FVarVertexUV()
  {
    U = 0;
    V = 0;
  }
  
  ~FVarVertexUV()
  {
    //Console::WriteLine("release FVarVertexUV");
  }

  FVarVertexUV(float u, float v)
  {
    U = u;
    V = v;
  }

  // Minimal required interface ----------------------
  void Clear()
  {
    U = V = 0.0f;
  }

  void AddWithWeight(FVarVertexUV const & src, float weight) {
    U += weight * src.U;
    V += weight * src.V;
  }
  // Basic 'uv' layout channel
  float U, V;
};

struct Mesh
{
  Vertex* Vertex;
  FVarVertexUV* UV;
};

static float g_verts[8][3] = { { -0.5f, -0.5f,  0.5f },
                              {  0.5f, -0.5f,  0.5f },
                              { -0.5f,  0.5f,  0.5f },
                              {  0.5f,  0.5f,  0.5f },
                              { -0.5f,  0.5f, -0.5f },
                              {  0.5f,  0.5f, -0.5f },
                              { -0.5f, -0.5f, -0.5f },
                              {  0.5f, -0.5f, -0.5f } };

static int g_nverts = 8,
g_nfaces = 6;

static int g_vertsperface[6] = { 4, 4, 4, 4, 4, 4 };

static int g_vertIndices[24] = { 0, 1, 3, 2,
                                 2, 3, 5, 4,
                                 4, 5, 7, 6,
                                 6, 7, 1, 0,
                                 1, 7, 5, 3,
                                 6, 0, 2, 4 };


public ref class Refiner
{
internal:

  std::vector<Vertex*>* Verts ;
  DescripterCs^ PolyDesc;
  AdaptiveOptionsCs^ Option;
  List<FVarChannelCs^>^ FVarChannels;
  std::vector<FVarVertexUV>* VertUV;

  std::vector<Descriptor::FVarChannel> GetChannels()
  {
      std::vector<Descriptor::FVarChannel> varChannels( FVarChannels->Count );
      for (int i = 0; i < FVarChannels->Count; i++)
      {
        varChannels[i] = (*FVarChannels)[i]->ToFVarChannel();
      }
      return varChannels;
  }

  public:

    Action<float, float, float>^ OnVert;
    Action<int, int, int>^ OnFace;
    Action<float, float>^ OnUV;

    void AddDesc(DescripterCs^ desc)
    {
      PolyDesc = desc;
    }

    void AddOption(AdaptiveOptionsCs^ opt)
    {
      Option = opt;
    }

    void AddChannel(FVarChannelCs^ channel)
    {
      FVarChannels->Add(channel);
    }

    void SetUV(float u , float v)
    {
      VertUV->push_back(FVarVertexUV( u,v) );
    }

    void print(Far::TopologyRefiner * refiner, int level , Mesh mesh)
    {
      Far::TopologyLevel const & refLastLevel = refiner->GetLevel(level);

      int nverts = refLastLevel.GetNumVertices();
      int nfaces = refLastLevel.GetNumFaces();

      // Print vertex positions
      int firstOfLastVerts = refiner->GetNumVerticesTotal() - nverts;

      for (int vert = 0; vert < nverts; ++vert) {
        float const * pos = mesh.Vertex[firstOfLastVerts + vert].GetPosition();
        if (OnVert != nullptr)
        {
          OnVert( pos[0], pos[1], pos[2]);
        }
        //printf("v %f %f %f\n", pos[0], pos[1], pos[2]);
      }

      // varChannels 0は uvとする
      int channelUV = 0;
        // Print uvs
        int nuvs   = refLastLevel.GetNumFVarValues(channelUV);
        int firstOfLastUvs = refiner->GetNumFVarValuesTotal(channelUV) - nuvs;
        for (int fvvert = 0; fvvert < nuvs; ++fvvert)
        {
            FVarVertexUV const & uv = mesh.UV[firstOfLastUvs + fvvert];
            if (OnUV != nullptr)
            {
              OnUV(uv.U, uv.V);
            }
            //printf("vt %f %f\n", uv.U, uv.V);
        }

      // Print faces
      for (int face = 0; face < nfaces; ++face)
      {

        Far::ConstIndexArray fverts = refLastLevel.GetFaceVertices(face);

        // all refined Catmark faces should be quads
        assert(fverts.size() == 4);

#if 0
        printf("f ");
        for (int vert = 0; vert<fverts.size(); ++vert)
        {
          printf("%d ", fverts[vert] + 1); // OBJ uses 1-based arrays...
        }
#endif
        //printf("f %d %d %d\n", fverts[0] + 1 , fverts[1] + 1 , fverts[2] + 1 );
        //printf("f %d %d %d"  , fverts[0] + 1 , fverts[2] + 1 , fverts[3] + 1 );
        if (OnFace != nullptr)
        {
          OnFace(fverts[0] , fverts[1] , fverts[2] );
          OnFace(fverts[0] , fverts[2] , fverts[3] );
        }
        printf("\n");
      }
    }

    void DoRefine()
    {
      Sdc::SchemeType type = OpenSubdiv::Sdc::SCHEME_CATMARK;

      Sdc::Options options;
      // エッジのみをシャープに、コーナーも含められる
      options.SetVtxBoundaryInterpolation(Sdc::Options::VTX_BOUNDARY_EDGE_ONLY);
      // 三角がスムースになるけど、つながらない面が出るかも 3角面しかmmdにないし、変に穴が開くので
      //options.SetTriangleSubdivision(
      //  Sdc::Options::TriangleSubdivision::TRI_SUB_SMOOTH
      //);
      options.SetFVarLinearInterpolation(Sdc::Options::FVAR_LINEAR_NONE);
      Descriptor desc = *PolyDesc->desc;

      std::vector<Descriptor::FVarChannel> varChannels = GetChannels();
      desc.fvarChannels    = varChannels.data();
      desc.numFVarChannels = varChannels.size();

      TopoFactory::Options inOpt = TopoFactory::Options(type, options);
      // Instantiate a FarTopologyRefiner from the descriptor
      std::unique_ptr<TopologyRefiner> refiner = std::unique_ptr<TopologyRefiner>
         ( TopoFactory::Create(desc , inOpt ) );
#if Adaptive
      TopologyRefiner::AdaptiveOptions option = Option->ToAdaptiveOptions();
      // Uniformly refine the topology up to 'maxlevel'

      refiner->RefineAdaptive( option );
#else
     TopologyRefiner::UniformOptions option(Option->IsolationLevel);
      option.fullTopologyInLastLevel = true;
      refiner->RefineUniform ( option );
#endif
      // 最大レベルリファインした頂点+元頂点合計分バッファーを作る
      // Allocate a buffer for vertex primvar data. The buffer length is set to
      // be the sum of all children vertices up to the highest level of refinement.
      std::vector<Vertex> vbuffer( refiner->GetNumVerticesTotal() );
      Vertex * verts = &vbuffer[0];

      // 座標を登録
      int nCoarseVerts = desc.numVertices;
      for (int i = 0; i < nCoarseVerts; ++i)
      {
        const float* v = (*Verts)[i]->GetPosition();
        verts[i].SetPosition(v[0], v[1], v[2]);
      }

      // varChannels 0は uvとする
      int channelUV = 0;

      // fvBufferUV にuvを登録
      int uvsCount = refiner->GetNumFVarValuesTotal(channelUV);
      std::vector<FVarVertexUV> fvBufferUV ( 
        //refiner->GetNumVerticesTotal ( )
        uvsCount
      );
      FVarVertexUV* fvVertsUV = &fvBufferUV[0];
      for (size_t i = 0; i < VertUV->size(); ++i)
      {
        fvVertsUV[ i ].U = ( *VertUV )[ i ].U;
        fvVertsUV[ i ].V = ( *VertUV )[ i ].V;
      }

      // Interpolate vertex primvar data
      Far::PrimvarRefiner primvarRefiner ( *refiner );

      // 前のレベルに足す形で頂点をリファイン
      Vertex * src = verts;
      FVarVertexUV *    srcFVarUV = fvVertsUV;
      for (unsigned int level = 1; level <= Option->IsolationLevel ; ++level)
      {
        Vertex * dst = src + refiner->GetLevel(level - 1).GetNumVertices();

        primvarRefiner.Interpolate ( level , src , dst );
        src = dst;
#if UVRefine
        int currentLevelUVCount = refiner->GetLevel ( level - 1 ).GetNumFVarValues ( channelUV );
        FVarVertexUV * dstFVarUV = srcFVarUV + currentLevelUVCount;
        primvarRefiner.InterpolateFaceVarying ( level , srcFVarUV , dstFVarUV , channelUV );
        srcFVarUV = dstFVarUV;
#endif
      }

      Mesh mesh;
      mesh.Vertex = verts;
      mesh.UV = fvVertsUV;
      print( refiner.get() , Option->IsolationLevel , mesh);
    }


    Refiner(List<TexturedVert^>^ vert, List<int>^ ind , int faces)
    {
      _CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);

      Verts = new std::vector<Vertex*>( vert->Count );
      for (int i = 0; i < vert->Count; i++)
      {
        TexturedVert^ v = vert[i];
        (*Verts)[i] = new Vertex(v->X, v->Y, v->Z);
      }
      PolyDesc = gcnew DescripterCs( vert->Count , ind->Count / faces , ind , faces );
      Option = gcnew AdaptiveOptionsCs(1);
      FVarChannels = gcnew List<FVarChannelCs^>();
      VertUV = new std::vector<FVarVertexUV>();
    }

    ~Refiner()
    {
      this->!Refiner();
    }

    // GC , Dispose両方対応するため Finalizerで開放をさせる
    !Refiner()
    {
      for (size_t i = 0; i < Verts->size() ; i++)
      {
        Vertex* it = Verts->at(i);
        delete it;
      }
      delete Verts;
      PolyDesc->~DescripterCs();
      Option->~AdaptiveOptionsCs();
      delete FVarChannels;
      delete VertUV;
      printf("refine release");
    }


  static void TestRefine()
  {
    //http://graphics.pixar.com/opensubdiv/docs/far_tutorial_0.html
    //http://graphics.pixar.com/opensubdiv/docs/api_overview.html
    // 保管方法 Bilinear保管なども選べる
    Sdc::SchemeType type = OpenSubdiv::Sdc::SCHEME_CATMARK;

    Sdc::Options options;
    // エッジのみをシャープに、コーナーも含められる
    options.SetVtxBoundaryInterpolation(Sdc::Options::VTX_BOUNDARY_EDGE_ONLY);
    // スムーストライアングルウェイト、 Catmark ウェイトも選べる
#if 0
    Descriptor desc;
    desc.numVertices = g_nverts;
    desc.numFaces = g_nfaces;
    desc.numVertsPerFace = g_vertsperface;
    desc.vertIndicesPerFace = g_vertIndices;
#else
    DescripterCs^ cdesc = gcnew DescripterCs(g_nverts, g_nfaces, g_vertIndices);
    Descriptor desc = *cdesc->desc;

#endif


    // Instantiate a FarTopologyRefiner from the descriptor
    std::unique_ptr<TopoRefiner> refiner = std::unique_ptr<TopoRefiner>( TopoFactory::Create(desc,
      TopoFactory::Options(type, options)) );

    int maxlevel = 3;

    // Uniformly refine the topology up to 'maxlevel'
    refiner->RefineUniform(TopoRefiner::UniformOptions(maxlevel));


    // 最大レベルリファインした頂点+元頂点合計分バッファーを作る
    // Allocate a buffer for vertex primvar data. The buffer length is set to
    // be the sum of all children vertices up to the highest level of refinement.
    std::vector<Vertex> vbuffer(refiner->GetNumVerticesTotal());
    Vertex * verts = &vbuffer[0];


    // 座標を登録
    int nCoarseVerts = g_nverts;
    for (int i = 0; i<nCoarseVerts; ++i) {
      verts[i].SetPosition(g_verts[i][0], g_verts[i][1], g_verts[i][2]);
    }


    // Interpolate vertex primvar data
    Far::PrimvarRefiner primvarRefiner(*refiner);

    // 前のレベルに足す形で頂点をリファイン
    Vertex * src = verts;
    for (int level = 1; level <= maxlevel; ++level) {
      Vertex * dst = src + refiner->GetLevel(level - 1).GetNumVertices();
      primvarRefiner.Interpolate(level, src, dst);
      src = dst;
    }

    { // Output OBJ of the highest level refined -----------

      Far::TopologyLevel const & refLastLevel = refiner->GetLevel(maxlevel);

      int nverts = refLastLevel.GetNumVertices();
      int nfaces = refLastLevel.GetNumFaces();

      // Print vertex positions
      int firstOfLastVerts = refiner->GetNumVerticesTotal() - nverts;

      for (int vert = 0; vert < nverts; ++vert) {
        float const * pos = verts[firstOfLastVerts + vert].GetPosition();
        printf("v %f %f %f\n", pos[0], pos[1], pos[2]);
      }

      // Print faces
      for (int face = 0; face < nfaces; ++face)
      {

        Far::ConstIndexArray fverts = refLastLevel.GetFaceVertices(face);

        // all refined Catmark faces should be quads
        assert(fverts.size() == 4);

        printf("f ");
#if 0
        for (int vert = 0; vert<fverts.size(); ++vert)
        {
          printf("%d ", fverts[vert] + 1); // OBJ uses 1-based arrays...
        }
#endif
        printf("%d %d %d", fverts[0], fverts[1], fverts[2]);
        printf("%d %d %d", fverts[0], fverts[2], fverts[3]);
        printf("\n");
      }
    }
  }
};

int main(array<System::String ^> ^args)
{
    _CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF | _CRTDBG_CHECK_ALWAYS_DF);
    Refiner::TestRefine();
    //Console::WriteLine(L"Hello World");
    return 0;
}
