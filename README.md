# DllInjector

A command line tool to inject any dll into another process written in C#

## Environment
1. Download and install .NET 6.0 SDK at [.NET official website](https://dotnet.microsoft.com/en-us/download)

## Build
```bat
git clone https://github.com/cynault/DllInjector.git
cd DllInjector
dotnet publish -o publish -c Release --runtime win-x64 --self-contained
```
Then you can find a ```DllInject.exe``` in publish folder

## Usage
```bat
.\DllInject.exe -pid <target-process-pid> -dll <path to inject dll>
```
