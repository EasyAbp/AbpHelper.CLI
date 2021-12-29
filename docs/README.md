# AbpHelper.CLI

[![ABP version](https://img.shields.io/badge/dynamic/xml?style=flat-square&color=yellow&label=abp&query=%2F%2FProject%2FPropertyGroup%2FAbpVersion&url=https%3A%2F%2Fraw.githubusercontent.com%2FEasyAbp%2FAbpHelper.CLI%2Fmaster%2FDirectory.Build.props)](https://abp.io)
[![NuGet](https://img.shields.io/nuget/v/EasyAbp.AbpHelper.svg?style=flat-square)](https://www.nuget.org/packages/EasyAbp.AbpHelper)
[![NuGet Download](https://img.shields.io/nuget/dt/EasyAbp.AbpHelper.svg?style=flat-square)](https://www.nuget.org/packages/EasyAbp.AbpHelper)
[![Discord online](https://badgen.net/discord/online-members/S6QaezrCRq?label=Discord)](https://discord.gg/S6QaezrCRq)
[![GitHub stars](https://img.shields.io/github/stars/EasyAbp/AbpHelper.CLI?style=social)](https://www.github.com/EasyAbp/AbpHelper.CLI)

AbpHelper is a tool that help you with developing Abp vNext applications.

**Make sure to backup your source files before using it!**

## Getting Started

1. Install AbpHelper CLI tool

    `dotnet tool install EasyAbp.AbpHelper -g`

    > If you prefer GUI, there is also a tool with a fancy UI: [AbpHelper.GUI](https://github.com/EasyAbp/AbpHelper.GUI)

1. If you have previously installed it, update it with the following command:

    `dotnet tool update EasyAbp.AbpHelper -g`

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

    ![running_demo](/docs/images/2020-02-10-14-09-22.png)

    > If you don't see the TODO menu, check your permissions and make sure the TODO related permissions are granted

## Usage

* Run `abphelper -h` to see the general help
* Similarly, you can use `-h` or `--help` option to see detailed usage of each of the following commands

### Commands

* generate

  Generate files for ABP projects. See 'abphelper generate --help' for details

  * crud

    Generate a set of CRUD related files according to the specified entity

    [Demo GIF](/docs/images/crud.gif)

  * service

    Generate service interface and class files according to the specified name

    [Demo GIF](/docs/images/service.gif)

  * methods

    Generate service method(s) according to the specified name(s)

    [Demo GIF](/docs/images/methods.gif)

  * localization

    Generate localization item(s) according to the specified name(s)

    [Demo GIF](/docs/images/localization.gif)

  * controller

    Generate controller class and methods according to the specified service

    [Demo GIF](/docs/images/controller.gif)

* ef

  A shortcut to run 'dotnet ef' commands. See 'abphelper ef --help' for details

  [Demo GIF](/docs/images/ef.gif)

* module

  Help quickly add/update/remove ABP modules. See 'abphelper module --help' for details

  [Demo GIF](/docs/images/module.gif)

