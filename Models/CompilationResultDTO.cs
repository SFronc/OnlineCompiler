namespace OnlineCompiler.Models
{
    public class CompilationResultDTO
    {
        public int Id { get; set; }
        public bool Success { get; set; }    
        public string Output { get; set; }     
        public string Errors { get; set; }    
        public DateTime CompilatedAt { get; set; } 

        public static CompilationResultDTO CompilationResultToDTO(CompilationResult result) => new()
        {
            Id = result.Id,
            Success = result.Success,
            Output = result.Output,
            Errors = result.Errors,
            CompilatedAt = result.CompilatedAt
        };
    }

}