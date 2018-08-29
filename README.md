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
If you are on a Windows machine, you can build the csproj  files manually with msbuild which comes pre-installed with [`Visual studio`](https://visualstudio.microsoft.com/).

### Details
GradeCalc is made of 3 parts
- The main cs file which isn't much but the shell for this application
- The table-class which is the base for my grade storage
- The CLI class which basically makes the table-class available to the user on terminal level.

