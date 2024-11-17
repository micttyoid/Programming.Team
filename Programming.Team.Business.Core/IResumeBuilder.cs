using Programming.Team.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Business.Core
{
    public interface IResumeBuilder
    {
        Task<Resume> BuildResume(Guid userId, CancellationToken token = default);
        Task<Posting> BuildPosting(Guid userId, Guid documentTemplateId, string name, string positionText, Resume resume, ResumeConfiguration? config = null, CancellationToken token = default);
    }
    public interface IResumeBlob
    {
        Task UploadResume(Guid postingId, byte[] pdfData, CancellationToken token = default);
        Task<byte[]?> GetResume(Guid postingId, CancellationToken token = default);
    }
}
