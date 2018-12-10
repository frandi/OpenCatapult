// Copyright (c) Polyrific, Inc 2018. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polyrific.Catapult.Plugins.Core.Configs;
using Polyrific.Catapult.Shared.Dto.Constants;
using Polyrific.Catapult.Shared.Dto.ProjectDataModel;

namespace Polyrific.Catapult.Plugins.Core
{
    public abstract class CodeGeneratorProvider : TaskProvider
    {
        protected CodeGeneratorProvider(string[] args) : base(args)
        {
            ParseArguments();
        }

        protected virtual int MaxSearchModelIdLine => 10;

        public override string Type => PluginType.GeneratorProvider;

        public sealed override void ParseArguments()
        {
            base.ParseArguments();
            
            foreach (var key in ParsedArguments.Keys)
            {
                switch (key.ToLower())
                {
                    case "project":
                        ProjectName = ParsedArguments[key].ToString();
                        break;
                    case "models":
                        Models = JsonConvert.DeserializeObject<List<ProjectDataModelDto>>(ParsedArguments[key].ToString());
                        break;
                    case "config":
                        Config = JsonConvert.DeserializeObject<GenerateTaskConfig>(ParsedArguments[key].ToString());
                        break;
                    case "additional":
                        AdditionalConfigs = JsonConvert.DeserializeObject<Dictionary<string, string>>(ParsedArguments[key].ToString());
                        break;
                }
            }
        }

