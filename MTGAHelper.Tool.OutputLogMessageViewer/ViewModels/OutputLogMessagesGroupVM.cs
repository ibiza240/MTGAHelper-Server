using System.Collections.Generic;
using MTGAHelper.Lib.OutputLogParser.Models;

namespace MTGAHelper.Tool.OutputLogMessageViewer.ViewModels
{
    public class OutputLogMessagesGroupVM
    {
        public OutputLogMessagesGroupVM()
        {
        }

        public OutputLogMessagesGroupVM(string name)
        {
            Name = name;
        }

        public OutputLogMessagesGroupVM(string name, IMtgaOutputLogPartResult message)
        {
            Name = name;
            Message = message;
        }

        public string Name { get; set; } = "";
        public IMtgaOutputLogPartResult Message { get; set; }
        public IList<OutputLogMessagesGroupVM> Children { get; set; } = new List<OutputLogMessagesGroupVM>();

        public bool IsExpanded { get; set; }
        public bool IsSelected { get; set; }
    }
}
