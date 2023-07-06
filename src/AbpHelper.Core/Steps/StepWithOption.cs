using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EasyAbp.AbpHelper.Core.Commands;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;

namespace EasyAbp.AbpHelper.Core.Steps
{
    public abstract class StepWithOption : Step
    {
        private static readonly Dictionary<string, IReadOnlyList<string>> ExcludeDirectorySearchCache =
            new Dictionary<string, IReadOnlyList<string>>();

        protected virtual string OptionVariableName => CommandConsts.OptionVariableName;
        protected virtual string BaseDirectoryVariableName => CommandConsts.BaseDirectoryVariableName;
        protected virtual string ProjectNameVariableName => CommandConsts.ProjectNameVariableName;
        protected virtual string ExcludeDirectoriesVariableName => CommandConsts.ExcludeDirectoriesVariableName;
        protected virtual string OverwriteVariableName => CommandConsts.OverwriteVariableName;

        public WorkflowExpression<string[]> ExcludeDirectories
        {
            get => GetState(() => new JavaScriptExpression<string[]>(ExcludeDirectoriesVariableName));
            set => SetState(value);
        }

        public WorkflowExpression<string> ProjectName
        {
            get => GetState(() => new JavaScriptExpression<string>(ProjectNameVariableName));
            set => SetState(value);
        }

        public WorkflowExpression<string> BaseDirectory
        {
            get => GetState(() => new JavaScriptExpression<string>(BaseDirectoryVariableName));
            set => SetState(value);
        }

        public WorkflowExpression<bool> CommandOption
        {
            get => GetState(() => new JavaScriptExpression<bool>(OverwriteVariableName));
            set => SetState(value);
        }

        protected virtual bool FileExistsInDirectory(string directory, string pattern,
            ICollection<string> excludedDirectories, bool includeSubModules = false)
        {
            return !SearchFileInDirectory(directory, pattern, excludedDirectories, includeSubModules).IsNullOrWhiteSpace();
        }

        protected virtual string? SearchFileInDirectory(string directory, string pattern,
            ICollection<string> excludedDirectories, bool includeSubModules = false)
        {
            var actualExcluded = ExcludeDirectorySearchCache.GetOrAdd(
                GetExcludedDirectorySearchCacheKey(directory, excludedDirectories),
                () => GetDirectoryFullPath(directory, excludedDirectories));

            var targetDirectories = new Queue<string>();
            targetDirectories.Enqueue(directory);

            return FindInDirectoryInQueue(targetDirectories, pattern, new HashSet<string>(actualExcluded),
                Directory.EnumerateFiles, includeSubModules);
        }

        protected virtual string? SearchDirectoryInDirectory(string directory, string pattern,
            ICollection<string> excludedDirectories, bool includeSubModules = false)
        {
            var actualExcluded = ExcludeDirectorySearchCache.GetOrAdd(
                GetExcludedDirectorySearchCacheKey(directory, excludedDirectories),
                () => GetDirectoryFullPath(directory, excludedDirectories));

            var targetDirectories = new Queue<string>();
            targetDirectories.Enqueue(directory);

            return FindInDirectoryInQueue(targetDirectories, pattern, new HashSet<string>(actualExcluded),
                Directory.EnumerateDirectories, includeSubModules);
        }

        protected virtual IEnumerable<string> SearchFilesInDirectory(string directory, string pattern,
            ICollection<string> excludedDirectories, bool includeSubModules = false)
        {
            var actualExcluded = ExcludeDirectorySearchCache.GetOrAdd(
                GetExcludedDirectorySearchCacheKey(directory, excludedDirectories),
                () => GetDirectoryFullPath(directory, excludedDirectories));

            var targetDirectories = new Queue<string>();
            targetDirectories.Enqueue(directory);

            return SearchInDirectoryRecursive(targetDirectories, pattern, new HashSet<string>(actualExcluded),
                Directory.EnumerateFiles, includeSubModules);
        }

        private static IEnumerable<string> SearchInDirectoryRecursive(Queue<string> queue, string pattern,
            IReadOnlySet<string> actualExcluded, Func<string, string, SearchOption, IEnumerable<string>> searchFunc,
            bool includeSubModules = false)
        {
            var matchedSln = false;
            while (queue.TryDequeue(out var directory))
            {
                if (!includeSubModules &&
                    null != searchFunc(directory, "*.sln", SearchOption.TopDirectoryOnly).FirstOrDefault())
                {
                    if (matchedSln)
                    {
                        continue;
                    }

                    matchedSln = true;
                }

                var result = searchFunc(directory, pattern, SearchOption.TopDirectoryOnly);

                foreach (var item in result)
                {
                    yield return item;
                }

                foreach (var d in Directory.EnumerateDirectories(directory))
                {
                    if (actualExcluded.Contains(d))
                    {
                        continue;
                    }

                    queue.Enqueue(d);
                }
            }
        }

        private static string? FindInDirectoryInQueue(Queue<string> queue, string pattern,
            IReadOnlySet<string> actualExcluded, Func<string, string, SearchOption, IEnumerable<string>> searchFunc,
            bool includeSubModules = false)
        {
            var matchedSln = false;
            while (queue.TryDequeue(out var directory))
            {
                if (!includeSubModules &&
                    null != searchFunc(directory, "*.sln", SearchOption.TopDirectoryOnly).FirstOrDefault())
                {
                    if (matchedSln)
                    {
                        continue;
                    }

                    matchedSln = true;
                }

                var result = searchFunc(directory, pattern, SearchOption.TopDirectoryOnly);
                if (result != null && result.Any())
                {
                    return result.FirstOrDefault();
                }

                foreach (var d in Directory.EnumerateDirectories(directory))
                {
                    if (actualExcluded.Contains(d))
                    {
                        continue;
                    }

                    queue.Enqueue(d);
                }
            }

            return null;
        }

        private string GetExcludedDirectorySearchCacheKey(string directory, IEnumerable<string> excludedDirectories)
        {
            return $"{directory}{string.Join("", excludedDirectories)}";
        }

        private IReadOnlyList<string> GetDirectoryFullPath(string directory, IEnumerable<string> patterns)
        {
            var list = new List<string>();
            foreach (var pattern in patterns)
            {
                var p = pattern;
                var all = false;
                if (pattern.StartsWith("**/") || pattern.StartsWith("**\\"))
                {
                    p = pattern.Substring(3);
                    all = true;
                }

                foreach (var d in Directory
                             .GetDirectories(directory, p,
                                 all ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    list.Add(d);
                }
            }

            return list;
        }
    }
}