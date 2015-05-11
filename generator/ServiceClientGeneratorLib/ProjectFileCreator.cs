﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using ServiceClientGenerator.Generators.ProjectFiles;

namespace ServiceClientGenerator
{
    /// <summary>
    /// Class used to emit the set of per-platform project files for a service. 
    /// Existing project files are retained, only missing files are generated.
    /// </summary>
    public class ProjectFileCreator
    {
        /// <summary>
        /// On conclusion, the set of project files that were generated for the service.
        /// </summary>
        public Dictionary<string, ProjectConfigurationData> CreatedProjectFiles { get; set; }

        /// <summary>
        /// Creates the platform-specific project files for the given service configuration
        /// </summary>
        /// <param name="serviceFilesRoot">The folder under which all of the source files for the service will exist</param>
        /// <param name="serviceConfiguration"></param>
        /// <param name="projectFileConfigurations"></param>
        public void Execute(string serviceFilesRoot, ServiceConfiguration serviceConfiguration, IEnumerable<ProjectFileConfiguration> projectFileConfigurations)
        {
            CreatedProjectFiles = new Dictionary<string, ProjectConfigurationData>();

            foreach (var projectFileConfiguration in projectFileConfigurations)
            {
                var projectType = projectFileConfiguration.Name;

                var assemblyName = "AWSSDK." + serviceConfiguration.Namespace.Split('.')[1];
                var projectFilename = string.Concat(assemblyName, ".", projectType, ".csproj");
                bool newProject = false;
                string projectGuid;
                if (File.Exists(Path.Combine(serviceFilesRoot, projectFilename)))
                {
                    Console.WriteLine("...updating existing project file {0}", projectFilename);
                    var projectPath = Path.Combine(serviceFilesRoot, projectFilename);
                    projectGuid = GetProjectGuid(projectPath);
                }
                else
                {
                    newProject = true;
                    projectGuid = NewProjectGuid;
                    Console.WriteLine("...creating project file {0}", projectFilename);
                }


                var templateSession = new Dictionary<string, object>();

                templateSession["ProjectGuid"] = projectGuid;
                templateSession["RootNamespace"] = serviceConfiguration.Namespace;
                templateSession["AssemblyName"] = assemblyName;
                templateSession["SourceDirectories"] = GetProjectSourceFolders(projectFileConfiguration, serviceFilesRoot);
                templateSession["NugetPackagesLocation"] = @"..\..\..\packages\";
                templateSession["TargetFrameworkVersion"] = projectFileConfiguration.TargetFrameworkVersion;
                templateSession["DefineConstants"] = projectFileConfiguration.CompilationConstants;
                templateSession["BinSubfolder"] = projectFileConfiguration.BinSubFolder;

                var projectConfigurationData = new ProjectConfigurationData { ProjectGuid = projectGuid };
                var projectName = Path.GetFileNameWithoutExtension(projectFilename);

                if(newProject)
                    CreatedProjectFiles[projectName] = projectConfigurationData;

                var coreRuntimeProject = string.Concat(@"..\..\Core\AWSSDK.Core.", projectType, ".csproj");
                var projectReferences = new List<ProjectReference>();


                if (serviceConfiguration.ServiceDependencies != null)
                {
                    foreach (var dependency in serviceConfiguration.ServiceDependencies)
                    {
                        var dependencyProjectName = "AWSSDK." + dependency.Key + "." + projectType;
                        string dependencyProject;
                        if (string.Equals(dependency.Key, "Core", StringComparison.InvariantCultureIgnoreCase))
                        {
                            dependencyProject = string.Concat(@"..\..\", dependency.Key, "\\", dependencyProjectName, ".csproj");
                        }
                        else
                        {
                            dependencyProject = string.Concat(@"..\", dependency.Key, "\\", dependencyProjectName, ".csproj");
                        }

                        projectReferences.Add(new ProjectReference
                        {
                            IncludePath = dependencyProject,
                            ProjectGuid = GetProjectGuid(Path.Combine(serviceFilesRoot, dependencyProject)),
                            Name = dependencyProjectName
                        });
                    }
                }


                templateSession["ProjectReferences"] = projectReferences.OrderBy(x => x.Name).ToList();

                if (serviceConfiguration.ModelName.Equals("s3", StringComparison.OrdinalIgnoreCase) && projectType == "Net45")
                {
                    templateSession["SystemReferences"] = new List<string> { "System.Net.Http" };
                }

                GenerateProjectFile(projectFileConfiguration, projectConfigurationData, templateSession, serviceFilesRoot, projectFilename);
            }
        }

        /// <summary>
        /// Invokes the T4 generator to emit a platform-specific project file.
        /// </summary>
        /// <param name="projectFileConfiguration"></param>
        /// <param name="session"></param>
        /// <param name="serviceFilesRoot"></param>
        /// <param name="projectFilename"></param>
        private void GenerateProjectFile(ProjectFileConfiguration projectFileConfiguration, 
                                         ProjectConfigurationData projectConfiguration,
                                         IDictionary<string, object> session, 
                                         string serviceFilesRoot, 
                                         string projectFilename)
        {
            var projectName = Path.GetFileNameWithoutExtension(projectFilename);

            // have not found a reasonable way to be able to activate from a string typename and
            // cast back to actual generator type instance :-(. Was hoping to make this completely
            // generic.
            string generatedContent;
            switch (projectFileConfiguration.Template)
            {
                case "BclProjectFile":
                    {
                        var generator = new BclProjectFile { Session = session };
                        generatedContent = generator.TransformText();
                    }
                    break;
                case "PhoneProjectFile":
                    {
                        var generator = new PhoneProjectFile { Session = session };
                        generatedContent = generator.TransformText();
                    }
                    break;
                case "RtProjectFile":
                    {
                        var generator = new RtProjectFile { Session = session };
                        generatedContent = generator.TransformText();
                    }
                    break;
                case "PortableProjectFile":
                    {
                        var generator = new PortableProjectFile { Session = session };
                        generatedContent = generator.TransformText();
                    }
                    break;
                default:
                    throw new ArgumentException("Project template name " + projectFileConfiguration.Template + " is not recognized");
            }


            GeneratorDriver.WriteFile(serviceFilesRoot, string.Empty, projectFilename, generatedContent);
            projectConfiguration.ConfigurationPlatforms = projectFileConfiguration.Configurations;
        }

        /// <summary>
        /// Returns the collection of subfolders containing source code that need to be 
        /// included in the project file. This is comprised the standard platform folders
        /// under Generated, plus any custom folders we find that are not otherwise handled
        /// (eg Properties).
        /// </summary>
        /// <param name="projectFileConfiguration">
        /// The .Net project type we are generating. This governs the platform-specific
        /// subfolders that get included in the project.
        /// </param>
        /// <param name="serviceRootFolder">The root output folder for the service code</param>
        /// <returns></returns>
        private IList<string> GetProjectSourceFolders(ProjectFileConfiguration projectFileConfiguration, string serviceRootFolder)
        {
            // Start with the standard generated code folders for the platform
            var sourceCodeFolders = new List<string>
            {
                "Generated", 
                @"Generated\Model", 
                @"Generated\Model\Internal", 
                @"Generated\Model\Internal\MarshallTransformations"
            };

            var platformSubFolders = projectFileConfiguration.PlatformCodeFolders;
            sourceCodeFolders.AddRange(platformSubFolders.Select(folder => Path.Combine(@"Generated", folder)));

            // Augment the returned folders with any custom subfolders already in existence. If the custom folder 
            // ends with a recognised platform, only add it to the set if it matches the platform being generated
            if (Directory.Exists(serviceRootFolder))
            {
                var subFolders = Directory.GetDirectories(serviceRootFolder, "*", SearchOption.AllDirectories);
                if (subFolders.Any())
                {
                    foreach (var folder in subFolders)
                    {
                        var serviceRelativeFolder = folder.Substring(serviceRootFolder.Length);

                        if (!serviceRelativeFolder.StartsWith(@"\Custom", StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (projectFileConfiguration.IsPlatformCodeFolder(serviceRelativeFolder))
                        {
                            if (projectFileConfiguration.IsValidPlatformCodeFolderForProject(serviceRelativeFolder))
                                sourceCodeFolders.Add(serviceRelativeFolder.TrimStart('\\'));
                        }
                        else
                            sourceCodeFolders.Add(serviceRelativeFolder.TrimStart('\\'));
                    }
                }
            }

            var foldersThatExist = new List<string>();
            foreach (var folder in sourceCodeFolders)
            {
                if (Directory.Exists(Path.Combine(serviceRootFolder, folder)))
                    foldersThatExist.Add(folder);
            }

            // sort so we get a predictable layout
            foldersThatExist.Sort(StringComparer.OrdinalIgnoreCase);
            return foldersThatExist;
        }

        //private static string ProjectGuidFromFile(string projectFile)
        //{
        //    var content = File.ReadAllText(projectFile);
        //    var pos = content.IndexOf("<ProjectGuid>", StringComparison.OrdinalIgnoreCase) + "<ProjectGuid>".Length;
        //    var lastPos = content.IndexOf("</ProjectGuid>", pos, StringComparison.OrdinalIgnoreCase);

        //    return content.Substring(pos, lastPos - pos);
        //}

        /// <summary>
        /// Recovers the guid of a project from an existing project file.
        /// </summary>
        /// <param name="projectFile"></param>
        /// <returns></returns>
        private static string GetProjectGuid(string projectPath)
        {
            var xdoc = new XmlDocument();
            xdoc.Load(projectPath);
            var propertyGroups = xdoc.GetElementsByTagName("PropertyGroup");
            var element = ((XmlElement)propertyGroups[0]).GetElementsByTagName("ProjectGuid")[0];
            if (element == null)
                throw new ApplicationException("Failed to find project guid for existing project: " + projectPath);

            var projectGuid = element.InnerText;
            return projectGuid;
        }


        public static string NewProjectGuid
        {
            get
            {
                return Guid.NewGuid().ToString("B").ToUpper();
            }    
        }

        public class ProjectReference
        {
            public string IncludePath { get; set; }
            public string ProjectGuid { get; set; }
            public string Name { get; set; }
        }

        public class ProjectConfigurationData
        {
            public string ProjectGuid { get; set; }
            public IEnumerable<string> ConfigurationPlatforms { get; set; }
        }

    }
}