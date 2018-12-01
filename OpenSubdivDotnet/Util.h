#pragma once
class Util
{
public:
  template <class ForwardIterator, class T>
  void Fill(ForwardIterator first, ForwardIterator last, T value)
  {
    for (; first != last; ++first) {
      *first = value;
    }
  }

  Util();
  ~Util();
};

