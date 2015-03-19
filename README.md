# DotNetExpect


### What is DotNetExpect?
Inspired by the design of the [Expect library](http://en.wikipedia.org/wiki/Expect), DotNetExpect is a .NET library that provides console input/control of console-based Windows applications. Unlike other solutions that can only redirect the standard streams of child processes, DotNetExpect directly accesses the console screen buffer of applications so that regardless of the mechanism the application uses to write to the console (either by writing to the standard streams or by invoking low-level console output functions), DotNetExpect is able to work with a much wider range of console applications.

DotNetExpect is designed with the intention of exposing a simple, easy to use API so that automation of console applications becomes a trivial task.

### How do I use DotNetExpect?
The DotNetExpect library exposes the primary class of the library interface, which is named `ChildProcess`. All operations regarding the creation/termination of console applications as well as reading and writing to an application's console is performed using this class.

Here is a short example showing how the Windows Telnet client application can be automated to log into a Linux machine and retrieve a directory listing:

```csharp
// Create a new instance of ChildProcess. This will spawn telnet.exe and connect its console to the library.
// ChildProcess implements IDisposable, so generally you will want to instantiate this class in a using statement.
using(ChildProcess childProc = new ChildProcess("telnet.exe", "192.168.1.1"))
{
	// Wait for the login prompt to appear
	childProc.Read("login:");

	// Write the user name with which to login to the console input
	childProc.WriteLine("root");

	// Wait for the password prompt to appear
	childProc.Read("Password:");

	// Write the password
	childProc.WriteLine("MySecretPassword");

	// Wait for the root shell prompt to appear
	childProc.Read("#");

	// Issue the "ls" command to output the directory contents
	childProc.WriteLine("ls");

	// Wait until the root shell prompt appears and return the directory contents
	string dirContents = childProc.Read("#");

	// Display the directory contents to our console
	Console.WriteLine(dirContents);
}
```

Many other properties and methods are exposed on the `ChildProcess` class, including regular expression output matching and process management (such as terminating a child process and retrieving its exit code, etc.). Documentation is provided for each public member of the library and the library was designed to leverage Visual Studio's IntelliSense functionality for discoverability and ease of use.

### How do I build DotNetExpect?
DotNetExpect is Visual Studio 2013 project, so it is possible to build the project by merely opening up the solution (.sln) file for the library and building it or invoking MSBuild from the command line. Binaries may be provided at some point depending on demand.

### What are the system requirements for using DotNetExpect?
DotNetExpect is written against .NET framework 3.5, so that version of the framework or higher must be installed. DotNetExpect uses PowerShell as a proxy process (see the section "How does DotNetExpect work?" below for details), so PowerShell must be installed and on the system PATH. Any version of PowerShell should work.

### What is the license for DotNetExpect?
DotNetExpect is licensed under the LGPL version 3.0.

### Are there tests available?
A small suite of unit and integration tests is included in the Visual Studio solution. These tests are written against NUnit 2.6.4, so you will need to have NUnit installed to build and run these tests.

### How do I report bugs or request enhancements?
Please [create an issue](https://github.com/CBonnell/dotnetexpect/issues).

### How does DotNetExpect work?
When an instance of `ChildProcess` is created, a randomly-named named pipe is created and a "proxy process" is launched. The proxy process that is created is PowerShell, which then loads the DotNetExpect assembly and invokes the code in the DotNetExpect library that runs the proxy process server. The proxy process server connects to the named pipe and services commands sent from the library. Whenever a method or property is called on `ChildProcess`, that request is sent via the named pipe to the proxy process, which then services the request.

The rationale for using a proxy process is so that graphical applications (which do not have a console) or console applications that still want to have console input/output of their own will function normally, as the proxy process is created with its own console entirely separate from the parent (calling) process. It is the proxy process that actually spawns the child process and shares its console with the child process. PowerShell was chosen due to its ability to dynamically load and call into .NET assemblies, obviating the need to author a separate executable for the proxy process. In this regard, PowerShell acts as a host process for running .NET code.



