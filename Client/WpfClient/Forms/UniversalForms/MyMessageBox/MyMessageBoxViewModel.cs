using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class MyMessageBoxViewModel : Screen
    {
        private readonly string _caption;
        public List<MyMessageBoxLineModel> Lines { get; set; }
        public Visibility CancelVisibility { get; set; }
        public bool IsAnswerPositive { get; set; }


        public MyMessageBoxViewModel(MessageType messageType, string message)
        {
            Lines = new List<MyMessageBoxLineModel>()
            {
                new MyMessageBoxLineModel(){Line = message, FontWeight = FontWeights.Bold}
            };

            _caption = messageType.GetLocalizedString();
            CancelVisibility = messageType.ShouldCancelBeVisible();
            IsAnswerPositive = false;
        }

        public MyMessageBoxViewModel(MessageType messageType, List<string> strs)
        {
            Lines = strs.Select(s => new MyMessageBoxLineModel() {Line = s}).ToList();

            _caption = messageType.GetLocalizedString();
            CancelVisibility = messageType.ShouldCancelBeVisible();
            IsAnswerPositive = false;
        }

        public MyMessageBoxViewModel(MessageType messageType, List<MyMessageBoxLineModel> lines)
        {
            Lines = lines;

            _caption = messageType.GetLocalizedString();
            CancelVisibility = messageType.ShouldCancelBeVisible();
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
            return "MyMessageBoxViewModel:" + Lines.FirstOrDefault();
        }
    }
}
