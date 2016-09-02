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
			bool separateObjects = false;

			LBDConverter.CreateSeparateOBJsForMultipleObjects(true);
		
			Console.WriteLine("LBD2OBJ - Made by Figglewatts 2015");
			if (args.Length > 0)
			{
				int argCount = 0;
				for (int i = argCount; i < args.Length; i++)
				{
					string arg = args[i];
					if (Path.GetExtension(arg).ToLower().Equals(".tmd"))
					{
						string filePath = arg;

						WriteTMDToObjects(LBDConverter.ConvertTMD(filePath), Path.GetDirectoryName(arg), "TMD");
					}
					else if (Path.GetExtension(arg).ToLower().Equals(".lbd"))
					{
						WriteTMDToObjects(LBDConverter.GetTMDFromLBD(arg), Path.GetDirectoryName(arg), "LBD_TMD");
						TMD[] tmds = LBDConverter.GetTMDFromMOMinLBD(arg);
						if (tmds != null)
						{
							foreach (TMD tmd in tmds)
							{
								WriteTMDToObjects(tmd, Path.GetDirectoryName(arg), "LBD_MOM_TMD");
							}
						}
					}
					else if (Path.GetExtension(arg).ToLower().Equals(".mom"))
					{
						WriteTMDToObjects(LBDConverter.GetTMDFromMOM(arg), Path.GetDirectoryName(arg), "MOM_TMD");
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
					WriteTMDToObjects(LBDConverter.ConvertTMD(filePath), Path.GetDirectoryName(filePath), "TMD");
				}
				else if (Path.GetExtension(filePath).ToLower().Equals(".lbd"))
				{
					WriteTMDToObjects(LBDConverter.GetTMDFromLBD(filePath), Path.GetDirectoryName(filePath), "LBD_TMD");
					TMD[] tmds = LBDConverter.GetTMDFromMOMinLBD(filePath);
					if (tmds != null)
					{
						foreach (TMD tmd in tmds)
						{
							WriteTMDToObjects(tmd, Path.GetDirectoryName(filePath), "LBD_MOM_TMD");
						}
					}
				}
				else if (Path.GetExtension(filePath).ToLower().Equals(".mom"))
				{
					WriteTMDToObjects(LBDConverter.GetTMDFromMOM(filePath), Path.GetDirectoryName(filePath), "MOM_TMD");
				}
				else
				{
					Console.WriteLine("Invalid input file. Extension: {0} not recognized.", Path.GetExtension(filePath));
				}
			}
			Console.Write("Press ENTER to exit...");
            Console.ReadLine();
		}

		private static void WriteTMDToObjects(TMD t, string outputDir, string prefix)
		{
			int i = 0;
			foreach (OBJECT o in t.objTable)
			{
				using (StreamWriter w = new StreamWriter(File.Open(Path.Combine(outputDir, prefix + "_OBJECT_" + i + ".obj"), FileMode.OpenOrCreate)))
				{
					LBDConverter.WriteObjectToObj(o, w, i);
				}
				i++;
			}
		}
	}
}
