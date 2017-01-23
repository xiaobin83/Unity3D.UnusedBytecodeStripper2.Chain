using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace UnusedBytecodeStripper2.Chain
{
	class Program
	{
		static int Main(string[] args)
		{
			DumpArgs(args);
			ProcessDlls(args);
			return SpawnOriginalExecutable(args);
		}

		static void ProcessDlls(string[] args)
		{
			var dirOfDlls = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var possibleAssemblies = Directory.GetFiles(dirOfDlls, "*.dll");
			var assemblies = new List<Type>();
			IEnumerable<Type> processors = null;
			foreach (var assemblyFile in possibleAssemblies)
			{
				try
				{
					Log("Loading assembly " + assemblyFile + " to check if contains DllProcessor ... ");
					var assembly = Assembly.LoadFrom(assemblyFile);
					if (assembly != null)
					{
						if (processors == null)
							processors = assembly.GetTypes()
								.Where(t => t.GetCustomAttributes(typeof(DllProcessorAttribute), false).Length > 0);
						else
							processors = processors
								.Union(
								assembly.GetTypes().Where(t => t.GetCustomAttributes(typeof(DllProcessorAttribute), false).Length > 0));
					}
				}
				catch (Exception e)
				{
					Log(e.Message);
				}
			}
			if (assemblies.Count > 0)
			{
				var procs = processors.ToArray();
				System.Array.Sort(
					procs, 
					(a,	b) =>
					{
						var attrA = (DllProcessorAttribute)a.GetCustomAttributes(typeof(DllProcessorAttribute), false)[0];
						var attrB = (DllProcessorAttribute)b.GetCustomAttributes(typeof(DllProcessorAttribute), false)[0];
						return attrA.Priority - attrB.Priority;
					});

				foreach (var proc in procs)
				{
					Log("Execute DllProcessor " + proc.FullName);
					var ctor = proc.GetConstructor(new Type[] { });
					var newProc = (IProcessDll)ctor.Invoke(new object[] { });
					newProc.ProcessDll(args);
				}
			}
			else
			{
				Log("No DllProcessor found.");
			}
		}

		static void DumpArgs(string[] args)
		{
			Log(string.Format("Exe: {0}", Assembly.GetExecutingAssembly().Location));
			Log(string.Format("Path: {0}", Environment.CurrentDirectory));

			Log("---- ARGS -----");
			for (var i = 0; i < args.Length; i++)
				Log(string.Format("args[{0}]='{1}'", i, args[i]));
			Log("---------------");
		}

		static void DumpEnvs()
		{
			Log("---- ENVS-----");
			var variables = Environment.GetEnvironmentVariables();
			foreach (DictionaryEntry item in variables)
				Log(string.Format("{0}={1}", item.Key, item.Value));
			Log("---------------");
		}

		static int SpawnOriginalExecutable(string[] args)
		{
			Log("Execute original UnusedBytecodeStripper2 ... ");
			try
			{
				var monoCfgDir = Environment.GetEnvironmentVariable("MONO_CFG_DIR");
				if (string.IsNullOrEmpty(monoCfgDir))
				{
					// Windows

					var currentModulePath = Assembly.GetExecutingAssembly().Location;
					var orgModulePath = currentModulePath.Substring(0, currentModulePath.Length - 3) + "org.exe";

					var orgArgs = string.Join(" ", args.Select(a => '"' + a + '"'));
					Log(string.Format("Spawn: Exec={0}", orgModulePath));
					Log(string.Format("       Args={0}", orgArgs));
					var handle = Process.Start(orgModulePath, orgArgs);
					handle.WaitForExit();
					return handle.ExitCode;
				}
				else
				{
					// OSX has env-values for running Mono
					// - MONO_PATH=/Applications/Unity531/Unity.app/Contents/Frameworks/MonoBleedingEdge/lib/mono/4.0
					// - MONO_CFG_DIR=/Applications/Unity531/Unity.app/Contents/Frameworks/MonoBleedingEdge/etc

					var monoPath = monoCfgDir.Substring(0, monoCfgDir.Length - 3) + "bin/mono";
					var currentModulePath = Assembly.GetExecutingAssembly().Location;
					var orgModulePath = currentModulePath.Substring(0, currentModulePath.Length - 3) + "org.exe";

					var orgArgs = '"' + orgModulePath + '"' + ' ' + string.Join(" ", args.Select(a => '"' + a + '"'));
					Log(string.Format("Spawn: Mono={0}", monoPath));
					Log(string.Format("       Exec={0}", orgModulePath));
					Log(string.Format("       Args={0}", orgArgs));
					var handle = Process.Start(monoPath, orgArgs);
					handle.WaitForExit();
					return handle.ExitCode;
				}
			}
			catch (Exception e)
			{
				Log(string.Format("SpawnOriginalExecutable got exception. Exception={0}", e));
				DumpEnvs();
				return 1;
			}
		}

		static void Log(string log)
		{
			File.AppendAllText("UnusedBytecodeStripper2.Chain.txt", log + "\n");
			Console.WriteLine(log);
		}
	}
}
