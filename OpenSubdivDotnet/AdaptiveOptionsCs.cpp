#include "stdafx.h"
#include "AdaptiveOptionsCs.h"
/*
    struct AdaptiveOptions {

        AdaptiveOptions(int level) :
            isolationLevel(level),
            secondaryLevel(15),
            useSingleCreasePatch(false),
            useInfSharpPatch(false),
            considerFVarChannels(false),
            orderVerticesFromFacesFirst(false) { }

        unsigned int isolationLevel:4;              ///< Number of iterations applied to isolate
                                                    ///< extraordinary vertices and creases
        unsigned int secondaryLevel:4;              ///< Shallower level to stop isolation of
                                                    ///< smooth irregular features
        unsigned int useSingleCreasePatch:1;        ///< Use 'single-crease' patch and stop
                                                    ///< isolation where applicable
        unsigned int useInfSharpPatch:1;            ///< Use infinitely sharp patches and stop
                                                    ///< isolation where applicable
        unsigned int considerFVarChannels:1;        ///< Inspect face-varying channels and
                                                    ///< isolate when irregular features present
        unsigned int orderVerticesFromFacesFirst:1; ///< Order child vertices from faces first
                                                    ///< instead of child vertices of vertices
    };


*/

AdaptiveOptionsCs::AdaptiveOptionsCs(unsigned short isolationLevel)
{
  IsolationLevel = isolationLevel;
  SecondaryLevel = 15;
  UseSingleCreasePatch = false;
  UseInfSharpPatch = false;
  ConsiderFVarChannels = false;
  OrderVerticesFromFacesFirst = false;
}

AdaptiveOptionsCs::~AdaptiveOptionsCs()
{
  this->!AdaptiveOptionsCs();
}

AdaptiveOptionsCs::!AdaptiveOptionsCs()
{
  printf("released AdaptiveOptionsCs \n");
}

TopologyRefiner::AdaptiveOptions AdaptiveOptionsCs::ToAdaptiveOptions()
{
  TopologyRefiner::AdaptiveOptions option(IsolationLevel);
  option.secondaryLevel = SecondaryLevel;
  option.useInfSharpPatch = UseInfSharpPatch;
  option.useSingleCreasePatch = UseSingleCreasePatch;
  option.considerFVarChannels = ConsiderFVarChannels;
  option.orderVerticesFromFacesFirst = OrderVerticesFromFacesFirst;
  return option;
}