using System;

namespace UnusedBytecodeStripper2.Chain
{
	public interface IProcessDll
	{
		void ProcessDll(string[] args);
	}

	public class DllProcessorAttribute : Attribute
	{
		public int Priority = 0;
	}
}
