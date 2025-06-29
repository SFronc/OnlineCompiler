
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGeneration;
using OnlineCompiler.Data;
using OnlineCompiler.Models;
using CompilationResult = OnlineCompiler.Models.CompilationResult;

namespace OnlineCompiler.Services
{
    public interface ICompilerService
    {
        Task<CompilationResult> CompileAsync(string code, string language, List<FileModel> projFiles, List<FileModel> libFiles, DataBaseContext _context);
    }

    public class CompilerService : ICompilerService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CompilerService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _compilerPath;
        private readonly string _includePath;
        private readonly string _libPath;

        public CompilerService(IWebHostEnvironment env, ILogger<CompilerService> logger, IConfiguration config)
        {
            _env = env;
            _logger = logger;
            _configuration = config;

            _compilerPath = GetFullPath("CompilerPaths:Compiler");
            _includePath = GetFullPath("CompilerPaths:IncludePath");
            _libPath = GetFullPath("CompilerPaths:LibPath");

            ValidatePaths();
        }

        private void ValidatePaths()
        {
            if (!File.Exists(_compilerPath))
                throw new FileNotFoundException($"GCC compiler not found at: {_compilerPath}");

            if (!Directory.Exists(_includePath))
                throw new DirectoryNotFoundException($"Include directory not found at: {_includePath}");

            if (!Directory.Exists(_libPath))
                throw new DirectoryNotFoundException($"Lib directory not found at: {_libPath}");
        }

        public async Task<CompilationResult> CompileAsync(string code, string language, List<FileModel> projFiles, List<FileModel> libFiles, DataBaseContext _context)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"compile_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            var projFileMappings = new Dictionary<string, FileModel>();

            var inputFile = Path.Combine(tempDir, "input.cz");
            var poczetFile = Path.Combine(tempDir, "poczet.txt");
            var errorFile = Path.Combine(tempDir, "blad.txt");

