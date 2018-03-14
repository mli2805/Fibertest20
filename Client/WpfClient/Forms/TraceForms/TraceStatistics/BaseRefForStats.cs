using System;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class BaseRefModel
    {
        public BaseRefType BaseRefType { get; set; }
        public string BaseRefTypeString => BaseRefType.GetLocalizedString();
        public DateTime AssignedAt { get; set; }
        public string AssignedBy { get; set; }
        public Guid BaseRefId { get; set; }

        public int SorFileId { get; set; }

    }

    public class BaseRefModelFactory
    {
        public BaseRefModel Create(BaseRef baseRef)
        {
            var result = new BaseRefModel()
            {
                BaseRefType = baseRef.BaseRefType,
                AssignedAt = baseRef.SaveTimestamp,
                AssignedBy = baseRef.UserName,
                BaseRefId = baseRef.Id,

                SorFileId = baseRef.SorFileId,
            };
            return result;
        }
    }
}