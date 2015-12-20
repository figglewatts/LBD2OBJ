using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LBD2OBJ.Types;

namespace LBD2OBJ
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("LBD2OBJ - Made by Figglewatts 2015");
			foreach (string arg in args)
			{
				if (Path.GetExtension(arg).ToLower().Equals("tmd"))
				{
					// convert from tmd
				}
				else if (Path.GetExtension(arg).ToLower().Equals("lbd"))
				{
					// convert from lbd
				}
				else
				{
					Console.WriteLine("Invalid input file. Extension: {0} not recognized.", Path.GetExtension(arg));
				}
			}
			Console.Write("Press ENTER to exit...");
            Console.ReadLine();
		}

		public static void ConvertTMD(string path)
		{
			Console.WriteLine("Converting TMD...");
			TMD tmd;
			bool fixP; // if true, pointers are fixed i.e. non-relative
			using (BinaryReader b = new BinaryReader(File.Open(path, FileMode.Open)))
			{
				tmd.header = readHeader(b);
				fixP = (tmd.header.flags & 1) == 1 ? true : false;

				tmd.objTable = new OBJECT[tmd.header.numObjects];
				for (int i = 0; i < tmd.header.numObjects; i++)
				{
					tmd.objTable[i] = readObject(b);
				}
			}
		}

		private static TMDHEADER readHeader(BinaryReader b)
		{
			TMDHEADER tmdHeader;
			tmdHeader.ID = b.ReadUInt32();
			tmdHeader.flags = b.ReadUInt32();
			tmdHeader.numObjects = b.ReadUInt32();
			return tmdHeader;
		}

		private static OBJECT readObject(BinaryReader b)
		{
			OBJECT obj;
			obj.vertTop = b.ReadUInt32();
			obj.numVerts = b.ReadUInt32();
			obj.normTop = b.ReadUInt32();
			obj.numNorms = b.ReadUInt32();
			obj.primTop = b.ReadUInt32();
			obj.numPrims = b.ReadUInt32();
			obj.scale = b.ReadInt32();
			return obj;
		}
	}
}
