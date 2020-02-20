# AbpHelper

AbpHelper is a tool to help you with developing Abp vNext applications.

**Make sure **backup** your source files before using it!**.

# Getting Started

1. Install AbpHelper CLI tool

    `dotnet tool install EasyAbp.AbpHelper -g`

1. Use ABP CLI to create a test application

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

    `abphelper generate -e Todo -d C:\MyTodo`

    * `-e` specified the entity name (`Todo` in this example)
    * `-d` specified the **root** directory of the ABP project, which is created by ABP CLI

    AbpHelper will generate all the CRUD stuff , even include adding migration and database updating!

1. Just rebuild your application and run. See the magic happens:)

    ![](doc/images/2020-02-10-14-09-22.png)

# Usage

* Run `abphelper -h` to see the general help
* Run `abphelper generate -h` to see the generate command help

# Extensibility

TODO

# Roadmap

- [ ] More CLI parameters
- [ ] Support ABP module solutions
- [ ] Support MogoDB generation
- [ ] Support Angular UI generation

