// This is the main DLL file.

#include "ConvexAPI.h"

namespace ConvexAPI {
	ConvexHullResult ^iConvexDecomposition::getConvexHullResult(NxU32 hullIndex){
		CONVEX_DECOMPOSITION::ConvexHullResult *result = (CONVEX_DECOMPOSITION::ConvexHullResult*) malloc(sizeof(CONVEX_DECOMPOSITION::ConvexHullResult));
		if (backing->getConvexHullResult(hullIndex, *result)){
			return gcnew ConvexHullResult(result);
		}
		return nullptr;
	}
	bool iConvexDecomposition::setMesh(NxU32 vertCount, array<NxF32> ^verts, NxU32 faceCount, array<NxU32> ^facets)
	{
		pin_ptr<NxF32> vertsP = &verts[0];
		pin_ptr<NxU32> facesP = &facets[0];
		return backing->setMesh(vertCount, vertsP, faceCount, facesP);
	}
	cli::array<NxF32> ^ConvexHullResult::getVertices(){
		cli::array<NxF32> ^verts = gcnew cli::array<NxF32>(backing->mVcount * 3);
		System::Runtime::InteropServices::Marshal::Copy(IntPtr((void*)backing->mVertices),verts,0,verts->Length);
		return verts;
	}
	cli::array<NxU32> ^ConvexHullResult::getIndicies(){
		cli::array<NxU32> ^verts = gcnew cli::array<NxU32>(backing->mTcount * 3);
		pin_ptr<NxU32> ptr = &verts[0];
		int cpyCount = backing->mTcount * 3 * sizeof(NxU32);
		memcpy_s(ptr, cpyCount, backing->mIndices, cpyCount);
		return verts;
	}

	void StandaloneConvexHull::computeFor(NxU32 vertCount, array<NxF32> ^verts){
		if (backing != NULL){
			library->ReleaseResult(*backing);
		}

		pin_ptr<NxF32> vertsP = &verts[0];
		//NxF32 *vertCopy = (NxF32*) malloc(sizeof(NxF32) * vertCount);
		CONVEX_DECOMPOSITION::HullDesc desc(CONVEX_DECOMPOSITION::HullFlag::QF_TRIANGLES,vertCount, vertsP,sizeof(NxF32)*3);
		CONVEX_DECOMPOSITION::HullError error = library->CreateConvexHull(desc, *backing);
		printf("Error: %d\n", error);
	}
	cli::array<NxF32> ^StandaloneConvexHull::getVertices(){
		cli::array<NxF32> ^verts = gcnew cli::array<NxF32>(backing->mNumOutputVertices * 3);
		System::Runtime::InteropServices::Marshal::Copy(IntPtr((void*)backing->mOutputVertices),verts,0,verts->Length);
		return verts;
	}
	cli::array<NxU32> ^StandaloneConvexHull::getIndicies(){
		cli::array<NxU32> ^verts = gcnew cli::array<NxU32>(backing->mNumFaces * 3);
		pin_ptr<NxU32> ptr = &verts[0];
		int cpyCount = backing->mNumFaces * 3 * sizeof(NxU32);
		memcpy_s(ptr, cpyCount, backing->mIndices, cpyCount);
		return verts;
	}
}