using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Programming.Team.Web.REST.Controllers
{
    public class LatexHolder
    {
        public string Latex { get; set; } = null!;
    }
    [ApiController]
    [Route("[controller]")]
    public class LatexController : ControllerBase
    {
        [HttpPost]
        [Route("compile")]
        public IActionResult CompileLatex([FromBody] LatexHolder request)
        {
            if (string.IsNullOrWhiteSpace(request.Latex))
            {
                return BadRequest("LaTeX content cannot be empty.");
            }

            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            string texFilePath = Path.Combine(tempDir, "document.tex");
            string pdfFilePath = Path.Combine(tempDir, "document.pdf");

            try
            {
                // Save LaTeX content to a .tex file
                System.IO.File.WriteAllText(texFilePath, request.Latex);

                // Compile the .tex file into a PDF using pdflatex
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "pdflatex",
                        Arguments = $"-interaction=nonstopmode -output-directory \"{tempDir}\" \"{texFilePath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                process.Start();
                process.WaitForExit();

                // Check if the PDF was generated
                if (System.IO.File.Exists(pdfFilePath))
                {
                    byte[] pdfBytes = System.IO.File.ReadAllBytes(pdfFilePath);
                    return File(pdfBytes, "application/pdf", "document.pdf");
                }

                return StatusCode(500, "Failed to compile LaTeX to PDF.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
            finally
            {
                // Clean up temporary files
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
    }
}
