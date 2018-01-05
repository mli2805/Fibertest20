using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class ConfirmationViewModel : Screen
    {
        private readonly string _caption;
        public List<ConfirmaionLineModel> Lines { get; set; }
        public bool IsAnswerPositive { get; set; }

        public ConfirmationViewModel(string caption, List<ConfirmaionLineModel> lines)
        {
            _caption = caption;
            Lines = lines;
            IsAnswerPositive = false;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _caption;
        }
        public void OkButton()
        {
            IsAnswerPositive = true;
            TryClose();
        }

        public void CancelButton()
        {
            IsAnswerPositive = false;
            TryClose();
        }

        /// <summary>Just for debug purposes </summary>
        [Localizable(false)]
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "QuestionViewModel:" + Lines.FirstOrDefault();
        }
    }
}
