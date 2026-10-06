using SimpleApi.Domain;

namespace SimpleApi.Infra;

public interface IFileRepository
{
    Task<FileDirectory> GetFileDirectoryAsync(string fileName);
}