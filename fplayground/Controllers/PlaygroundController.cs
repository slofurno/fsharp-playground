using Microsoft.AspNet.Mvc;
using MvcSample.Web.Models;
using System.Diagnostics;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using CrockSharp;
using curl_sharp;

namespace MvcSample.Web
{
    
    public class PlaygroundController : Controller
    {
        
        private MvcSample.Web.DataContext dataContext = new MvcSample.Web.DataContext();
        
        [HttpPost("playground/github")]
        public async Task<IActionResult> Github(string accessCode){
            
            var res = await CurlClient.Curl("-X POST https://github.com/login/oauth/access_token?client_id=e802f1474d79b005649f&client_secret=e03abf5e8203dac0dc91ebba899f3bd4b4d6664a&code="+accessCode);
            return new ObjectResult(res);
        }
        
        [Route("playground/{hash?}")]
       //[HttpGet("{playground/{hash:string}")]
        public IActionResult Index(string hash)
        {
            var result = new Fsharp();
            
            if (!string.IsNullOrEmpty(hash)){
                var match = dataContext.GetByHash(hash);
                if (match!=null){
                    result = match;
                }
            }
            
            return View(result);
            
            
            /*
            string content = "";
            
            var dirs =
                from d in Directory.EnumerateDirectories("saved")
                select d.Split(new[] { '/', '\\' }).Last();
                
            if (hash!=null && dirs.Contains(hash)){
                content = System.IO.File.ReadAllText("saved/"+hash+"/saved.txt");
            }
                    
            var vm = new PlaygroundViewModel(){ 
                RecentHashes = dirs,
                Content=content
            };
            
            return View(vm);
            */
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
            
            var fsharp = new Fsharp(){
                Hash = encoded,
                Owner = command.Owner,
                Content = command.Line
            };
            
            dataContext.Save(fsharp);
           
            
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
            //startInfo.FileName = "fsharpi";
            //startInfo.Arguments = "--nologo";
            
            var args = "-e " + "\"$(echo " + command.Line + ")\"";
            
            Console.WriteLine(args);
            
            startInfo.FileName = "scala";
            startInfo.Arguments = args;
            //scala -e "$(cat test.scala)"

            
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;

            var process = Process.Start(startInfo);

            var reader = process.StandardOutput;
            var writer = process.StandardInput;
            
            /*
            using(var sr = new StringReader(command.Line)){
                
                string line;
                while((line=sr.ReadLine())!=null){
                    writer.WriteLine(line);
                }
            }
            
            writer.WriteLine(";;");
            writer.WriteLine("#quit;;");
            */
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
        public string Owner { get; set; }
    }
    

}