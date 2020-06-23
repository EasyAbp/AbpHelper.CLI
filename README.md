# AbpHelper.CLI

[![NuGet](https://img.shields.io/nuget/v/DosSEdo.AbpHelper.svg?style=flat-square)](https://www.nuget.org/packages/DosSEdo.AbpHelper)
[![NuGet Download](https://img.shields.io/nuget/dt/DosSEdo.AbpHelper.svg?style=flat-square)](https://www.nuget.org/packages/DosSEdo.AbpHelper)

AbpHelper is a tool that helps you with developing [Abp vNext](https://abp.io/) applications.

**Make sure to backup your source files before using it!**

## Getting Started

1. Install AbpHelper CLI tool

    `dotnet tool install DosSEdo.AbpHelper -g`

    > If you prefer GUI, there is also a tool with a fancy UI: [AbpHelper.GUI](https://github.com/DosSEdo/AbpHelper.GUI)

1. If you have previously installed it, update it with the following command:

    `dotnet tool update DosSEdo.AbpHelper -g`

1. Use [ABP CLI](https://docs.abp.io/en/abp/latest/CLI) to create an ABP application

    `abp new MyToDo`

1. Create an entity

    ``` csharp
    public class Todo : FullAuditedEntity<Guid>
    {
        public string Content { get; set; }
        public bool Done { get; set; }
    }

    ```

1. Run AbpHelper

    `abphelper generate crud Todo -d C:\MyTodo`

    * `generate crud` is a sub command to generate CRUD files
    * `Todo` specified the entity name we created earlier
    * `-d` specified the **root** directory of the ABP project, which is created by the ABP CLI

    AbpHelper will generate all the CRUD stuffs , even include adding migration and database updating!

1. Run the `DbMigrator` to seed the database
1. Startup your application 
1. Login with the default admin account, and see the magic happens!

    ![running_demo](doc/images/2020-02-10-14-09-22.png)
    
    > If you don't see the TODO menu, check your permissions and make sure the TODO related permissions are granted

## Usage

* Run `abphelper -h` to see the general help
* Similarly, you can use `-h` or `--help` option to see detailed usage of each of the following commands

### Commands

* generate

  Generate files for ABP projects. See 'abphelper generate --help' for details

  * crud

    Generate a set of CRUD related files according to the specified entity

    [Demo GIF](doc/images/crud.gif)

  * service

    Generate service interface and class files according to the specified name

    [Demo GIF](doc/images/service.gif)

  * methods

    Generate service method(s) according to the specified name(s)

    [Demo GIF](doc/images/methods.gif)

  * localization

    Generate localization item(s) according to the specified name(s)

    [Demo GIF](doc/images/localization.gif)

## Extensibility

TODO: Describe how to custom the generating steps, and custom templates.

## Roadmap

- [ ] More CLI parameters
- [x] Support ABP module solutions
- [ ] Support MogoDB generation
- [ ] Support Angular UI generation

