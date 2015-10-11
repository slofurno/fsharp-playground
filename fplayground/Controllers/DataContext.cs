using System.IO;
using System;
using MvcSample.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using System.Linq;
using CrockSharp;

namespace MvcSample.Web
{
	
	public class FsharpSaveRequest{
		private TaskCompletionSource<Fsharp> _result;
		public Fsharp fsharp { get; set; }
		
		public Task<Fsharp> GetResultAsync (){
			_result = new TaskCompletionSource<Fsharp>();
			return _result.Task;
		}
		
		public bool SetResult(Fsharp result){
			return _result.TrySetResult(result);
		}
	}
	
	public class DataContextWorker{
		private BufferBlock<FsharpSaveRequest> _queue;
		private List<Fsharp> _lookup;
		private Random random;
		
		public DataContextWorker(BufferBlock<FsharpSaveRequest> queue, List<Fsharp> lookup){
			_queue = queue;
			_lookup = lookup;
			random = new Random();
		}
		
		public async Task Process(){
			
			while(true){

				var request = await requestQueue.ReceiveAsync();
            
				var bytes = new byte[10];
				random.NextBytes(bytes);
				var encoded = CrockSharp.Crock32.Encode(bytes);
				
				Lookup.Add(fsharp);
				
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
				
				request.SetResult(fsharp);
	
			}
		}
		
	}
	
	public class DataContext
	{
			
		private static List<Fsharp> Lookup;
		public static BufferBlock<FsharpSaveRequest> requestQueue;
		static DataContextWorker worker;
			
		static DataContext()
		{
				requestQueue= new BufferBlock<FsharpSaveRequest>();
				Lookup = new List<Fsharp>();
				worker = new DataContextWorker(requestQueue,lookup);
				
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
				
				worker.Process();
				
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
		
		public async Task<Fsharp> SaveAsync(Fsharp fsharp){
			
			var request = new FsharpSaveRequest(){fsharp=Fsharp};
			await requestQueue.SendAsync(fsharp);
			return request.GetResultAsync();
			
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
	
