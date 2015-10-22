#include "stdafx.h"
#include "CppUnitTest.h"

#include <PersistentMemoryManager.h>
#include <Link.h>

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace PlatformDataKernelTests
{
    unsigned_integer thingVisitorCounter;
    unsigned_integer isAVisitorCounter;
    unsigned_integer linkVisitorCounter;

    TEST_CLASS(LinkTests)
    {
    public:
        static void ThingVisitor(link_index linkIndex)
        {
            thingVisitorCounter += linkIndex;
        }

        static void IsAVisitor(link_index linkIndex)
        {
            isAVisitorCounter += linkIndex;
        }

        static void LinkVisitor(link_index linkIndex)
        {
            linkVisitorCounter += linkIndex;
        }

        TEST_METHOD(CreateDeleteLinkTest)
        {
            char* filename = "db.links";

            remove(filename);

            InitPersistentMemoryManager();

            Assert::IsTrue(succeeded(OpenStorageFile(filename)));

            Assert::IsTrue(succeeded(SetStorageFileMemoryMapping()));

            link_index link1 = CreateLink(itself, itself, itself);

            DeleteLink(link1);

            Assert::IsTrue(succeeded(ResetStorageFileMemoryMapping()));

            Assert::IsTrue(succeeded(CloseStorageFile()));

            remove(filename);
        }

        TEST_METHOD(DeepCreateUpdateDeleteLinkTest)
        {
            char* filename = "db.links";

            remove(filename);

            InitPersistentMemoryManager();

            Assert::IsTrue(succeeded(OpenStorageFile(filename)));

            Assert::IsTrue(succeeded(SetStorageFileMemoryMapping()));

            link_index isA = CreateLink(itself, itself, itself);
            link_index isNotA = CreateLink(itself, itself, isA);
            link_index link = CreateLink(itself, isA, itself);
            link_index thing = CreateLink(itself, isNotA, link);

            Assert::IsTrue(GetLinksCount() == 4);

            Assert::IsTrue(GetTargetIndex(isA) == isA);

            isA = UpdateLink(isA, isA, isA, link); // Произведено замыкание

            Assert::IsTrue(GetTargetIndex(isA) == link);

            DeleteLink(isA); // Одна эта операция удалит все 4 связи

            Assert::IsTrue(GetLinksCount() == 0);

            Assert::IsTrue(succeeded(ResetStorageFileMemoryMapping()));

            Assert::IsTrue(succeeded(CloseStorageFile()));

            remove(filename);
        }

        TEST_METHOD(LinkReferersWalkTest)
        {
            char* filename = "db.links";

            remove(filename);

            InitPersistentMemoryManager();

            Assert::IsTrue(succeeded(OpenStorageFile(filename)));

            Assert::IsTrue(succeeded(SetStorageFileMemoryMapping()));

            link_index isA = CreateLink(itself, itself, itself);
            link_index isNotA = CreateLink(itself, itself, isA);
            link_index link = CreateLink(itself, isA, itself);
            link_index thing = CreateLink(itself, isNotA, link);
            isA = UpdateLink(isA, isA, isA, link);

            Assert::IsTrue(GetLinkNumberOfReferersBySource(thing) == 1);
            Assert::IsTrue(GetLinkNumberOfReferersByLinker(isA) == 2);
            Assert::IsTrue(GetLinkNumberOfReferersByTarget(link) == 3);

            thingVisitorCounter = 0;
            isAVisitorCounter = 0;
            linkVisitorCounter = 0;

            WalkThroughAllReferersBySource(thing, ThingVisitor);
            WalkThroughAllReferersByLinker(isA, IsAVisitor);
            WalkThroughAllReferersByTarget(link, LinkVisitor);

            Assert::IsTrue(thingVisitorCounter == 4);
            Assert::IsTrue(isAVisitorCounter == (1 + 3));
            Assert::IsTrue(linkVisitorCounter == (1 + 3 + 4));

            Assert::IsTrue(succeeded(ResetStorageFileMemoryMapping()));

            Assert::IsTrue(succeeded(CloseStorageFile()));

            remove(filename);
        }
    };
}