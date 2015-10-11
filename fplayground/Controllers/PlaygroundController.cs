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
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;

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
        public async Task<IActionResult> Share([FromBody]Command command){
                        
            var fsharp = new Fsharp(){
                Owner = command.Owner,
                Content = command.Line
            };
            
            var saved = await dataContext.SaveAsync(fsharp);
            
            return new ObjectResult(saved.Hash);
        }
        
        [HttpPost("playground/compile")]
        public async Task<IActionResult> Compile([FromBody]Command command)
        {
            
            if (command == null)
            {
                return HttpBadRequest();
            }
            
            var request = new FsharpRequest(){ Body=command.Line };
            
            await FsharpWorker.requestQueue.SendAsync(request);
            
            var response = await request.GetResultAsync();
            
            return new ObjectResult(response);
        }
        
    }
    
    public class Command{
        public string Line { get; set; }
        public string Owner { get; set; }
    }
    

}