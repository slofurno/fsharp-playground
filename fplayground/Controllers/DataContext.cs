using System.IO;
using System;
using MvcSample.Web.Models;
using System.Collections.Generic;
using System.Linq;

namespace MvcSample.Web
{
	
	public class DataContext
	{
			
		private static List<Fsharp> Lookup;
			
		static DataContext()
		{
				
				Lookup = new List<Fsharp>();
				
				var dirs =
				from d in Directory.EnumerateDirectories("saved")
				select d.Split(new[] { '/', '\\' }).Last();
				
				foreach (var hash in dirs){
					
					var fsharp = new Fsharp();
				
					fsharp.Owner = File.ReadAllText("saved/"+hash+"/owner");
					fsharp.Content = File.ReadAllText("saved/"+hash+"/content");
					fsharp.Hash=hash;
					
					Lookup.Add(fsharp);
				}
				
		}
		
		public DataContext(){
			
		}
		
		
		public Fsharp GetByHash(string hash)
		{
			return Lookup.Where(x=>x.Hash==hash).FirstOrDefault();
		}
		
		public List<Fsharp> GetByOwner(string owner){
			
			return Lookup.Where(x=>x.Owner==owner).ToList();
		}
		
		public void Save(Fsharp fsharp){
			
			Lookup.Add(fsharp);
			
			var encoded = fsharp.Hash;

            var dir = "saved/"+encoded;
            
            Directory.CreateDirectory(dir);

            using(var fs = System.IO.File.Create(dir+"/content"))
            using(var writer = new StreamWriter(fs))
            using(var sr = new StringReader(fsharp.Content)){
                
                string line;
                while((line=sr.ReadLine())!=null){
                    writer.WriteLine(line);
                }
            }
			
			using(var fs = File.Create(dir+"/owner"))
			using(var writer = new StreamWriter(fs)){
				writer.Write(fsharp.Owner);
			}
			
		}
		
	
	
	}
}
	
