using DataFlowAPI.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFlowAPI.ResourceAccess.Compute
{
    public interface IComputeManagment
    {
        IDiskInfo GetDiskStatus(IDiskRequest diskInfo);
        void CreateAttachedDisk(IJobEntity job);
        void CreateAttachedDisk(IJobEntity job, TextWriter logger);
        void DetachDisk(IJobEntity job);
        void DetachDisk(IJobEntity job, TextWriter logger);
        void DeleteDetachedDisk(IJobEntity job);
        void DeleteDetachedDisk(IJobEntity job, TextWriter logger);
        void AttachDisk(IJobEntity job);
        void AttachDisk(IJobEntity job, TextWriter logger);
        void CopyFromBlobToAttachedDisk(IJobEntity job);
        void CopyFromBlobToAttachedDisk(IJobEntity job, TextWriter logger);
        List<string> ResponseLog { get; }
        void GetAttachedDisks(IJobEntity job);
        void GetAttachedDisks(IJobEntity job, TextWriter logger);
    }
}