        public override async Task<string> Execute()
        {
            var result = new Dictionary<string, object>();

            switch (ProcessToExecute)
            {
                case "pre":
                    var error = await BeforeGenerate();
                    if (!string.IsNullOrEmpty(error))
                        result.Add("error", error);
                    break;
                case "main":
                    (string outputLocation, Dictionary<string, string> outputValues, string errorMessage) = await Generate();
                    result.Add("outputLocation", outputLocation);
                    result.Add("outputValues", outputValues);
                    result.Add("errorMessage", errorMessage);
                    break;
                case "post":
                    error = await AfterGenerate();
                    if (!string.IsNullOrEmpty(error))
                        result.Add("error", error);
                    break;
                default:
                    await BeforeGenerate();
                    (outputLocation, outputValues, errorMessage) = await Generate();
                    await AfterGenerate();

                    result.Add("outputLocation", outputLocation);
                    result.Add("outputValues", outputValues);
                    result.Add("errorMessage", errorMessage);
                    break;
            }

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Name of the project
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// Project data models
        /// </summary>
        public List<ProjectDataModelDto> Models { get; set; }

        /// <summary>
        /// Generate task configuration
        /// </summary>
        public GenerateTaskConfig Config { get; set; }

        /// <summary>
        /// Additional configurations for specific provider
        /// </summary>
        public Dictionary<string, string> AdditionalConfigs { get; set; }

        /// <summary>
        /// Process to run before executing the code generation
        /// </summary>
        /// <returns></returns>
        public virtual Task<string> BeforeGenerate()
        {
            return Task.FromResult("");
        }

        /// <summary>
        /// Generate code from data models
        /// </summary>
        /// <returns></returns>
        public abstract Task<(string outputLocation, Dictionary<string, string> outputValues, string errorMessage)> Generate();

        /// <summary>
        /// Process to run after executing code generation
        /// </summary>
        /// <returns></returns>
        public virtual Task<string> AfterGenerate()
        {
            return Task.FromResult("");
        }
        
        /// <summary>
        /// Add a model tag in the beginning of the file content, so it can be cleaned later when the model has been deleted from the project
        /// </summary>
        /// <param name="fileContent">The content of the file</param>
        /// <param name="fileType">The file/languange type. <see cref="GeneratedFileType"/></param>
        /// <param name="modelId">The Id of the model</param>
        /// <returns></returns>
        public virtual string AddModelTag(string fileContent, string fileType, string modelId)
        {
            var sb = new StringBuilder();
            switch (fileType)
            {
                case GeneratedFileType.CSharp:
                    sb.AppendLine(string.Format(GeneratedFileCommentTag.CsharpModelTag, modelId));
                    break;
                case GeneratedFileType.CsHtml:
                    sb.AppendLine(string.Format(GeneratedFileCommentTag.CshtmlModelTag, modelId));
                    break;
                case GeneratedFileType.Javascript:
                    sb.AppendLine(string.Format(GeneratedFileCommentTag.JavascriptModelTag, modelId));
                    break;
            }

            sb.Append(fileContent);
            return sb.ToString();
        }

        /// <summary>
        /// Get the file model id tag value from a file content
        /// </summary>
        /// <param name="fileContent">The content of the file</param>
        /// <param name="fileType">The file/languange type. <see cref="GeneratedFileType"/></param>
        /// <returns></returns>
        public virtual string GetFileModelId(string fileContent, string fileType)
        {
            int modelId;
            int counter = 0;
            string line;
            var strReader = new StringReader(fileContent);
            while (counter <= MaxSearchModelIdLine && (line = strReader.ReadLine()) != null)
            {
                switch (fileType)
                {
                    case GeneratedFileType.CSharp:
                        if (line.StartsWith(GeneratedFileCommentTag.CsharpModelTag.Replace("{0}", "")))
                            if (int.TryParse(line.Split(':').LastOrDefault()?.Trim(), out modelId))
                                return modelId.ToString();
                        break;
                    case GeneratedFileType.CsHtml:
                        if (line.StartsWith(GeneratedFileCommentTag.CshtmlModelTag.Replace("{0}-->", "")))
                            if (int.TryParse(line.Replace("-->", "").Split(':').LastOrDefault()?.Trim(), out modelId))
                                return modelId.ToString();
                        break;
                    case GeneratedFileType.Javascript:
                        if (line.StartsWith(GeneratedFileCommentTag.JavascriptModelTag.Replace("{0}", "")))
                            if (int.TryParse(line.Split(':').LastOrDefault()?.Trim(), out modelId))
                                return modelId.ToString();
                        break;
                }

                counter++;
            }

            return null;
        }

        /// <summary>
        /// Add the auto-generated content tag to a content
        /// </summary>
        /// <param name="generatedContent">The generated content</param>
        /// <param name="fileType">The file/languange type. <see cref="GeneratedFileType"/></param>
        /// <param name="tagName">The tag name to identify the generated content section</param>
        /// <returns></returns>
        public virtual string AddAutoGeneratedContentTag(string generatedContent, string fileType, string tagName)
        {
            var sb = new StringBuilder();
            switch (fileType)
            {
                case GeneratedFileType.CSharp:
                    sb.AppendLine(string.Format(GeneratedFileCommentTag.CsharpBeginComment, tagName));
                    sb.Append(generatedContent);
                    sb.AppendLine(string.Format(GeneratedFileCommentTag.CsharpEndComment, tagName));
                    break;
                case GeneratedFileType.CsHtml:
                    sb.AppendLine(string.Format(GeneratedFileCommentTag.CshtmlBeginComment, tagName));
                    sb.Append(generatedContent);
                    sb.AppendLine(string.Format(GeneratedFileCommentTag.CshtmlEndComment, tagName));
                    break;
                case GeneratedFileType.Javascript:
                    sb.AppendLine(string.Format(GeneratedFileCommentTag.JavascriptBeginComment, tagName));
                    sb.Append(generatedContent);
                    sb.AppendLine(string.Format(GeneratedFileCommentTag.JavascriptEndComment, tagName));
                    break;
                default:
                    sb.Append(generatedContent);
                    break;
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// Update the generated content of a generated tag name
        /// </summary>
        /// <param name="fileContent">The content of the file</param>
        /// <param name="updatedFileContent">The updated content of the file</param>
        /// <param name="fileType">The file/languange type. <see cref="GeneratedFileType"/></param>
        /// <param name="tagName">The tag name to identify the generated content section</param>
        /// <returns></returns>
        public virtual string UpdateAutoGeneratedContent(string fileContent, string updatedFileContent, string fileType, string tagName)
        {
            string line;
            var strReader = new StringReader(fileContent);
            var sb = new StringBuilder();
            string currentBeginTagName = null;
            string currentEndTagName = null;
            bool currentTag = false;
            bool contentUpdated = false;
            while ((line = strReader.ReadLine()) != null)
            {
                if (!currentTag || contentUpdated)
                    sb.AppendLine(line);
                
                if (!contentUpdated)
                {
                    switch (fileType)
                    {
                        case GeneratedFileType.CSharp:
                            if (!currentTag && line.StartsWith(GeneratedFileCommentTag.CsharpBeginComment.Replace("{0}", "")))
                            {
                                currentBeginTagName = line.Split(':').LastOrDefault()?.Trim();
                                if (currentBeginTagName == tagName)
                                    currentTag = true;
                            }
                            else if (currentTag && line.StartsWith(GeneratedFileCommentTag.CsharpEndComment.Replace("{0}", "")))
                            {
                                currentEndTagName = line.Split(':').LastOrDefault()?.Trim();
                                if (currentEndTagName == tagName)
                                {
                                    sb.Append(updatedFileContent);
                                    sb.AppendLine(line);
                                    contentUpdated = true;
                                }
                            }

                            break;
                        case GeneratedFileType.CsHtml:
                            if (!currentTag && line.StartsWith(GeneratedFileCommentTag.CshtmlBeginComment.Replace("{0}-->", "")))
                            {
                                currentBeginTagName = line.Replace("-->", "").Split(':').LastOrDefault()?.Trim();
                                if (currentBeginTagName == tagName)
                                    currentTag = true;
                            }
                            else if (currentTag && line.StartsWith(GeneratedFileCommentTag.CshtmlEndComment.Replace("{0}-->", "")))
                            {
                                currentEndTagName = line.Replace("-->", "").Split(':').LastOrDefault()?.Trim();
                                if (currentEndTagName == tagName)
                                {
                                    sb.Append(updatedFileContent);
                                    sb.AppendLine(line);
                                    contentUpdated = true;
                                }
                            }

                            break;
                        case GeneratedFileType.Javascript:
                            if (!currentTag && line.StartsWith(GeneratedFileCommentTag.JavascriptBeginComment.Replace("{0}", "")))
                            {
                                currentBeginTagName = line.Split(':').LastOrDefault()?.Trim();
                                if (currentBeginTagName == tagName)
                                    currentTag = true;
                            }
                            else if (currentTag && line.StartsWith(GeneratedFileCommentTag.JavascriptEndComment.Replace("{0}", "")))
                            {
                                currentEndTagName = line.Split(':').LastOrDefault()?.Trim();
                                if (currentEndTagName == tagName)
                                {
                                    sb.Append(updatedFileContent);
                                    sb.AppendLine(line);
                                    contentUpdated = true;
                                }
                            }
                            break;
                    }
                }                
            }

            return sb.ToString();
        }
    }
}
