#pragma once

#include <vector>

using namespace std;


template <typename T> 
class AutoCollection
{
public:

	static DWORD Collect(T &obj)
	{
		s_CollectedObjects.push_back(&obj);
		DWORD objID = GetCollectionSize() - 1;
		return objID;
	}

	static bool TryGetObject(DWORD id, T** out_CollectedObject)
	{
		if(!out_CollectedObject) { return false; }
		*out_CollectedObject = NULL;

		if(id >= GetCollectionSize()) { return false; }

		T* temp = s_CollectedObjects[id];
		if(!temp) { return false; }

		*out_CollectedObject = temp;

		return true;
	}

	static void RemoveObject(DWORD id, bool releaseObject = true)
	{
		if(id >= GetCollectionSize()) { return; }

		if(releaseObject) { SafeRelease(s_CollectedObjects[id]); }
		s_CollectedObjects[id] = NULL;		// Note: We don't erase the entry because this would shorten the vector, invalidating any index references
	}

	static void RemoveAll(bool releaseObject = true)
	{
		for (DWORD i = 0; i < GetCollectionSize(); i++)
		{
			RemoveObject(i, releaseObject);
		}
	}

	static vector<T*>& GetCollection() { return *(new vector<T*>(s_CollectedObjects)); }

	static DWORD GetCollectionSize() { return s_CollectedObjects.size(); }


private:

	static vector<T*> s_CollectedObjects;

	AutoCollection(){}	// private constructor (no instances of this class may be instantiated; it is purely static)
};


template< typename T >
vector<T*> AutoCollection<T>::s_CollectedObjects;