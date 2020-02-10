# AbpHelper

AbpHelper is a tool to help you with developing Abp vNext applications.

**Make sure **backup** your source files before using it!**.

# Getting Started

1. Install AbpHelper CLI tool

    `dotnet tool install AbpHelper -g --version 0.1.2-alpha`

1. Use ABP CLI to create a test application

    `abp new MyToDo`

1. Create an entity(e.g. `Todo`)

    ``` csharp
    public class Todo : FullAuditedEntity<Guid>
    {
        public string Content { get; set; }
        public bool Done { get; set; }
    }

    ```

1. Run AbpHelper

    `abphelper c:\MyTodo Todo.cs`

    AbpHelper will generate all the CRUD stuff , even include adding migration and database updating!

1. Just rebuild your application and run. See the magic happens:)

    ![](doc/images/2020-02-10-14-09-22.png)

# Usage

`abphelper abp_solution_dir entity_file_name`

* `abp_solution_dir`: ABP souliton directory(e.g. `c:\MyTodo` )
* `entity_file_name`: The entity file name(e.g. `Todo.cs` )

# Extensibility

TODO

# Roadmap

- [ ] More CLI parameters
- [ ] Support ABP module solutions
- [ ] Support MogoDB generation
- [ ] Support Angular UI generation

