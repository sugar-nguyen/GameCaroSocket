using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace GameCaroSocket.Pages
{
    public class IndexModel : PageModel
    {
        [Obsolete]
        private readonly IHostingEnvironment _host;

        [Obsolete]
        public IndexModel(IHostingEnvironment host)
        {
            _host = host;
        }

        [Obsolete]
        public void OnGet()
        {

            string ip_log_path = Path.Combine(_host.WebRootPath, "lib", "log");
            string ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();
            string fileName = System.IO.Path.Combine(ip_log_path, "log.txt");

            if (!System.IO.File.Exists(fileName))
            {
                System.IO.File.WriteAllText(fileName, "");
            }

            try
            {
                int line = System.IO.File.ReadAllLines(fileName).Where(x=>!x.Contains("------------")).Count();
                using (FileStream stream = new FileStream(fileName, FileMode.Append))
                {
                    if (line == 0)
                    {
                        line = 1;
                    }
                    else
                    {
                        line += 1;
                    }

                    byte[] bytes = Encoding.UTF8.GetBytes(line + ".\t" + ip + '\t' + DateTime.Now + "\n-----------------------------------------------\n");;
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
            catch (IOException)
            {
            }

        }
    }
}
