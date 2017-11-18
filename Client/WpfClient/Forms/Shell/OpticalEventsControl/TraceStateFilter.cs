using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class TraceStateFilter
    {
        public bool IsOn { get; set; }
        public FiberState TraceState { get; set; }

        /// <summary>
        /// ����� ������������� ��������� ����������� ������
        /// </summary>
        public TraceStateFilter() { IsOn = false; }

        /// <summary>
        /// � ����� ������ ���������� ������ "����" ��������
        /// </summary>
        /// <param name="traceState"></param>
        public TraceStateFilter(FiberState traceState)
        {
            IsOn = true;
            TraceState = traceState;
        }

        public override string ToString()
        {
            return IsOn ? TraceState.GetLocalizedString() : @"<no filter>";
        }
    }
}