using Microsoft.AspNet.Mvc;
using MvcSample.Web.Models;
using System.Diagnostics;
using System.IO;
using System;
using System.Threading.Tasks;
using CrockSharp;
using curl_sharp;

namespace MvcSample.Web
{
    
    public class PlaygroundController : Controller
    {
        [HttpPost("playground/github")]
        public async Task<IActionResult> Github(string accessCode){
            
            var res = await CurlClient.Post("https://github.com/login/oauth/access_token?client_id=e802f1474d79b005649f&client_secret=e03abf5e8203dac0dc91ebba899f3bd4b4d6664a&code="+accessCode,"");
            return new ObjectResult(res);
        }
        
        [Route("playground/{hash?}")]
       //[HttpGet("{playground/{hash:string}")]
        public IActionResult Index(string hash)
        {
            
            if (hash==null){
                return View(CreateUser(""));
            }
            
            if (!Directory.Exists("saved/"+hash)){
                return View(CreateUser(""));
            }
            
            var saved = System.IO.File.ReadAllText("saved/"+hash+"/saved.txt");
            return View(CreateUser(saved));
        }

        private User CreateUser(string name)
        {
            User user = new User()
            {
                Name = name,
                Address = "My address"
            };

            return user;
        }
        
        [Route("playground/share")]
        [HttpPost]
        public IActionResult Share([FromBody]Command command){
            Random random = new Random();
            
            var bytes = new byte[10];
            random.NextBytes(bytes);
            
            var encoded = CrockSharp.Crock32.Encode(bytes);

            var dir = "saved/"+encoded;
            
            Directory.CreateDirectory(dir);

            using(var fs = System.IO.File.Create(dir+"/saved.txt"))
            using(var writer = new StreamWriter(fs))
            using(var sr = new StringReader(command.Line)){
                
                string line;
                while((line=sr.ReadLine())!=null){
                    writer.WriteLine(line);
                }
            }
            
            return new ObjectResult(encoded);
        }
        
        [HttpPost("playground/compile")]
        public IActionResult Compile([FromBody]Command command)
        {
            
            if (command == null)
            {
                return HttpBadRequest();
            }
            
            ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.FileName = "C:/Program Files (x86)/Microsoft SDKs/F#/4.0/Framework/v4.0/fsi.exe";
            startInfo.FileName = "fsharpi";
            startInfo.Arguments = "--nologo";
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;

            var process = Process.Start(startInfo);

            var reader = process.StandardOutput;
            var writer = process.StandardInput;
            
            using(var sr = new StringReader(command.Line)){
                
                string line;
                while((line=sr.ReadLine())!=null){
                    writer.WriteLine(line);
                }
            }
            
            writer.WriteLine(";;");
            writer.WriteLine("#quit;;");
            
            string output;
            string response="";
            
            while((output = reader.ReadLine())!=null){
                response+=output;
                response+="\r\n";
            }
            
            if (!process.WaitForExit(200)){
                Console.WriteLine("fsharpi process failed to quit in time");
            }
            
            return new ObjectResult(response);
        }
        
    }
    
    public class Command{
        public string Line { get; set; }
    }
}