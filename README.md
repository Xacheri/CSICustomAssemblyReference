# Custom Assembly Reference Guide

The purpose of this repository is to provide a guide and reference for developing Infor Syteline Custom Assemblies/IDO Extension Classes.
This includes both instructional markdown documents as well as C# utility classes that you can use while developing Custom Assemblies.

# Class/Interface Reference

Find a quick reference guide for relevant Classes/Interfaces/Methods [here]('./reference.md')

# Quick Start

1. Create an empty C# project (Class Library) in Visual Studio
![Create a class library](./images/newproject.png "Create a new project")
![Name your project](./images/newproject_name.png "Name the project")

2. Import the SyteLine DLLs
In order to access Mongoose/SyteLine code, we must import it. There are 8 DLLs that must be imported:
- IDOCore.dll 
- IDOProtocol.dll 
- IDORequestClient.dll 
- MGScriptMgr.dll 
- MGShared.dll 
- MGSharedResources.dll 
- WSEnums.dll 
- WSFormServerProtocol.dll 

These DLLs can be found on the utility server in:
```sh
<InstallerDirectory>/<DatedFolder>/Server/WinStudio/
```

![Get the DLL from the server](./images/dll_location.png "Find the DLLs")
![Import the DLL](./images/importthedll.gif "Import the DLL")

3. Set Up A Testing Console App
Create a new C# console app in the same solution. Import those same DLLs to the console app. You will use this console app to test your assembly outside of the Syteline Frontend.

4. Create the Static Methods
These methods contain the actual "work" that the Custom Assembly does. Since we make 
these static, we can access them without instantiating the whole Mongoose infrastructure.

5. Link the Static Methods into the non-static methods with decorators
These are the methods that Syteline will call. Simply pass the base.Context to the static method as well as any parameters 

6. Build the project
Building the project will generate a .dll and a symbols file that you will use when importing the CA into the Syteline System.

7. Import the project into SyteLine 
Use the "IDO Extension Class Assemblies" form to import the Assembly. You must include 
both the Symbols as well as the built .dll file.



# Session-Related Errors
-- Show the regular way, with the session management. Then show using(){} method to prevent it.
