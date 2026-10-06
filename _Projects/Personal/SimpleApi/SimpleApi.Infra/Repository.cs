using SimpleApi.Domain;

namespace SimpleApi.Infra;

public class FileRepository : IFileRepository
{
    public async Task<FileDirectory> GetFileDirectoryAsync(string fileName)
    {
        // Simulate fetching file directory from a data source
        await Task.Delay(100); // Simulating async operation
        return new FileDirectory
        {
            FileName = fileName,
            FilePath = $"/path/to/{fileName}.pdf"
        };
    }
}