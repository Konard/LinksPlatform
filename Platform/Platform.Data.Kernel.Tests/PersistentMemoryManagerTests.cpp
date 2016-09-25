#include "stdafx.h"
#include "CppUnitTest.h"

#include <PersistentMemoryManager.h>

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace PlatformDataKernelTests
{
    TEST_CLASS(PersistentMemoryManagerTests)
    {
    public:

        TEST_METHOD(FileMappingTest)
        {
            char* filename = "db.links";

            remove(filename);

			Assert::IsTrue(succeeded(OpenLinks(filename)));

			Assert::IsTrue(succeeded(CloseLinks()));

            remove(filename);
        }

        TEST_METHOD(AllocateFreeLinkTest)
        {
            char* filename = "db.links";

            remove(filename);

			Assert::IsTrue(succeeded(OpenLinks(filename)));

            link_index link = AllocateLink();

            FreeLink(link);

			Assert::IsTrue(succeeded(CloseLinks()));

            remove(filename);
        }

        TEST_METHOD(AttachToUnusedLinkTest)
        {
            char* filename = "db.links";

            remove(filename);

			Assert::IsTrue(succeeded(OpenLinks(filename)));

            link_index link1 = AllocateLink();
            link_index link2 = AllocateLink();

            FreeLink(link1); // Creates "hole" and forces "Attach" to be executed

			Assert::IsTrue(succeeded(CloseLinks()));

            remove(filename);
        }

        TEST_METHOD(DetachToUnusedLinkTest)
        {
            char* filename = "db.links";

            remove(filename);

			Assert::IsTrue(succeeded(OpenLinks(filename)));

            link_index link1 = AllocateLink();
            link_index link2 = AllocateLink();

            FreeLink(link1); // Creates "hole" and forces "Attach" to be executed
            FreeLink(link2); // Removes both links, all "Attached" links forced to be "Detached" here

			Assert::IsTrue(succeeded(CloseLinks()));

            remove(filename);
        }

        TEST_METHOD(GetSetMappedLinkTest)
        {
            char* filename = "db.links";

            remove(filename);

			Assert::IsTrue(succeeded(OpenLinks(filename)));

            link_index mapped = GetMappedLink(0);

            SetMappedLink(0, mapped);

			Assert::IsTrue(succeeded(CloseLinks()));

            remove(filename);
        }
    };
}