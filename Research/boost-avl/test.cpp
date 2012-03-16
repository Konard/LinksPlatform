// see http://www.boost.org/doc/libs/1_43_0/doc/html/boost/intrusive/avltree.html
// and package libboost-dev
// and http://www.boost.org/doc/libs/1_39_0/doc/html/intrusive/avl_set_multiset.html

#include <boost/intrusive/avl_set.hpp>
#include <vector>
#include <algorithm>
#include <cassert>

using namespace boost::intrusive;

class MyClass : public avl_set_base_hook<optimize_size<true> >
{
   int int_;

   public:
   //This is a member hook
   avl_set_member_hook<> member_hook_;

   MyClass(int i)
      :  int_(i)
      {}
   friend bool operator< (const MyClass &a, const MyClass &b)
      {  return a.int_ < b.int_;  }
   friend bool operator> (const MyClass &a, const MyClass &b)
      {  return a.int_ > b.int_;  }
   friend bool operator== (const MyClass &a, const MyClass &b)
      {  return a.int_ < b.int_;  }
};

typedef avl_set< MyClass, compare<std::greater<MyClass> > >     BaseSet;
typedef member_hook<MyClass, avl_set_member_hook<>, &MyClass::member_hook_> MemberOption;
typedef avl_multiset< MyClass, MemberOption>   MemberMultiset;

int main() {
	typedef std::vector<MyClass>::iterator VectIt;
	typedef std::vector<MyClass>::reverse_iterator VectRit;

	BaseSet baseset;
	MemberMultiset membermultiset;

	std::vector<MyClass> values;
	for(int i = 0; i < 1000000; ++i)  values.push_back(MyClass(i));

	//Check that size optimization is activated in the base hook 
	assert(sizeof(avl_set_base_hook<optimize_size<true> >) == 3*sizeof(void*));
	//Check that size optimization is deactivated in the member hook 
	assert(sizeof(avl_set_member_hook<>) > 3*sizeof(void*));

	for(VectIt it(values.begin()), itend(values.end()); it != itend; ++it){
		baseset.insert(*it);
//		membermultiset.insert(*it);
	}

	return 0;
}
