## GradeCalc

GradeCalc is my first personal C# project. I'm using it to learn more about C# and about object orientated-programming in general.
The purpose of this program is to store grades and calculate their average.

### Compilation
Currently I'm using msbuild with csproj files and a bash script to compile my assembly. 
Regardless of the OS, the following step is mandatory:

You will need to download [`Fody`](https://www.nuget.org/packages/Fody/) and [`Costura`](https://www.nuget.org/packages/Costura.Fody/) and put them in a new directory called `packages` inside the master directory.
#### Linux
To compile on Linux just run [`the bash script`](https://github.com/clragon/GradeCalc/blob/master/compile.sh) but keep in mind you have to have [`mono-complete`](https://www.mono-project.com/download/stable/) installed.
#### Windows
If you are on a Windows machine, you can build the [`csproj`](https://docs.microsoft.com/en-us/aspnet/web-forms/overview/deployment/web-deployment-in-the-enterprise/understanding-the-project-file)  files manually with [`msbuild`](https://docs.microsoft.com/visualstudio/msbuild/msbuild-concepts?view=vs-2017) which comes pre-installed with [`Visual studio`](https://visualstudio.microsoft.com/).

### Details
GradeCalc is made of 3 parts
- The main cs file which isn't much but the shell for this Application
- The Table-class which is the base for my grade storage
- The CLI class which basically makes the Table-class available to the user on terminal level.

