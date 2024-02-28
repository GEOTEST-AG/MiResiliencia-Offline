using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.GUI.Model
{
    public class LanguageModel
    {
        public string Language { get; set; }
        public string Text { get; set; }
        public bool IsChecked { get; set; }

        public override string ToString() { return Text; }
    }
}
