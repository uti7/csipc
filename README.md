# csipc

## description
Inter-process communication using window messages (x64) [^1]
[^1]: any implementation that is interoperable with .NET DLLs will work.

## how to use
  - compile and create the DLL.
     * example of using csc in the PWSH console:
      ```
      PS> & C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /out:csipc.IPC.dll /target:library .\csipc\Class1.cs
      ```
  - load the DLL in your source code.
  - see below source code for details
      * main unit:
        * .\csipc/Class1.cs
      * usage:
        * example of using C#:
          * ./WindowsFormsApplication1/Form1.cs

## note
The methods for implementing inter-process communication in the .NET environment are well-established. There's no real reason to use this DLL in this day and age.
