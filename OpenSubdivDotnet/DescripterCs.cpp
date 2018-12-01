#include "stdafx.h"
#include "DescripterCs.h"
#include <memory>


    //int           numVertices,
    //              numFaces;

    //int const   * numVertsPerFace;
    //Index const * vertIndicesPerFace;

    //int           numCreases;
    //Index const * creaseVertexIndexPairs;
    //float const * creaseWeights;

    //int           numCorners;
    //Index const * cornerVertexIndices;
    //float const * cornerWeights;
    //    
    //int           numHoles;
    //Index const * holeIndices;

    //bool          isLeftHanded;

void DescripterCs::Create(int verts, int faces, const Far::Index* ind , int vertsByFace)
{
  desc = new Descriptor();
  desc->numVertices = verts;
  desc->numFaces = faces;
  // 4–Ê‚ª‚È‚¢‚Ì‚Å
  VertPerFaces = new int[faces];
  for (size_t i = 0; i < faces; i++)
  {
    VertPerFaces[i] = vertsByFace;
  }
  desc->numVertsPerFace = VertPerFaces;
  desc->vertIndicesPerFace = ind;
}

DescripterCs::DescripterCs(int verts, int faces , const Far::Index* ind)
{
  Create( verts, faces, ind , 4);
}

DescripterCs::DescripterCs(int verts, int faces, List<int>^ ind , int vertsByFace)
{
  //auto indArray = std::vector<std::unique_ptr<int> >( ind->Count );
  IndArray = new std::vector< int >( ind->Count );
  for (size_t i = 0; i < ind->Count ; i++)
  {
    //indArray[i].reset( new int(ind[i]) );
    (*IndArray)[i] = ind[i];
  }
  //Create(verts, faces, indArray.data()->get());
  Create(verts, faces, IndArray->data() , vertsByFace );
}

DescripterCs::~DescripterCs()
{
  printf("release desc");
  delete[] VertPerFaces;
  delete IndArray;
}
