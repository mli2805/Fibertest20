using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Iit.Fibertest.Client;

namespace Graph.Tests
{
    public class FakeWindowManager : IMyWindowManager
    {
        private readonly List<Func<object, bool>> _handlersQueue =
            new List<Func<object, bool>>();
        public readonly List<object> Log = new List<object>();
        //public bool? ShowDialog(object rootModel, object context = null, IDictionary<string, object> settings = null)
        public bool? ShowDialog(object rootModel)
        {
            Log.Add(rootModel);
            var one = _handlersQueue.FirstOrDefault(handler => handler(rootModel));
            if (one == null)
                throw new InvalidOperationException(
                    @"We have forgotten to predefine handler for the following model: " + rootModel);
            _handlersQueue.Remove(one);
            return null;
        }

        [ExcludeFromCodeCoverage]
        //public void ShowWindow(object rootModel, object context = null, IDictionary<string, object> settings = null)
        public void ShowWindow(object rootModel)
        {
            Log.Add(rootModel);
            var one = _handlersQueue.FirstOrDefault(handler => handler(rootModel));
            if (one == null)
                throw new InvalidOperationException(
                    @"We have forgotten to predefine handler for the following model: " + rootModel);
            _handlersQueue.Remove(one);
        }

        [ExcludeFromCodeCoverage]
        public void ShowPopup(object rootModel, object context = null, IDictionary<string, object> settings = null)
        {
            throw new NotImplementedException();
        }

        public FakeWindowManager()
        {
            RegisterHandler(m => m is NotificationViewModel);
            RegisterHandler(m => m is LoginViewModel);
        }

        public void RegisterHandler(Func<object, bool> del)
        {
            if (del == null) throw new ArgumentNullException(nameof(del));
            _handlersQueue.Add(del);
        }

     
    }
}
