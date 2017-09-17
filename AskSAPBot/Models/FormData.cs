using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskSAPBOT.Models
{
    [Serializable]
    public class FormData
    {
        public string id;
        public string Description;
        public List<string> Terms;
        public FormData() { }
        public FormData(string id, string description, List<string> terms)
        {
            this.id = id;
            this.Description = description;
            this.Terms = terms;
        }
    }
}
