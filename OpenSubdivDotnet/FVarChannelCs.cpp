#include "stdafx.h"
#include "FVarChannelCs.h"


FVarChannelCs::FVarChannelCs( List<int>^ ind , int uniqueIndiceCount)
{
  Inds = ind;
  UniqueIndiceCount = uniqueIndiceCount;
  IndArray = new std::vector< int >(ind->Count);
  for (int i = 0; i < ind->Count; i++)
  {
    (*IndArray)[i] = Inds[i];
  }
}


FVarChannelCs::~FVarChannelCs()
{
  this->!FVarChannelCs();
}

FVarChannelCs::!FVarChannelCs()
{
  printf("release FVarChannel\n");
  delete IndArray;
}

Descriptor::FVarChannel FVarChannelCs::ToFVarChannel()
{
  Descriptor::FVarChannel channel;
  channel.numValues = UniqueIndiceCount;
  channel.valueIndices = IndArray->data();
  return channel;
}
