## GradeCalc

GradeCalc is my first personal C# project. I'm using it to learn more about C# and about object orientated-programming in general.
The purpose of this program is to store my grades and calculate their average for me.

### Compilation
#### Linux
Currently I'm using mono's msbuild with [`csproj`](https://docs.microsoft.com/en-us/aspnet/web-forms/overview/deployment/web-deployment-in-the-enterprise/understanding-the-project-file) files and a [`bash script`](https://github.com/clragon/GradeCalc/blob/master/compile.sh) to compile my assembly. 
To compile it yourself, clone the repository and run the bash script on a linux machine that has [`mono-complete`](https://www.mono-project.com/download/stable/) installed. You will also need to install [`Fody`](https://www.nuget.org/packages/Fody/) and [`Costura`](https://www.nuget.org/packages/Costura.Fody/)
#### Windows
If you are on a Windows machine, you can build the `csproj` files manually with `msbuild` which comes pre-installed with [`Visual studio`](https://visualstudio.microsoft.com/).

### Details
GradeCalc is made of 3 parts
- The main cs file which isn't much but the shell for this Application
- The Table-class which is the base for my grade storage
- The CLI class which basically makes the Table-class available to the user on terminal level.

