using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.TestBench;

namespace Graph.Tests
{
    public class FakeWindowManager : IWindowManager
    {
        private readonly List<Func<object, bool>> _handlersQueue =
            new List<Func<object, bool>>();
        public readonly List<object> Log = new List<object>();
        public bool? ShowDialog(object rootModel, object context = null, IDictionary<string, object> settings = null)
        {
            Log.Add(rootModel);
            var one = _handlersQueue.FirstOrDefault(handler => handler(rootModel));
            if (one == null)
                throw new InvalidOperationException(
                    @"We have forgotten to predefine handler for the following model: " + rootModel);
            _handlersQueue.Remove(one);
            return null;
        }

        public void ShowWindow(object rootModel, object context = null, IDictionary<string, object> settings = null)
        {
            throw new NotImplementedException();
        }

        public void ShowPopup(object rootModel, object context = null, IDictionary<string, object> settings = null)
        {
            throw new NotImplementedException();
        }

        public FakeWindowManager()
        {
            RegisterHandler(m => m is NotificationViewModel);
        }

        public void RegisterHandler(Func<object, bool> del)
        {
            if (del == null) throw new ArgumentNullException(nameof(del));
            _handlersQueue.Add(del);
        }

        public void BaseIsSet()
        {
            RegisterHandler(model =>
            {
                var vm = model as BaseRefsAssignViewModel;
                if (vm == null) return false;
                vm.PreciseBaseFilename = SystemUnderTest2.Path;
                vm.FastBaseFilename = SystemUnderTest2.Path;
                vm.Save();
                return true;
            });
        }
    }
}
