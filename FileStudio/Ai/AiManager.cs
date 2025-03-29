using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStudio.Ai
{
    class AiManager
    {
        private string _apiKey;

        public AiManager()
        {
            var apiKey = Environment.GetEnvironmentVariable("AI_API_KEY");
        }
    }
}
