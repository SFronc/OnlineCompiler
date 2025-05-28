
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Web.CodeGeneration;
using OnlineCompiler.Models;
using CompilationResult = OnlineCompiler.Models.CompilationResult;

namespace OnlineCompiler.Services
{
    public interface ICompilerService
    {
        Task<CompilationResult> CompileAsync(string code, string language, List<FileModel> projFiles);
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

        public async Task<CompilationResult> CompileAsync(string code, string language, List<FileModel> projFiles)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"compile_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            var inputFile = Path.Combine(tempDir, "input.cz");
            var poczetFile = Path.Combine(tempDir, "poczet.txt"); 

            try
            {
                await File.WriteAllTextAsync(inputFile, code, Encoding.ASCII);

                foreach (var item in projFiles)
                {
                    await File.WriteAllBytesAsync(Path.Combine(tempDir, item.Name + ".cz"), item.Content);
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
                    StandardErrorEncoding = Encoding.UTF8
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
                            output.AppendLine(args.Data);
                        else
                            outputCompletion.SetResult(true);
                    };

                    process.ErrorDataReceived += (sender, args) =>
                    {
                        if (args.Data != null)
                            errors.AppendLine(args.Data);
                        else
                            errorCompletion.SetResult(true);
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    await Task.Delay(100);
                    await process.StandardInput.WriteLineAsync();

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

                    return new CompilationResult
                    {
                        Success = process.ExitCode == 0,
                        Output = AnsiConverter.ToHtml(output.ToString()),
                        Errors = AnsiConverter.ToHtml(errors.ToString()),
                        Poczet = poczetContent 
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
                CleanupTempDirectory(tempDir);
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

        private void CleanupTempDirectory(string tempDir)
        {
            try
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, recursive: true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Could not delete temp directory: {tempDir}");
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
        string output = ansiText;

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

        return "<pre style=\"white-space:pre-wrap\">" + output + "</pre>";
    }
}


}