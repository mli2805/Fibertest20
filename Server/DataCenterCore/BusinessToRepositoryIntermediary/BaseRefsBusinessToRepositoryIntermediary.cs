using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public class BaseRefsBusinessToRepositoryIntermediary
    {
        private readonly BaseRefsRepository _baseRefsRepository;

        public BaseRefsBusinessToRepositoryIntermediary(BaseRefsRepository baseRefsRepository)
        {
            _baseRefsRepository = baseRefsRepository;
        }

        public async Task<AssignBaseRefsDto> ConvertReSendToAssign(ReSendBaseRefsDto dto)
        {
            var result = new AssignBaseRefsDto()
            {
                TraceId = dto.TraceId,
                RtuId = dto.RtuId,
                ClientId = dto.ClientId,
                OtauPortDto = dto.OtauPortDto,
                BaseRefs = await _baseRefsRepository.GetTraceBaseRefsAsync(dto.TraceId)
            };
            return result;
        }

        // !!! Direct call from WcfService to Repository failed
        public async Task<List<BaseRefDto>> GetTraceBaseRefsAsync(Guid traceId)
        {
            return await _baseRefsRepository.GetTraceBaseRefsAsync(traceId);
        }

        public async Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto)
        {
            List<BaseRef> baseRefs = new List<BaseRef>();
            foreach (var baseRefDto in dto.BaseRefs)
            {
                baseRefs.Add(Create(dto, baseRefDto));
            }
            return await _baseRefsRepository.AddUpdateOrRemoveBaseRef(baseRefs);
        }

        private BaseRef Create(AssignBaseRefsDto dto, BaseRefDto baseRef)
        {
            var newBaseRef = new BaseRef()
            {
                BaseRefId = baseRef.Id,
                TraceId = dto.TraceId,
                UserName = baseRef.UserName,
                BaseRefType = baseRef.BaseRefType,
                SaveTimestamp = baseRef.SaveTimestamp,
                SorBytes = baseRef.SorBytes,
            };
            return newBaseRef;
        }



    }
}