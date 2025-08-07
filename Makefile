install:
	dotnet tool install -g dotnet-format

lint:
	dotnet new sln --name ConwayTemp --force
	find . -name '*.csproj' -exec dotnet sln ConwayTemp.sln add {} \;
	dotnet format ConwayTemp.sln --verbosity minimal

