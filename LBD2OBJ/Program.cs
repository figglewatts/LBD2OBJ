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
			obj.primitives = new PRIMITIVE[obj.numPrims];
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

			// go to address of primitives for object and load them
			b.BaseStream.Seek(objTop + obj.primTop, SeekOrigin.Begin);
			for (int p = 0; p < obj.numPrims; p++)
			{
				obj.primitives[p] = readPrimitive(b);
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
			p.classification = readPrimitiveClassification(b, p.mode, p.flag);
			p.data = readPrimitiveData(b, p.classification);
			return p;
		}

		private static PRIMITIVECLASSIFICATION readPrimitiveClassification(BinaryReader b, int mode, int flags)
		{
			PRIMITIVECLASSIFICATION classification;
			// a bitmask of 11100000 to check if the mode is 001, i.e. a polygon
			if ((mode & 224) != 32) { throw new InvalidDataException("Unrecognized polygon mode"); }
			classification.gouraudShaded = intToBool(mode & 1);
			mode >>= 1;
			classification.quad = intToBool(mode & 1);
			mode >>= 1;
			classification.textureMapped = intToBool(mode & 1);

			classification.unlit = intToBool(flags & 1);
			flags >>= 1;
			classification.twoSided = intToBool(flags & 1);
			flags >>= 1;
			classification.gradation = intToBool(flags & 1);
			
			return classification;

		}

		// this method, lol, hope you like nested if statements
		private static PRIMITIVEDATA readPrimitiveData(BinaryReader b, PRIMITIVECLASSIFICATION classification)
		{
			PRIMITIVEDATA data = new PRIMITIVEDATA();
			if (classification.quad)
			{
				if (classification.unlit)
				{
					if (classification.gradation)
					{
						if (classification.textureMapped)
						{
							// fig. 2-54 d
							data.uvCoords = new UV[4];
							for (int i = 0; i < 4; i++)
							{
								data.uvCoords[i] = readUV(b);
								skipBytes(2, b);
							}
							skipBytes(16, b);
							data.triangleIndices = new short[4];
							for (int i = 0; i < 4; i++)
							{
								data.triangleIndices[i] = b.ReadInt16();
							}

						}
						else
						{
							// fig. 2-54 c
							skipBytes(16, b);
							data.triangleIndices = new short[4];
							for (int i = 0; i < 4; i++)
							{
								data.triangleIndices[i] = b.ReadInt16();
							}
						}
					}
					else
					{
						if (classification.textureMapped)
						{
							// fig. 2-54 b
							data.uvCoords = new UV[4];
							for (int i = 0; i < 4; i++)
							{
								data.uvCoords[i] = readUV(b);
								skipBytes(2, b);
							}
							skipBytes(4, b);
							data.triangleIndices = new short[4];
							for (int i = 0; i < 4; i++)
							{
								data.triangleIndices[i] = b.ReadInt16();
							}
						}
						else
						{
							// fig. 2-54 a
							skipBytes(4, b);
							data.triangleIndices = new short[4];
							for (int i = 0; i < 4; i++)
							{
								data.triangleIndices[i] = b.ReadInt16();
							}
						}
					}
				}
				else
				{
					if (classification.gouraudShaded)
					{
						if (classification.textureMapped)
						{
							// fig. 2-50 f
							data.uvCoords = new UV[4];
							for (int i = 0; i < 4; i++)
							{
								data.uvCoords[i] = readUV(b);
								skipBytes(2, b);
							}

							data.normalIndices = new short[4];
							data.triangleIndices = new short[4];
							for (int i = 0; i < 4; i++)
							{
								data.normalIndices[i] = b.ReadInt16();
								data.triangleIndices[i] = b.ReadInt16();
							}
						}
						else
						{
							if (classification.gradation)
							{
								// fig. 2-50 e
								skipBytes(16, b);

								data.normalIndices = new short[4];
								data.triangleIndices = new short[4];
								for (int i = 0; i < 4; i++)
								{
									data.normalIndices[i] = b.ReadInt16();
									data.triangleIndices[i] = b.ReadInt16();
								}
							}
							else
							{
								// fig. 2-50 d
								skipBytes(4, b);

								data.normalIndices = new short[4];
								data.triangleIndices = new short[4];
								for (int i = 0; i < 4; i++)
								{
									data.normalIndices[i] = b.ReadInt16();
									data.triangleIndices[i] = b.ReadInt16();
								}
							}
						}
					}
					else
					{
						if (classification.textureMapped)
						{
							// fig. 2-50 c
							data.uvCoords = new UV[4];
							for (int i = 0; i < 4; i++)
							{
								data.uvCoords[i] = readUV(b);
								skipBytes(2, b);
							}

							data.normalIndices = new short[1];
							data.normalIndices[0] = b.ReadInt16();

							data.triangleIndices = new short[4];
							for (int i = 0; i < 4; i++)
							{
								data.triangleIndices[i] = b.ReadInt16();
							}
							skipBytes(2, b);
						}
						else
						{
							if (classification.gradation)
							{
								// fig. 2-50 b
								skipBytes(16, b);

								data.normalIndices = new short[1];
								data.normalIndices[0] = b.ReadInt16();

								data.triangleIndices = new short[4];
								for (int i = 0; i < 4; i++)
								{
									data.triangleIndices[i] = b.ReadInt16();
								}
								skipBytes(2, b);
							}
							else
							{
								// fig. 2-50 a
								skipBytes(4, b);

								data.normalIndices = new short[1];
								data.normalIndices[0] = b.ReadInt16();

								data.triangleIndices = new short[4];
								for (int i = 0; i < 4; i++)
								{
									data.triangleIndices[i] = b.ReadInt16();
								}
								skipBytes(2, b);
							}
						}
					}
				}
			}
			else
			{
				if (classification.unlit)
				{
					if (classification.gradation)
					{
						if (classification.textureMapped)
						{
							// fig. 2-52 d
							data.uvCoords = new UV[3];
							for (int i = 0; i < 3; i++)
							{
								data.uvCoords[i] = readUV(b);
								skipBytes(2, b);
							}
							skipBytes(12, b);

							data.triangleIndices = new short[3];
							for (int i = 0; i < 3; i++)
							{
								data.triangleIndices[i] = b.ReadInt16();
							}
							skipBytes(2, b);
						}
						else
						{
							// fig. 2-52 c
							skipBytes(12, b);

							data.triangleIndices = new short[3];
							for (int i = 0; i < 3; i++)
							{
								data.triangleIndices[i] = b.ReadInt16();
							}
							skipBytes(2, b);
						}
					}
					else
					{
						if (classification.textureMapped)
						{
							// fig. 2-52 b
							data.uvCoords = new UV[3];
							for (int i = 0; i < 3; i++)
							{
								data.uvCoords[i] = readUV(b);
								skipBytes(2, b);
							}
							skipBytes(4, b);

							data.triangleIndices = new short[3];
							for (int i = 0; i < 3; i++)
							{
								data.triangleIndices[i] = b.ReadInt16();
							}
							skipBytes(2, b);
						}
						else
						{
							// fig. 2-52 a
							skipBytes(4, b);

							data.triangleIndices = new short[3];
							for (int i = 0; i < 3; i++)
							{
								data.triangleIndices[i] = b.ReadInt16();
							}
							skipBytes(2, b);
						}
					}
				}
				else
				{
					if (classification.gouraudShaded)
					{
						if (classification.textureMapped)
						{
							// fig. 2-48 f
							data.uvCoords = new UV[3];
							for (int i = 0; i < 3; i++)
							{
								data.uvCoords[i] = readUV(b);
								skipBytes(2, b);
							}

							data.normalIndices = new short[3];
							data.triangleIndices = new short[3];
							for (int i = 0; i < 3; i++)
							{
								data.normalIndices[i] = b.ReadInt16();
								data.triangleIndices[i] = b.ReadInt16();
							}
						}
						else
						{
							if (classification.gradation)
							{
								// fig. 2-48 e
								skipBytes(12, b);

								data.normalIndices = new short[3];
								data.triangleIndices = new short[3];
								for (int i = 0; i < 3; i++)
								{
									data.normalIndices[i] = b.ReadInt16();
									data.triangleIndices[i] = b.ReadInt16();
								}
							}
							else
							{
								// fig. 2-48 d
								skipBytes(4, b);

								data.normalIndices = new short[3];
								data.triangleIndices = new short[3];
								for (int i = 0; i < 3; i++)
								{
									data.normalIndices[i] = b.ReadInt16();
									data.triangleIndices[i] = b.ReadInt16();
								}
							}
						}
					}
					else
					{
						if (classification.textureMapped)
						{
							// fig. 2-48 c
							data.uvCoords = new UV[3];
							for (int i = 0; i < 3; i++)
							{
								data.uvCoords[i] = readUV(b);
								skipBytes(2, b);
							}

							data.normalIndices = new short[1];
							data.normalIndices[0] = b.ReadInt16();

							data.triangleIndices = new short[3];
							for (int i = 0; i < 3; i++)
							{
								data.triangleIndices[i] = b.ReadInt16();
							}
						}
						else
						{
							if (classification.gradation)
							{
								// fig. 2-48 b
								skipBytes(12, b);

								data.normalIndices = new short[1];
								data.normalIndices[0] = b.ReadInt16();

								data.triangleIndices = new short[3];
								for (int i = 0; i < 3; i++)
								{
									data.triangleIndices[i] = b.ReadInt16();
								}
							}
							else
							{
								// fig. 2-48 a
								skipBytes(4, b);

								data.normalIndices = new short[1];
								data.normalIndices[0] = b.ReadInt16();

								data.triangleIndices = new short[3];
								for (int i = 0; i < 3; i++)
								{
									data.triangleIndices[i] = b.ReadInt16();
								}
								
							}
                        }
					}
				}
			}
			return data;
		}

		private static UV readUV(BinaryReader b)
		{
			UV uv;
			uv.U = b.ReadByte();
			uv.V = b.ReadByte();
			return uv;

		}

		private static void skipBytes(long bytes, BinaryReader b)
		{
			b.BaseStream.Seek(bytes, SeekOrigin.Current);
		}

		private static bool intToBool(int input)
		{
			return input != 0 ? true : false;
		}
	}
}
