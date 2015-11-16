# Useful links to understand code

GNU C predefined macros, http://gcc.gnu.org/onlinedocs/cpp/Common-Predefined-Macros.html#Common-Predefined-Macros

Microsoft Visual C++ predefined macros, http://msdn.microsoft.com/en-us/library/b0084kay%28VS.80%29.aspx

# Compile and Run

## On Linux

Build library and test for it:
$ make

It is actually builds into Platform.Data.Kernel.dll on Linux too, because it is referenced .NET Library like Platform.Data.Core.dll (exactly as "Platform.Data.Kernel.dll").

Run test:
$ ./run.sh

To enable debug output put -DDEBUG option into makefile.

Compiled library will be available at `Platform\Platform.Data.Kernel` folder as `Platform.Data.Kernel.dll` file.

To view resulting database file in binary:
$ od -tx2 -w128 db.links | less -S

## On Windows

To build the code on Windows the compiler is required:

1. Visual Studio (Can be installed from https://www.visualstudio.com/ru-ru/products/vs-2015-product-editions.aspx)
2. MinGW (Can be installed from http://www.mingw.org/)

### Using Visual Studio

Open `Platform.Data.Kernel.vcxproj` or `Platform.sln` using Visual Studio (can be found in `Platform` folder)

Press `CTRL+SHIFT+B` or `F6` or use menu item (`Build Solution` or `Build Platform.Data.Kernel`) from `Build` menu.

Compiled library will be available at `Debug`/`Release` folder of in `Platform` folder as `Platform.Data.Kernel.dll` file.

To Run tests in Visual Studio use Test Explorer. Actual test are located at `Platform.Data.Kernel.Tests` and `Platform.Tests` projects.

### Using MinGW

Run `cmd` with administrator rights.

Change directory `cd` to `Platform\Platform.Data.Kernel` folder.

Build library and test for it:
$ mingw32-make

Run test:
$ test

To enable debug output put -DDEBUG option into makefile.

Compiled library will be available at `Platform\Platform.Data.Kernel` folder as `Platform.Data.Kernel.dll` file.

You can use any HEX Viewer/Editor to check for `db.links` structure after the `test` was run.
