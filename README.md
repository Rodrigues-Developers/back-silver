# back-silver

## Install .NET 9 SDK

`https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-9.0.100-windows-x64-installer`

## Install the dependencies

`dotnet restore`

## Configure the .env file in the root of the project

## Running the Project

To run this project, type:

`dotnet run`

## Install editorconfig for vscode

Right click on the root of the project and click on "Generate .editorconfig" then copy this code and paste it in the .editorconfig file

```editorconfig

#  CSharp formatting rules:
[*.cs]
csharp_new_line_before_open_brace = none
csharp_new_line_before_else = false
csharp_new_line_before_catch = false
csharp_new_line_before_finally = false

```

## Running with Hot Reload

To run with "Hot reload", type:

`dotnet watch run`

## Accessing the API

Then you can access the API via [localhost:5016](http://localhost:5016/swagger/index.html).
