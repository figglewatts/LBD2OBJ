using System;
using System.IO;
using LBD2OBJLib;
using LBD2OBJLib.Types;

namespace LBD2OBJ
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("LBD2OBJ - Made by Figglewatts 2015");
			if (args.Length > 0)
			{
				int argCount = 0;
				if (args[0] == "-s")
				{
					LBDConverter.CreateSeparateOBJsForMultipleObjects(true);
					argCount++;
				}
				for (int i = argCount; i < args.Length; i++)
				{
					string arg = args[i];
					if (Path.GetExtension(arg).ToLower().Equals(".tmd"))
					{
						string filePath = arg;
						LBDConverter.WriteToObj(filePath, LBDConverter.ConvertTMD(filePath));
					}
					else if (Path.GetExtension(arg).ToLower().Equals(".lbd"))
					{
						LBDConverter.CreateSeparateOBJsForMultipleObjects(true);
						LBDConverter.WriteToObj(arg, LBDConverter.GetTMDFromLBD(arg), "_LBD");
						LBDConverter.CreateSeparateOBJsForMultipleObjects(false);
						TMD[] tmds = LBDConverter.GetTMDFromMOMinLBD(arg);
						if (tmds != null)
						{
							foreach (TMD tmd in tmds)
							{
								LBDConverter.WriteToObj(arg, tmd, "_MOM");
							}
						}
					}
					else if (Path.GetExtension(arg).ToLower().Equals(".mom"))
					{
						LBDConverter.WriteToObj(arg, LBDConverter.GetTMDFromMOM(arg), "_MOM");
					}
					else
					{
						Console.WriteLine("Invalid input file. Extension: {0} not recognized.", Path.GetExtension(arg));
					}
				}
			}
			else
			{
				Console.WriteLine("Please enter path to file...");
				Console.Write(">");
				string filePath = Console.ReadLine();

				if (Path.GetExtension(filePath).ToLower().Equals(".tmd"))
				{
					LBDConverter.WriteToObj(filePath, LBDConverter.ConvertTMD(filePath));
				}
				else if (Path.GetExtension(filePath).ToLower().Equals(".lbd"))
				{
					LBDConverter.CreateSeparateOBJsForMultipleObjects(true);
					LBDConverter.WriteToObj(filePath, LBDConverter.GetTMDFromLBD(filePath), "_LBD");
					LBDConverter.CreateSeparateOBJsForMultipleObjects(false);
					TMD[] tmds = LBDConverter.GetTMDFromMOMinLBD(filePath);
					if (tmds != null)
					{
						foreach (TMD tmd in tmds)
						{
							LBDConverter.WriteToObj(filePath, tmd, "_MOM");
						}
					}
				}
				else if (Path.GetExtension(filePath).ToLower().Equals(".mom"))
				{
					LBDConverter.WriteToObj(filePath, LBDConverter.GetTMDFromMOM(filePath), "_MOM");
				}
				else
				{
					Console.WriteLine("Invalid input file. Extension: {0} not recognized.", Path.GetExtension(filePath));
				}
			}
			Console.Write("Press ENTER to exit...");
            Console.ReadLine();
		}

		

		
	}
}
