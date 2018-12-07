#pragma once
#include "far\topologyRefiner.h"
using namespace OpenSubdiv::Far;

public ref struct AdaptiveOptionsCs
{
  unsigned short IsolationLevel;
  unsigned short SecondaryLevel;
  bool UseSingleCreasePatch;
  bool UseInfSharpPatch;
  bool ConsiderFVarChannels;
  bool OrderVerticesFromFacesFirst;
public:
  AdaptiveOptionsCs(unsigned short isolationLevel);
  ~AdaptiveOptionsCs();
  !AdaptiveOptionsCs();
  TopologyRefiner::AdaptiveOptions ToAdaptiveOptions();
};

