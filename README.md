# scribi
(script binder)

By [Benjamin Proemmer](https://github.com/proemmer)

Scribi is the main application to run scripts and automatic generate an WebApi and SignalR interface for them.


#Deployment

Based on the blog of [Scott Hanselman](http://www.hanselman.com/blog/SelfcontainedNETCoreApplications.aspx), you have two possibilities to deploy the app. 

## Framework-dependent (FDD)

project.json 

```
"dependencies": {
    "Microsoft.NETCore.App": {
      "version": "1.0.1" ,
      "type": "platform"  
    },
}
```

### dotnet build

```
dotnet build
```

### dotnet publish

```
dotnet publish
```

after this you will find a Scribi.dll which you can run with dotnet.exe.

## Framework-dependent (SCD)

project.json 

### Remove the "type"="platform" from dependencies.

```
"dependencies": {
    "Microsoft.NETCore.App": {
      "version": "1.0.1" 
    },
}
```

### Add runtimes (get Id's from [here](https://github.com/dotnet/corefx/blob/master/pkg/Microsoft.NETCore.Platforms/runtime.json))
```
"runtimes": {
     "win10-x64": {},
     "osx.10.10-x64": {},
     "ubuntu.14.04-x64": {}
   }
```
### dotnet build

```
dotnet build -r win10-x64
dotnet build -r osx.10.10-x64
dotnet build -r ubuntu.14.04-x64
```

### dotnet publish

```
dotnet publish -c release -r win10-x64
dotnet publish -c release -r osx.10.10-x64
dotnet publish -c release -r ubuntu.14.04-x64
```

Once this is done, you get on folder for every runtime in ..\Scribi\bin\Release\netcoreapp1.0\[Runtime].
In Windows10 you get in the win10-x64 folder the file Scribi.exe.


# Links

- [License](LICENSE)
- [Commercial Support/License](http://www.insite-gmbh.de/kontakt.html)
