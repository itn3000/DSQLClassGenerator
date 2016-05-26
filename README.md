# DSQLClassGenerator

C# class generator for DeclarativeSQL( https://github.com/xin9le/DeclarativeSql )

## Prerequisits

* .NET Framework 4.5.2 or later
* Target Database
    * SQL Server and PostgreSQL are currently supported
* Visual Studio 2015(community is ok)
    * for compile

## Usage

1. edit connection string named 'Target' in DSQLClassGenerator.exe.config to your target database
2. execute the exe

## Commandline options

Basic usage:`DSQLClassGenerator.exe [options]`
options are:
* `-a` or `--amalgamation`
    * generate classes into one file
    * default false
* `-d [dirpath]` or `--outputdir=[dirpath]`
    * output directory for separete files
    * default is `out`
    * if `-a` is specified,just ignored this option
* `-f [filepath]` or `--outputfile=[filepath]`
    * output file path for amalgamated file
    * default is `TableDefinition.cs`
* `-n [namespace]` or `--namespace=[namespace]`
    * generated class namespace
    * default is `Example`
* `-s` or `noschema`
    * flag for ignore schema
    * when it's on,`Schema` attribute will be omitted
* `-c [config filepath]` or `--config [filepath]`
    * external configuration file
    * format is same as DSQLClassGenerator.exe.config`
* `-h` or `--help`
    * just print help then exit

