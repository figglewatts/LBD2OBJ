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
			if (args.Length > 0)
			{
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
			}
			else
			{
				Console.WriteLine("Please enter path to file...");
				string filePath = Console.ReadLine();
				ConvertTMD(filePath);
			}
			Console.Write("Press ENTER to exit...");
            Console.ReadLine();
		}

		public static TMD ConvertTMD(string path)
		{
			Console.WriteLine("Converting TMD...");
			TMD tmd;
			using (BinaryReader b = new BinaryReader(File.Open(path, FileMode.Open)))
			{
				tmd.header = readHeader(b);
				tmd.fixP = (tmd.header.flags & 1) == 1 ? true : false; // if true, pointers are fixed; if false, pointers are relative

				tmd.objTop = b.BaseStream.Position;
				tmd.objTable = new OBJECT[tmd.header.numObjects];
				for (int i = 0; i < tmd.header.numObjects; i++)
				{
					tmd.objTable[i] = readObject(b, tmd.objTop);
				}
			}
			return tmd;
		}

		private static TMDHEADER readHeader(BinaryReader b)
		{
			TMDHEADER tmdHeader;
			tmdHeader.ID = b.ReadInt32();
			tmdHeader.flags = b.ReadUInt32();
			tmdHeader.numObjects = b.ReadUInt32();
			return tmdHeader;
		}

		private static OBJECT readObject(BinaryReader b, long objTop)
		{
			OBJECT obj;
			obj.vertTop = b.ReadUInt32();
			obj.numVerts = b.ReadUInt32();
			obj.vertices = new VERTEX[obj.numVerts];
			obj.normTop = b.ReadUInt32();
			obj.numNorms = b.ReadUInt32();
			obj.normals = new NORMAL[obj.numNorms];
			obj.primTop = b.ReadUInt32();
			obj.numPrims = b.ReadUInt32();
			obj.scale = b.ReadInt32();

			long cachedPosition = b.BaseStream.Position; // cache position so we can return

			// go to address of vertices for object and load them
			b.BaseStream.Seek(objTop + obj.vertTop, SeekOrigin.Begin);
			for (int v = 0; v < obj.numVerts; v++)
			{
				obj.vertices[v] = readVertex(b);
			}

			// go to address of normals for object and load them
			b.BaseStream.Seek(objTop + obj.normTop, SeekOrigin.Begin);
			for (int n = 0; n < obj.numNorms; n++)
			{
				obj.normals[n] = readNormal(b);
			}
			b.BaseStream.Seek(cachedPosition, SeekOrigin.Begin); // return to cached position to load next object

			return obj;
		}

		private static VERTEX readVertex(BinaryReader b)
		{
			VERTEX v;
			v.X = b.ReadInt16();
			v.Y = b.ReadInt16();
			v.Z = b.ReadInt16();
			b.BaseStream.Seek(2, SeekOrigin.Current); // unused data, for alignment
			return v;
		}

		private static NORMAL readNormal(BinaryReader b)
		{
			NORMAL n;
			n.nX = new FixedPoint(b.ReadBytes(2));
			n.nY = new FixedPoint(b.ReadBytes(2));
			n.nZ = new FixedPoint(b.ReadBytes(2));
			b.BaseStream.Seek(2, SeekOrigin.Current); // unused data, for alignment
			return n;
		}

		private static PRIMITIVE readPrimitive(BinaryReader b)
		{
			PRIMITIVE p;
			p.olen = b.ReadByte();
			p.ilen = b.ReadByte();
			p.flag = b.ReadByte();
			p.mode = b.ReadByte();
			p.classification = readPrimitiveClassification(b, p.mode);
			p.data = readPrimitiveData(b, p.classification);
			return p;
		}

		private static PRIMITIVECLASSIFICATION readPrimitiveClassification(BinaryReader b, int mode)
		{
			PRIMITIVECLASSIFICATION classification;
			// it's a bitmask
			if ((mode & 224) != 32) { throw new InvalidDataException("Unrecognized polygon mode"); }
			classification.gouraudShaded = intToBool(mode & 1);
			mode >>= 1;
			classification.quad = intToBool(mode & 1);
			mode >>= 1;
			classification.textureMapped = intToBool(mode & 1);
			mode >>= 2; // skip bit at 6, we don't need it
			classification.unlit = intToBool(mode & 1);
			return classification;

		}

		private static PRIMITIVEDATA readPrimitiveData(BinaryReader b, PRIMITIVECLASSIFICATION classification)
		{
			
		}

		private static bool intToBool(int input)
		{
			return input != 0 ? true : false;
		}
	}
}
