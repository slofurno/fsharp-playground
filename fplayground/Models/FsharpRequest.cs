using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using System.Diagnostics;
using System.IO;

odels
{
	public class FsharpRequest{
		
		private TaskCompletionSource<string> _result;
		
		public string Body { get; set; }
		public Task<string> GetResultAsync (){
			_result = new TaskCompletionSource<string>();
			return _result.Task;
		}
		
		public void SetResult(string result){
			_result.TrySetResult(result);
		}
		
	}
	
	public class FsharpWorker{
		
		public static BufferBlock<FsharpRequest> requestQueue;
		static Task worker;
		
		static FsharpWorker(){
			
			requestQueue = new BufferBlock<FsharpRequest>();
			
			worker = Task.Run(async ()=>{
				
				while(true){
				
					var request = await requestQueue.ReceiveAsync();
					
					ProcessStartInfo startInfo = new ProcessStartInfo();
				
					startInfo.FileName = "fsharpi";
					startInfo.Arguments = "--nologo";
	
					startInfo.RedirectStandardInput = true;
					startInfo.RedirectStandardOutput = true;
					startInfo.UseShellExecute = false;
		
					var process = Process.Start(startInfo);
		
					var reader = process.StandardOutput;
					var writer = process.StandardInput;
					
					
					using(var sr = new StringReader(request.Body)){
						
						string line;
						while((line=await sr.ReadLineAsync())!=null){
							writer.WriteLine(line);
						}
					}
					
					writer.WriteLine(";;");
					writer.WriteLine("#quit;;");
					
					string output;
					string response="";
					
					while((output = await reader.ReadLineAsync())!=null){
						response+=output;
						response+="\r\n";
					}
					
					if (!process.WaitForExit(200)){
						Console.WriteLine("fsharpi process failed to quit in time");
					}
					
					request.SetResult(response);
				}
				
			});
			
		}
		
	}
	
}