﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;
using DotDocs.Core.Loader.Build;
using DotDocs.Core.Loader.Exceptions;
using DotDocs.Core.Loader.Services;
using DotDocs.Core.Models;
using DotDocs.Core.Models.Language;
using DotDocs.Core.Models.Mongo;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Logging.StructuredLogger;
using MongoDB.Driver;

namespace DotDocs.Core.Loader
{
    public class Repository : IDisposable
    {
        BuildInstance build;

        CommentService comments;

        public string Name { get; set; }
        /// <summary>
        /// The current commit hash of the repository.
        /// </summary>
        public string CommitHash { get; private set; }
        /// <summary>
        /// The url of the repository.
        /// </summary>
        public string Url { get; init; }
        /// <summary>
        /// The directory of the repository.
        /// </summary>
        public string Dir { get; private set; }

        // public ImmutableList<SolutionFile> Solutions { get; private set; }
        
        /// <summary>
        /// All project groups in the repository.
        /// </summary>
        public ImmutableArray<ProjectDocument> ProjectGraphs { get; private set; }
        /// <summary>
        /// The select root project of a group to be documented.
        /// </summary>
        public ProjectDocument ActiveProject { get; private set; }

        /// <summary>
        /// A collection containing all models required by the project.
        /// </summary>
        public ImmutableArray<TypeModel> AllModels { get; private set; }
        /// <summary>
        /// A collection containing only user defined models.
        /// </summary>
        public ImmutableArray<UserTypeModel> UserModels { get; private set; }
        public ImmutableArray<Models.AssemblyModel> UsedAssemblies { get; private set; }

        public Repository(string url, CommentService comments)
        {
            Url = url;
            this.comments = comments;
        }               

        /// <summary>
        /// Downloads a repository and returns the path to the repository.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Repository Download()
        {
            // CODE FOR DOWNLOADING AND BUILDNG
            string directory = AppContext.BaseDirectory; // directory of process execution
            string downloadRepoLocation = Path.Combine(directory, "downloads");
            if (!Directory.Exists(downloadRepoLocation))
                Directory.CreateDirectory(downloadRepoLocation);

            using PowerShell powershell = PowerShell.Create();
            // this changes from the user folder that PowerShell starts up with to your git repository
            powershell.AddScript($"cd {downloadRepoLocation}");
            powershell.AddScript(@"git clone https://github.com/Chase-William/.Docs.Core.git");
            //powershell.AddScript("cd.. / .. /.Docs.Core");
            //powershell.AddScript("dotnet build");
            powershell.Invoke(); // Run powershell            

            var folder = Url.Split("/").Last();
            if (folder.Contains(".git"))
                folder = folder[..4];

            Dir = Path.Combine(downloadRepoLocation, folder);
            return this;
        }

        /// <summary>
        /// Retrieves the current hash for the HEAD commit of the downloaded repository.
        /// </summary>
        /// <param name="repoDir">Base directory of the repo.</param>
        /// <returns>Commit HEAD Hash</returns>
        /// <exception cref="FileNotFoundException"></exception>
        public Repository RetrieveHashInfo()
        {
            string gitHeadFile = Path.Combine(Dir, @".git\HEAD");
            if (!File.Exists(gitHeadFile))
                throw new FileNotFoundException($"File 'HEAD' was not found at: {gitHeadFile}. Has the repository been downloaded using 'git clone <repo-url>' yet?");

            string commitHashFilePath = File.ReadAllText(gitHeadFile);
            // 'ref: ' <- skip these characters and get file dir that follows
            commitHashFilePath = Path.Combine(Dir, ".git", commitHashFilePath[5..]
                .Replace("\n", "")
                .Replace("/", "\\")
                .Trim());

            if (!File.Exists(commitHashFilePath))
                throw new FileNotFoundException($"The file containing the current HEAD file hash was not found at: {commitHashFilePath}");

            CommitHash = File.ReadAllText(commitHashFilePath)
                .Replace("\n", "")
                .Trim();
            return this;
        }

