using SimpleApi.Domain;
using SimpleApi.Infra;

namespace SimpleApi.Application;

public class DownloadFileHandler(IFileRepository _fileRepository)
{
    private string _hostUrl = "https://downloadfileserver.com/files/";
    public async Task<DownloadFileResponse> HandleAsync(DownloadFileRequest request)
    {
        var fileDirectory = await _fileRepository.GetFileDirectoryAsync(request.FileName);
        
        // Logic to validate the file exists
        if (fileDirectory == null)
        {
            throw new FileNotFoundException("File not found");
        }
        
        return new DownloadFileResponse
        {
            FileName = fileDirectory.FileName,
            FileUrl = $"{_hostUrl}{fileDirectory.FilePath}"
        };
    }
}