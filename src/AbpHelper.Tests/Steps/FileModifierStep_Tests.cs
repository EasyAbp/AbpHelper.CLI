using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace EasyApp.AbpHelper.Tests.Steps
{
    public class FileModifierStep_Tests : StepTestsBase
    {
        public FileModifierStep_Tests(ITestOutputHelper output)
        {
            _output = output;
            _step = GetRequiredService<FileModifierStep>();
        }

        private readonly ITestOutputHelper _output;
        private readonly FileModifierStep _step;

        private const string DefaultFileContents = @"1
2
3
4
5
";

        private async Task<string> UsingTempFile(string contents, Func<string, Task> action)
        {
            string file = null;
            try
            {
                file = Path.GetTempFileName();
                await File.WriteAllTextAsync(file, contents);
                await action(file);
                return await File.ReadAllTextAsync(file);
            }
            finally
            {
                if (file != null) File.Delete(file);
            }
        }

        [Fact]
        private async Task Delete()
        {
            await UsingWorkflowContext(async ctx =>
            {
                await UsingTempFile(DefaultFileContents, async file =>
                {
                    // Arrange
                    var modifications = new List<Modification> {new Deletion(1, 3)};
                    ctx.SetVariable("Modifications", modifications);
                    ctx.SetVariable(FileFinderStep.DefaultFileParameterName, file);

                    // Act
                    await _step.ExecuteAsync(ctx, CancellationToken.None);

                    // Assert
                    var contents = await File.ReadAllTextAsync(file);
                    contents.ShouldBe(@"4
5
");
                });
            });
        }

        [Fact]
        private async Task Insert_After()
        {
            await UsingWorkflowContext(async ctx =>
            {
                await UsingTempFile(DefaultFileContents, async file =>
                {
                    // Arrange
                    var modifications = new List<Modification> {new Insertion(5, "abc\r\n", InsertPosition.After)};
                    ctx.SetVariable("Modifications", modifications);
                    ctx.SetVariable(FileFinderStep.DefaultFileParameterName, file);

                    // Act
                    await _step.ExecuteAsync(ctx, CancellationToken.None);

                    // Assert
                    var contents = await File.ReadAllTextAsync(file);
                    contents.ShouldBe(@"1
2
3
4
5
abc
");
                });
            });
        }

        [Fact]
        private async Task Insert_Before()
        {
            await UsingWorkflowContext(async ctx =>
            {
                await UsingTempFile(DefaultFileContents, async file =>
                {
                    // Arrange
                    var modifications = new List<Modification>{new Insertion(1, "abc\r\n")};
                    ctx.SetVariable("Modifications", modifications);
                    ctx.SetVariable(FileFinderStep.DefaultFileParameterName, file);

                    // Act
                    await _step.ExecuteAsync(ctx, CancellationToken.None);

                    // Assert
                    var contents = await File.ReadAllTextAsync(file);
                    contents.ShouldBe(@"abc
1
2
3
4
5
");
                });
            });
        }

        [Fact]
        private async Task Multi_Insertions_With_Same_StartLine()
        {
            await UsingWorkflowContext(async ctx =>
            {
                await UsingTempFile(DefaultFileContents, async file =>
                {
                    // Arrange
                    var modifications = new List<Modification>
                    {
                        new Insertion(1, "abc\r\n"),
                        new Insertion(1, "def\r\n")
                    };
                    ctx.SetVariable("Modifications", modifications);
                    ctx.SetVariable(FileFinderStep.DefaultFileParameterName, file);

                    // Act
                    await _step.ExecuteAsync(ctx, CancellationToken.None);

                    // Assert
                    var contents = await File.ReadAllTextAsync(file);
                    contents.ShouldBe(@"abc
def
1
2
3
4
5
");
                });
            });
        }

        [Fact]
        private async Task Multi_Modifications()
        {
            await UsingWorkflowContext(async ctx =>
            {
                await UsingTempFile(DefaultFileContents, async file =>
                {
                    // Arrange
                    var modifications = new List<Modification>
                    {
                        new Insertion(1, "a\r\n"),
                        new Deletion(2, 3),
                        new Replacement(4, 4, "b\r\n"),
                        new Insertion(5, "c", InsertPosition.After)
                    };
                    ctx.SetVariable("Modifications", modifications);
                    ctx.SetVariable(FileFinderStep.DefaultFileParameterName, file);

                    // Act
                    await _step.ExecuteAsync(ctx, CancellationToken.None);

                    // Assert
                    var contents = await File.ReadAllTextAsync(file);
                    contents.ShouldBe(@"a
1
b
5
c");
                });
            });
        }

        [Fact]
        private async Task Negative_Lines()
        {
            await UsingWorkflowContext(async ctx =>
            {
                await UsingTempFile(DefaultFileContents, async file =>
                {
                    // Arrange
                    var modifications = new List<Modification>
                    {
                        new Insertion(-4, "a\r\n"),
                        new Deletion(-3, -2),
                        new Replacement(-1, -1, "b\r\n")
                    };
                    ctx.SetVariable("Modifications", modifications);
                    ctx.SetVariable(FileFinderStep.DefaultFileParameterName, file);

                    // Act
                    await _step.ExecuteAsync(ctx, CancellationToken.None);

                    // Assert
                    var contents = await File.ReadAllTextAsync(file);
                    contents.ShouldBe(@"1
a
2
b
");
                });
            });
        }

        [Fact]
        private async Task OutOfRange_Lines()
        {
            await UsingWorkflowContext(async ctx =>
            {
                await UsingTempFile(DefaultFileContents, async file =>
                {
                    // Arrange
                    var modifications = new List<Modification>
                    {
                        new Insertion(0, "a\r\n"),
                        new Insertion(-6, "a\r\n"),
                        new Deletion(0, 1),
                        new Deletion(1, 6),
                        new Replacement(-6, 1, "a"),
                        new Replacement(7, 8, "a")
                    };
                    ctx.SetVariable("Modifications", modifications);
                    ctx.SetVariable(FileFinderStep.DefaultFileParameterName, file);

                    // Act
                    var ex = await Assert.ThrowsAsync<InvalidModificationException>(() => _step.ExecuteAsync(ctx, CancellationToken.None));

                    // Assert
                    _output.WriteLine(string.Join(Environment.NewLine, ex.Errors));
                    ex.Errors.Count.ShouldBe(7);
                });
            });
        }

        [Fact]
        private async Task Overlap_Deletion_And_Replacement()
        {
            await UsingWorkflowContext(async ctx =>
            {
                await UsingTempFile(DefaultFileContents, async file =>
                {
                    // Arrange
                    var modifications = new List<Modification>
                    {
                        new Deletion(1, 3),
                        new Replacement(2, 4, "abc")
                    };
                    ctx.SetVariable("Modifications", modifications);
                    ctx.SetVariable(FileFinderStep.DefaultFileParameterName, file);

                    // Act
                    var ex = await Assert.ThrowsAsync<InvalidModificationException>(() => _step.ExecuteAsync(ctx, CancellationToken.None));

                    // Assert
                    _output.WriteLine(string.Join(Environment.NewLine, ex.Errors));
                    ex.Errors.Count.ShouldBe(1);
                });
            });
        }

        [Fact]
        private async Task Overlap_Insertion_And_Deletion()
        {
            await UsingWorkflowContext(async ctx =>
            {
                await UsingTempFile(DefaultFileContents, async file =>
                {
                    // Arrange
                    var modifications = new List<Modification>
                    {
                        new Insertion(1, "a\r\n"),
                        new Deletion(1, 2)
                    };
                    ctx.SetVariable("Modifications", modifications);
                    ctx.SetVariable(FileFinderStep.DefaultFileParameterName, file);

                    // Act
                    var ex = await Assert.ThrowsAsync<InvalidModificationException>(() => _step.ExecuteAsync(ctx, CancellationToken.None));

                    // Assert
                    _output.WriteLine(string.Join(Environment.NewLine, ex.Errors));
                    ex.Errors.Count.ShouldBe(1);
                });
            });
        }

        [Fact]
        private async Task Overlap_Insertion_And_Replacement()
        {
            await UsingWorkflowContext(async ctx =>
            {
                await UsingTempFile(DefaultFileContents, async file =>
                {
                    // Arrange
                    var modifications = new List<Modification>
                    {
                        new Insertion(1, "a\r\n"),
                        new Replacement(1, 2, "abc")
                    };
                    ctx.SetVariable("Modifications", modifications);
                    ctx.SetVariable(FileFinderStep.DefaultFileParameterName, file);

                    // Act
                    var ex = await Assert.ThrowsAsync<InvalidModificationException>(() => _step.ExecuteAsync(ctx, CancellationToken.None));

                    // Assert
                    _output.WriteLine(string.Join(Environment.NewLine, ex.Errors));
                    ex.Errors.Count.ShouldBe(1);
                });
            });
        }

        [Fact]
        private async Task Replace()
        {
            await UsingWorkflowContext(async ctx =>
            {
                await UsingTempFile(DefaultFileContents, async file =>
                {
                    // Arrange
                    var modifications = new List<Modification> {new Replacement(1, 3, "abc\r\n")};
                    ctx.SetVariable("Modifications", modifications);
                    ctx.SetVariable(FileFinderStep.DefaultFileParameterName, file);

                    // Act
                    await _step.ExecuteAsync(ctx, CancellationToken.None);

                    // Assert
                    var contents = await File.ReadAllTextAsync(file);
                    contents.ShouldBe(@"abc
4
5
");
                });
            });
        }

        [Fact]
        private async Task StartLine_Greater_Than_EndLine_Lines()
        {
            await UsingWorkflowContext(async ctx =>
            {
                await UsingTempFile(DefaultFileContents, async file =>
                {
                    // Arrange
                    var modifications = new List<Modification>
                    {
                        new Deletion(2, 1),
                        new Deletion(-1, -2),
                        new Replacement(4, 3, "a"),
                        new Replacement(-3, -4, "a")
                    };
                    ctx.SetVariable("Modifications", modifications);
                    ctx.SetVariable(FileFinderStep.DefaultFileParameterName, file);

                    // Act
                    var ex = await Assert.ThrowsAsync<InvalidModificationException>(() => _step.ExecuteAsync(ctx, new CancellationToken()));

                    // Assert
                    _output.WriteLine(string.Join(Environment.NewLine, ex.Errors));
                    ex.Errors.Count.ShouldBe(4);
                });
            });
        }
    }
}