            try
            {
                await File.WriteAllTextAsync(inputFile, code, Encoding.ASCII);

                foreach (var item in projFiles)
                {
                    var ext = item.Type;
                    if (ext.IsNullOrEmpty())
                    {
                        ext = "cz";
                    }
                    //await File.WriteAllBytesAsync(Path.Combine(tempDir, item.Name + "." + ext), item.Content);
                    var filePath = Path.Combine(tempDir, $"{item.Name}.{ext}");
                    await File.WriteAllBytesAsync(filePath, item.Content);
                    projFileMappings[filePath] = item;
                }

                foreach (var item in libFiles)
                {
                    var ext = item.Type;
                    if (ext.IsNullOrEmpty())
                    {
                        ext = "cz";
                    }
                    Console.WriteLine($"==============:{item.Name}:==================");
                    await File.WriteAllBytesAsync(Path.Combine(tempDir, item.Name + "." + ext), item.Content);
                }

                var processInfo = new ProcessStartInfo
                {
                    FileName = _compilerPath,
                    Arguments = $"\"{inputFile}\"",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = tempDir,
                    StandardOutputEncoding = Encoding.ASCII,
                    StandardErrorEncoding = Encoding.ASCII
                };

                var output = new StringBuilder();
                var errors = new StringBuilder();

                using (var process = new Process { StartInfo = processInfo })
                {
                    var outputCompletion = new TaskCompletionSource<bool>();
                    var errorCompletion = new TaskCompletionSource<bool>();

                    process.OutputDataReceived += (sender, args) =>
                    {
                        if (args.Data != null)
                        {
                            if (args.Data != "")
                            {
                                output.AppendLine(args.Data);
                            }
                        }
                        else
                            outputCompletion.SetResult(true);
                    };

                    process.ErrorDataReceived += (sender, args) =>
                    {
                        if (args.Data != null)
                        {
                            if (args.Data != "")
                            {
                                errors.AppendLine(args.Data);
                            }
                        }
                        else
                            errorCompletion.SetResult(true);
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    await Task.Delay(100);
                    //await process.StandardInput.WriteLineAsync();
                    await process.StandardInput.WriteAsync("\x1A");

                    bool exited = await Task.Run(() => process.WaitForExit(10000));

                    if (!exited)
                    {
                        try { process.Kill(); } catch { }
                        return new CompilationResult
                        {
                            Success = false,
                            Output = AnsiConverter.ToHtml(output.ToString()),
                            Errors = "Process timeout exceeded. Forcefully terminated."
                        };
                    }

                    await Task.WhenAll(
                        Task.WhenAll(outputCompletion.Task, errorCompletion.Task),
                        Task.Delay(2000)
                    );

                    string poczetContent = string.Empty;
                    string errorContent = string.Empty;

                    if (File.Exists(poczetFile))
                    {
                        try
                        {
                            poczetContent = await File.ReadAllTextAsync(poczetFile, Encoding.UTF8);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to read poczet.txt file");
                        }
                    }

                    if (File.Exists(errorFile))
                    {
                        try
                        {
                            errorContent = await File.ReadAllTextAsync(errorFile, Encoding.UTF8);
                        }
                        catch { }
                    }

                    //Console.WriteLine("=============");
                    //Console.Write(output);
                    //Console.WriteLine("==============");

                    Console.WriteLine("==========Program Exit code==========");
                    Console.WriteLine(process.ExitCode);
                    Console.WriteLine("==============");

                    return new CompilationResult
                    {
                        Success = process.ExitCode == 0,
                        Output = AnsiConverter.ToHtml(output.ToString()),
                        Errors = AnsiConverter.ToHtml(errors.ToString()),
                        Poczet = poczetContent,
                        ErrorFile = errorContent
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Compilation failed");
                return new CompilationResult
                {
                    Success = false,
                    Output = $"Internal compiler error: {ex.Message}"
                };
            }
            finally
            {
                CleanupTempDirectoryAsync(tempDir, projFileMappings, _context);
            }
        }



        private string GetFullPath(string configKey)
        {
            var relativePath = _configuration[configKey];
            if (string.IsNullOrEmpty(relativePath))
            {
                throw new ConfigurationException($"Configuration key '{configKey}' is missing");
            }

            return Path.Combine(_env.ContentRootPath, relativePath);
        }

        private async Task CleanupTempDirectoryAsync(string tempDir, Dictionary<string, FileModel> projFileMappings, DataBaseContext _context)
        {
            try
            {
                if (Directory.Exists(tempDir))
                {
                    UpdateFileModelsContentAsync(projFileMappings);

                    foreach (var fileModel in projFileMappings.Values)
                    {
                        await _context.SaveChangesAsync();
                    }
        

                    //Console.WriteLine(tempDir);
                    Directory.Delete(tempDir, recursive: true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Could not delete temp directory: {tempDir}");
            }
        }

        private void UpdateFileModelsContentAsync(Dictionary<string, FileModel> fileMappings)
        {
            foreach (var mapping in fileMappings)
            {
                try
                {
                    if (File.Exists(mapping.Key))
                    {
                        var newContent = File.ReadAllBytes(mapping.Key);
                        mapping.Value.Content = newContent;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to update content for file {mapping.Key}");
                }
            }
        }
    }

    

    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message) : base(message) { }
    }

    public static class AnsiConverter
{
        public static string ToHtml(string ansiText)
        {
            if (string.IsNullOrEmpty(ansiText))
                return string.Empty;
            
             ansiText = ansiText.TrimEnd('\n', '\r');

             string output = ansiText
                  .Replace("\r\n", "\n") 
            //      .Replace("\r", "\n")
                  .TrimEnd('\n');
            //string output = ansiText;
    

            output = Regex.Replace(output, @"\x1B\[0?m", "</span>");

            output = Regex.Replace(output, @"\x1B\[(\d+;)?(\d+)m", match =>
            {
                var codes = match.Value
                    .Replace("\x1B[", "")
                    .Replace("m", "")
                    .Split(';');

                string style = "";

                foreach (var code in codes)
                {
                    switch (code)
                    {
                        case "1":
                            style += "font-weight:bold;";
                            break;
                        case "30": style += "color:black;"; break;
                        case "31": style += "color:red;"; break;
                        case "32": style += "color:green;"; break;
                        case "33": style += "color:yellow;"; break;
                        case "34": style += "color:blue;"; break;
                        case "35": style += "color:magenta;"; break;
                        case "36": style += "color:cyan;"; break;
                        case "37": style += "color:white;"; break;
                        case "90": style += "color:gray;"; break;
                    }
                }

                return $"<span style=\"{style}\">";
            });
            output = output.Replace("\n", "<br>");
            return "<pre>" + output + "</pre>";
        //return "<pre style=\"white-space:pre-wrap\">" + output + "</pre>";
    }
}


}