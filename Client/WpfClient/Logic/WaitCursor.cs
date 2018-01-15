using System;
using System.Windows.Input;

namespace Iit.Fibertest.Client
{
    public interface ICursorBlah : IDisposable
    {
        
    }

    public class WaitCursor : ICursorBlah
    {
        private Cursor _previousCursor;

        public WaitCursor()
        {
            _previousCursor = Mouse.OverrideCursor;

            Mouse.OverrideCursor = Cursors.Wait;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Mouse.OverrideCursor = _previousCursor;
        }

        #endregion
    }

    public class FakeWaitCursor : ICursorBlah
    {
        public void Dispose() { }
    }
}
