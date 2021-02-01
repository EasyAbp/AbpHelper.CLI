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
        private static readonly Dictionary<string, IReadOnlyList<string>> ExcludeDirectorySearchCache = new Dictionary<string, IReadOnlyList<string>>();

        protected virtual string OptionVariableName => CommandConsts.OptionVariableName;
        protected virtual string BaseDirectoryVariableName => CommandConsts.BaseDirectoryVariableName;
        protected virtual string ExcludeDirectoriesVariableName => CommandConsts.ExcludeDirectoriesVariableName;
        protected virtual string OverwriteVariableName => CommandConsts.OverwriteVariableName;

        public WorkflowExpression<string[]> ExcludeDirectories
        {
            get => GetState(() => new JavaScriptExpression<string[]>(ExcludeDirectoriesVariableName));
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

        protected virtual bool FileExistsInDirectory(string directory, string pattern, params string[] excludedDirectories)
        {
            return !SearchFileInDirectory(directory, pattern, excludedDirectories).IsNullOrWhiteSpace();
        }

        protected virtual string? SearchFileInDirectory(string directory, string pattern,
            params string[] excludedDirectories)
        {
            var actualExcluded = ExcludeDirectorySearchCache.GetOrAdd(
                GetExcludedDirectorySearchCacheKey(directory, excludedDirectories),
                () => GetDirectoryFullPath(directory, excludedDirectories));

            return FindInDirectoryRecursive(directory, pattern, new HashSet<string>(actualExcluded),
                Directory.EnumerateFiles);
        }

        protected virtual string? SearchDirectoryInDirectory(string directory, string pattern,
            params string[] excludedDirectories)
        {
            var actualExcluded = ExcludeDirectorySearchCache.GetOrAdd(
                GetExcludedDirectorySearchCacheKey(directory, excludedDirectories),
                () => GetDirectoryFullPath(directory, excludedDirectories));

            return FindInDirectoryRecursive(directory, pattern, new HashSet<string>(actualExcluded),
                Directory.EnumerateDirectories);
        }

        protected virtual IEnumerable<string> SearchFilesInDirectory(string directory, string pattern,
            params string[] excludedDirectories)
        {
            var actualExcluded = ExcludeDirectorySearchCache.GetOrAdd(
                GetExcludedDirectorySearchCacheKey(directory, excludedDirectories),
                () => GetDirectoryFullPath(directory, excludedDirectories));

            return SearchInDirectoryRecursive(directory, pattern, new HashSet<string>(excludedDirectories), Directory.EnumerateFiles);
        }

        private IEnumerable<string> SearchInDirectoryRecursive(string directory, string pattern, HashSet<string> actualExcluded, Func<string, string, SearchOption, IEnumerable<string>> searchFunc)
        {
            foreach (var result in searchFunc(directory, pattern, SearchOption.TopDirectoryOnly))
            {
                yield return result;
            }

            foreach (var d in Directory.EnumerateDirectories(directory))
            {
                if (actualExcluded.Contains(d))
                {
                    actualExcluded.Remove(d);
                    continue;
                }

                foreach (var result in SearchInDirectoryRecursive(d, pattern, actualExcluded, searchFunc))
                {
                    yield return result;
                }
            }
        }

        private string? FindInDirectoryRecursive(string directory, string pattern, HashSet<string> actualExcluded, Func<string, string, SearchOption, IEnumerable<string>> searchFunc)
        {
            var result = searchFunc(directory, pattern, SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (!result.IsNullOrWhiteSpace())
            {
                return result;
            }

            foreach (var d in Directory.EnumerateDirectories(directory))
            {
                if (actualExcluded.Contains(d))
                {
                    actualExcluded.Remove(d);
                    continue;
                }

                result = FindInDirectoryRecursive(d, pattern, actualExcluded, searchFunc);
                if (!result.IsNullOrWhiteSpace())
                {
                    return result;
                }
            }

            return null;
        }

        private string GetExcludedDirectorySearchCacheKey(string directory, string[] excludedDirectories)
        {
            return $"{directory}{string.Join("", excludedDirectories)}";
        }

        private IReadOnlyList<string> GetDirectoryFullPath(string directory, string[] patterns)
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