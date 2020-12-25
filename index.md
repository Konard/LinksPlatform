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
An implementation of Doublets.

#### [Platform.Data.Triplets](https://linksplatform.github.io/Data.Triplets)
A C# adapter of Triplets.

#### [Platform.Data.Triplets.Kernel](https://linksplatform.github.io/Data.Triplets.Kernel)
A native Triplets implementation.

### Auxiliary packages

#### [Platform.Data.Memory](https://linksplatform.github.io/Memory)
Platform.Data.Memory class library contains classes for memory management simplification. There you can find multiple implementations of [IMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.IMemory.html) interface.

The data can be accessed using [the raw pointer](https://linksplatform.github.io/Memory/api/Platform.Memory.IDirectMemory.html) or [by element's index](https://linksplatform.github.io/Memory/api/Platform.Memory.IArrayMemory-1.html) and can be stored in volatile memory:
* [HeapResizableDirect](https://linksplatform.github.io/Memory/api/Platform.Memory.HeapResizableDirectMemory.html),
* [ArrayMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.ArrayMemory-1.html)

or in non-volatile memory:
* [FileMappedResizableDirectMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.FileMappedResizableDirectMemory.html),
* [TemporaryFileMappedResizableDirectMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.TemporaryFileMappedResizableDirectMemory.html),
* [FileArrayMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.FileArrayMemory-1.html).

#### [Platform.Data.Communication](https://linksplatform.github.io/Communication)
Platform.Data.Communication class library contains classes for communication simplification supporting different protocols.

##### Gexf
XML-mapping classes for [Graph Exchange XML Format](https://gephi.org/gexf/format/).

##### Udp
`UdpSender` and `UdpReceiver` classes to simplify implementation of different roles of `UdpClient`.

##### Xml
A `Serializer` class to help with XML serialization and deserialization.

#### [Platform.Collections.Methods](https://linksplatform.github.io/Collections.Methods)
Platform.Collections.Methods class library contains classes with storage/state agnostic implementation of lists and trees.

#### [Platform.IO](https://linksplatform.github.io/IO)
Platform.Collections.Methods class library contains classes ... .

#### [Platform.Unsafe](https://linksplatform.github.io/Unsafe)
Platform.Collections.Methods class library contains classes ... .

#### [Platform.Numbers](https://linksplatform.github.io/Numbers)
Platform.Collections.Methods class library contains classes ... .

#### [Platform.Converters](https://linksplatform.github.io/Converters)
Platform.Collections.Methods class library contains classes ... .

#### [Platform.Scopes](https://linksplatform.github.io/Scopes)
Platform.Collections.Methods class library contains classes ... .

#### [Platform.Singletons](https://linksplatform.github.io/Singletons)
Platform.Collections.Methods class library contains classes ... .

#### [Platform.Reflection](https://linksplatform.github.io/Reflection)
Platform.Collections.Methods class library contains classes ... .

#### [Platform.Threading](https://linksplatform.github.io/Threading)
Platform.Collections.Methods class library contains classes ... .

#### [Platform.Collections](https://linksplatform.github.io/Collections)
Platform.Collections.Methods class library contains classes ... .

#### [Platform.Diagnostics](https://linksplatform.github.io/Diagnostics)
Platform.Collections.Methods class library contains classes ... .

#### [Platform.Counters](https://linksplatform.github.io/Counters)
Platform.Collections.Methods class library contains classes ... .

#### [Platform.Setters](https://linksplatform.github.io/Setters)
Platform.Collections.Methods class library contains classes ... .

#### [Platform.Comparers](https://linksplatform.github.io/Comparers)
Platform.Collections.Methods class library contains classes ... .

#### [Platform.Random](https://linksplatform.github.io/Random)
Platform.Collections.Methods class library contains classes ... .

#### [Platform.Timestamps](https://linksplatform.github.io/Timestamps)
Platform.Collections.Methods class library contains classes ... .

#### [Platform.Ranges](https://linksplatform.github.io/Ranges)
Platform.Collections.Methods class library contains `Range` struct with Minimum and Maximum fields.

#### [Platform.Disposables](https://linksplatform.github.io/Disposables)
Platform.Collections.Methods class library contains classes and interfaces that help to make objects disposable in a fast, short, easy and safe way.

##### DisposableBase
`Platform.Disposables.DisposableBase` abstract class tries to dispose the object at both on instance destruction and `OnProcessExit` whatever comes first even if `Dispose` method was not called anywhere by user.

##### Yet another IDisposable
The `Platform.Disposables.IDisposable` interface extends the `System.IDisposable` with `IsDisposed` property and `Destruct` method.

#### [Platform.Exceptions](https://linksplatform.github.io/Exceptions)
Platform.Collections.Methods class library contains classes ... .

#### [Platform.Interfaces](https://linksplatform.github.io/Interfaces)
Platform.Collections.Methods class library contains common interfaces that did not fit in any major category.