        //public Repository FindSolutions()
        //{
        //    var solutionFiles = Directory.GetFiles(repoDir, "*.sln", SearchOption.AllDirectories);
            
        //}

        /// <summary>
        /// Creates a dependency graph for each project group.
        /// </summary>
        /// <returns></returns>
        public Repository MakeProjectGraph()
        {
            // Locate all solution and project files            
            var projectFiles = Directory.GetFiles(Dir, "*.csproj", SearchOption.AllDirectories);
            ProjectGraphs = FindRootProjects(projectFiles.ToList())
                .ToImmutableArray();
            return this;
        }

        /// <summary>
        /// Ensures each project file has documentation generation enabled in the .csproj file.
        /// </summary>
        /// <returns></returns>
        public Repository EnableDocumentationGeneration()
        {
            ActiveProject.EnableDocumentationGeneration();
            return this;
        }

        /// <summary>
        /// Builds active project via the property <see cref="ActiveProject"/>.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="BuildException"></exception>
        public Repository Build()
        {
            build = new BuildInstance(ActiveProject)
                .Build()
                .MakeUserModels();                

            return this;
        }

        /// <summary>
        /// Sets the active project to build.
        /// </summary>
        /// <returns></returns>
        public Repository SetActiveProject()
        {
            if (ProjectGraphs.Length > 1)
            {
                while (true)
                {
                    Console.WriteLine("Multiple related project groups detected. Please choose one:");
                    for (int i = 0; i < ProjectGraphs.Length; i++)
                        Console.WriteLine($"{i + 1} - {ProjectGraphs[i].ProjectFilePath}");
                    Console.Write(": ");
                    // Valid input
                    if (int.TryParse(Console.ReadLine(), out int index))
                    {
                        index--;
                        // Valid index range
                        if (index < ProjectGraphs.Length && index > -1)
                        {
                            ActiveProject = ProjectGraphs[index];
                            break;
                        }
                    }
                }
            }
            ActiveProject = ProjectGraphs.First();
            return this;
        }

        public Repository Prepare()
        {
            /*
             * Load all supporting types for all user created models
             */
            AddSupportingTypes();

            /*
             * Aggregate all assemblies used for documentation later 
             */
            UsedAssemblies = AllModels
                .DistinctBy(m => m.Info.Assembly)
                .Select(m => new Models.AssemblyModel(m.Info.Assembly))
                .ToImmutableArray();

            /*
             * Load all documentation for models from the database
             */
            comments.UpdateDocumentation(this, build.AllProjectBuildInstances);

            return this;
        }

        public Repository Document()
        {
            // Render documentation
            return this;
        }       

        /// <summary>
        /// Load all supporting types for all user created models.
        /// </summary>
        void AddSupportingTypes()
        {
            UserModels = build.RootProjectBuildInstance
                .AggregateModels(new List<UserTypeModel>())
                .ToImmutableArray();
            var allModels = new Dictionary<string, TypeModel>(UserModels
                    .Select(model => new KeyValuePair<string, TypeModel>(model.Info.GetTypeId(), model)));
            // Add model dependencies to the collection of all types            
            foreach (var instance in build.AllProjectBuildInstances)
                foreach (var model in instance.Models)
                    model.Add(allModels);
        }

        /// <summary>
        /// Returns all .csproj files that are the root project of a possibly larger project structure.
        /// </summary>
        /// <returns></returns>
        static IEnumerable<ProjectDocument> FindRootProjects(List<string> projectFiles)
        {
            var projects = new List<ProjectDocument>();

            while (projectFiles.Count != 0)
            {
                var proj = projectFiles.First();

                if (!File.Exists(projectFiles.First()))
                    throw new FileNotFoundException($"The following project file path does not exist: {proj}");

                projects.Add(ProjectDocument.From(proj, projectFiles, projects));
            }
            return projects.Where(proj => proj.Parent == null).ToArray();
        }

        public void Dispose()
            => build?.Dispose();
        
    }
}