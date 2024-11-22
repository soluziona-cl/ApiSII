using System.Threading.Tasks;
using CajValp.Models;

namespace CajValp.Interfaces
{
    public interface IFileRepository
    {
        Task SaveFileRecordAsync(FileModel fileModel);
    }
}
