using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using MvcSample.Web.Models;

namespace MvcSample.Web
{
    public class HomeController : Controller
    {
        private MvcSample.Web.DataContext dataContext = new MvcSample.Web.DataContext();
        
        [Route("/{user?}")]
        public IActionResult Index(string user)
        {
            
            Console.WriteLine("USERNAME: "+user);
            
            var results = new List<Fsharp>();
            
            if (user!=null){
                results.AddRange(dataContext.GetByOwner(user));
            }
            
            return View(results);
        }

    }
}