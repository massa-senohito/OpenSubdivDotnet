#pragma once
#include "far\topologyDescriptor.h"
using namespace System::Collections::Generic;
using namespace OpenSubdiv::v3_3_3;
using Descriptor = Far::TopologyDescriptor;

public ref class FVarChannelCs
{
  List<int>^ Inds;
  std::vector< Far::Index >* IndArray;
  int UniqueIndiceCount;

public:
  FVarChannelCs(List<int>^ ind , int uniqueIndiceCount);
  ~FVarChannelCs();
  Descriptor::FVarChannel ToFVarChannel();
};

