#pragma once
#include "far\topologyDescriptor.h"
using namespace OpenSubdiv;
using namespace OpenSubdiv::v3_3_3;
using namespace System::Collections::Generic;
using Descriptor = Far::TopologyDescriptor;

public ref class DescripterCs
{
    int* VertPerFaces;
    std::vector<int >* IndArray;
  void Create(int verts, int faces, const Far::Index* ind , int vertsByFace);

public:
  Descriptor* desc;
  DescripterCs::DescripterCs(int verts, int faces, const Far::Index* ind);
  DescripterCs::DescripterCs(int verts, int faces, List<int>^ ind , int vertsByFace );
  ~DescripterCs();
};

