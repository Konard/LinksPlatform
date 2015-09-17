#include "stdafx.h"
#include "CppUnitTest.h"

#include <PersistentMemoryManager.h>

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace CoreTests
{
    TEST_CLASS(PersistentMemoryManagerTests)
    {
    public:

        TEST_METHOD(InitTest)
        {
            InitPersistentMemoryManager();
        }

        TEST_METHOD(FileTest)
        {
            char* filename = "db.links";

            remove(filename);

            InitPersistentMemoryManager();

            Assert::IsTrue(succeeded(OpenStorageFile(filename)));

            Assert::IsTrue(succeeded(CloseStorageFile()));

            remove(filename);
        }

        TEST_METHOD(FileMappingTest)
        {
            char* filename = "db.links";

            remove(filename);

            InitPersistentMemoryManager();

            Assert::IsTrue(succeeded(OpenStorageFile(filename)));

            Assert::IsTrue(succeeded(SetStorageFileMemoryMapping()));

            Assert::IsTrue(succeeded(ResetStorageFileMemoryMapping()));

            Assert::IsTrue(succeeded(CloseStorageFile()));

            remove(filename);
        }

        TEST_METHOD(AllocateFreeLinkTest)
        {
            char* filename = "db.links";

            remove(filename);

            InitPersistentMemoryManager();

            Assert::IsTrue(succeeded(OpenStorageFile(filename)));

            Assert::IsTrue(succeeded(SetStorageFileMemoryMapping()));

            link_index link = AllocateLink();

            FreeLink(link);

            Assert::IsTrue(succeeded(ResetStorageFileMemoryMapping()));

            Assert::IsTrue(succeeded(CloseStorageFile()));

            remove(filename);
        }

        TEST_METHOD(AttachToUnusedLinkTest)
        {
            char* filename = "db.links";

            remove(filename);

            InitPersistentMemoryManager();

            Assert::IsTrue(succeeded(OpenStorageFile(filename)));

            Assert::IsTrue(succeeded(SetStorageFileMemoryMapping()));

            link_index link1 = AllocateLink();
            link_index link2 = AllocateLink();

            FreeLink(link1);

            Assert::IsTrue(succeeded(ResetStorageFileMemoryMapping()));

            Assert::IsTrue(succeeded(CloseStorageFile()));

            remove(filename);
        }

        TEST_METHOD(DetachToUnusedLinkTest)
        {
            char* filename = "db.links";

            remove(filename);

            InitPersistentMemoryManager();

            Assert::IsTrue(succeeded(OpenStorageFile(filename)));

            Assert::IsTrue(succeeded(SetStorageFileMemoryMapping()));

            link_index link1 = AllocateLink();
            link_index link2 = AllocateLink();

            FreeLink(link1);
            FreeLink(link2);

            Assert::IsTrue(succeeded(ResetStorageFileMemoryMapping()));

            Assert::IsTrue(succeeded(CloseStorageFile()));

            remove(filename);
        }
    };
}