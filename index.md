# Links Platform ([русская версия](index.ru.md))
Holistic system for storage and transformation of information (in development) based on associative model of data.

## Prerequisites
* Linux, macOS or Windows operating system.
* [.NET Core](https://www.microsoft.com/net) SDK with version 2.2 or later.
* [MonoDevelop](https://www.monodevelop.com/), [Visual Studio](https://visualstudio.microsoft.com) or any other [IDE](https://en.wikipedia.org/wiki/Integrated_development_environment) or just a [text editor](https://en.wikipedia.org/wiki/Text_editor).

## Links Platform's NuGet packages

### Main packages

#### [Platform.Data](https://linksplatform.github.io/Data)
Common interfaces and classes for both [Doublets](https://linksplatform.github.io/Data.Doublets) and [Triplets](https://linksplatform.github.io/Data.Triplets).

#### [Platform.Data.Doublets](https://linksplatform.github.io/Data.Doublets)
#### [Platform.Data.Triplets](https://linksplatform.github.io/Data.Triplets)
#### [Platform.Data.Triplets.Kernel](https://linksplatform.github.io/Data.Triplets.Kernel)

### Auxiliary packages

#### [Platform.Data.Memory](https://linksplatform.github.io/Memory)
Platform.Data.Memory class library contains classes for memory management simplification. There you will find multiple implementations of [IMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.IMemory.html) interface.

The data can be accessed using [the raw pointer](https://linksplatform.github.io/Memory/api/Platform.Memory.IDirectMemory.html) or [by element's index](https://linksplatform.github.io/Memory/api/Platform.Memory.IArrayMemory-1.html) and can be stored in volatile memory:
* [HeapResizableDirect](https://linksplatform.github.io/Memory/api/Platform.Memory.HeapResizableDirectMemory.html),
* [ArrayMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.ArrayMemory-1.html)

or in non-volatile memory:
* [FileMappedResizableDirectMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.FileMappedResizableDirectMemory.html),
* [TemporaryFileMappedResizableDirectMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.TemporaryFileMappedResizableDirectMemory.html),
* [FileArrayMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.FileArrayMemory-1.html).

#### [Platform.Data.Communication](https://linksplatform.github.io/Communication)
#### [Platform.Collections.Methods](https://linksplatform.github.io/Collections.Methods)
#### [Platform.IO](https://linksplatform.github.io/IO)
#### [Platform.Unsafe](https://linksplatform.github.io/Unsafe)
#### [Platform.Numbers](https://linksplatform.github.io/Numbers)
#### [Platform.Converters](https://linksplatform.github.io/Converters)
#### [Platform.Scopes](https://linksplatform.github.io/Scopes)
#### [Platform.Singletons](https://linksplatform.github.io/Singletons)
#### [Platform.Reflection](https://linksplatform.github.io/Reflection)
#### [Platform.Threading](https://linksplatform.github.io/Threading)
#### [Platform.Collections](https://linksplatform.github.io/Collections)
#### [Platform.Diagnostics](https://linksplatform.github.io/Diagnostics)
#### [Platform.Counters](https://linksplatform.github.io/Counters)
#### [Platform.Setters](https://linksplatform.github.io/Setters)
#### [Platform.Comparers](https://linksplatform.github.io/Comparers)
#### [Platform.Random](https://linksplatform.github.io/Random)
#### [Platform.Timestamps](https://linksplatform.github.io/Timestamps)
#### [Platform.Ranges](https://linksplatform.github.io/Ranges)
#### [Platform.Disposables](https://linksplatform.github.io/Disposables)
#### [Platform.Exceptions](https://linksplatform.github.io/Exceptions)
#### [Platform.Interfaces](https://linksplatform.github.io/Interfaces)
