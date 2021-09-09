using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeplerProjectTemplate1.Interfaces.LegilityTest.v1.Models
{
    public class ServiceResponse<T>
    {
        public T Data { get; set; }
        public bool Success { get; set; } = true;
        public string Message { get; set; } = null;
        public Exception Exception { get; set; }
        public int StatusCode { get; set; } = 200;
    }
}
