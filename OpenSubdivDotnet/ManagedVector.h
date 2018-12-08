#pragma once
#include <vector>
template<typename value_t>
ref class ManagedVector
{
  std::vector< value_t >* vec;
public:
  ManagedVector()
  {
    vec = new std::vector<value_t>();
  }

  ~ManagedVector()
  {
    this->!ManagedVector();
  }
  !ManagedVector()
  {
    delete vec;
  }

  value_t& operator[](std::size_t idx) { return (*vec)[idx]; }

  //const value_t& operator[](std::size_t idx) const { return vec[idx]; }

  void Add(value_t v)
  {
    vec->push_back(v);
  }

};