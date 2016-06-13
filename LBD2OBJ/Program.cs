using System;
using System.IO;
using LBD2OBJLib;
using LBD2OBJLib.Types;

namespace LBD2OBJ
{
	class Program
	{
		public static bool separateObjects = false;
	
		static void Main(string[] args)
		{
			Console.WriteLine("LBD2OBJ - Made by Figglewatts 2015");
			if (args.Length > 0)
			{
				int argCount = 0;
				if (args[0] == "-s")
				{
					separateObjects = true;
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
						separateObjects = true;
						WriteToObj(arg, getTMDFromLBD(arg), "_LBD", separateObjects);
						separateObjects = false;
						TMD[] tmds = getTMDFromMOMinLBD(arg);
						foreach (TMD tmd in tmds)
						{
							WriteToObj(arg, tmd, "_MOM");
						}
					}
					else if (Path.GetExtension(arg).ToLower().Equals(".mom"))
					{
						WriteToObj(arg, getTMDFromMOM(arg), "_MOM");
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
					WriteToObj(filePath, ConvertTMD(filePath));
				}
				else if (Path.GetExtension(filePath).ToLower().Equals(".lbd"))
				{
					separateObjects = true;
					WriteToObj(filePath, getTMDFromLBD(filePath), "_LBD", separateObjects);
					separateObjects = false;
					TMD[] tmds = getTMDFromMOMinLBD(filePath);
					foreach (TMD tmd in tmds)
					{
						WriteToObj(filePath, tmd, "_MOM");
					}
				}
				else if (Path.GetExtension(filePath).ToLower().Equals(".mom"))
				{
					WriteToObj(filePath, getTMDFromMOM(filePath), "_MOM");
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
