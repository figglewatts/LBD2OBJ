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
				WriteToObj(filePath, ConvertTMD(filePath));
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
				short verticesRunningCount = 0;
				short normalsRunningCount = 0;
				int objCount = 0;
				for (int i = 0; i < tmd.header.numObjects; i++)
				{
					Console.WriteLine("Object {0}", objCount);
					tmd.objTable[i] = readObject(b, tmd.objTop);
					
					for (int j = 0; j < tmd.objTable[i].numPrims; j++)
					{
						tmd.objTable[i].primitives[j].data.triangleIndexOffset = verticesRunningCount;
						tmd.objTable[i].primitives[j].data.normalIndexOffset = normalsRunningCount;
					}
					verticesRunningCount += (short)tmd.objTable[i].numVerts;
					normalsRunningCount += (short)tmd.objTable[i].numNorms;
					objCount++;
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
				obj.primitives[p] = readPrimitive(b, obj.vertices);
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

		private static PRIMITIVE readPrimitive(BinaryReader b, VERTEX[] vertices)
		{
			PRIMITIVE p;
			p.olen = b.ReadByte();
			p.ilen = b.ReadByte();
			p.flag = b.ReadByte();
			p.mode = b.ReadByte();
			p.classification = readPrimitiveClassification(b, p.mode, p.flag);
			p.data = readPrimitiveData(b, p.classification, vertices);
			return p;
		}

		private static PRIMITIVECLASSIFICATION readPrimitiveClassification(BinaryReader b, int mode, int flags)
		{
			PRIMITIVECLASSIFICATION classification = new PRIMITIVECLASSIFICATION();
			// a bitmask of 11100000 to check if the mode is 001, i.e. a polygon
			if ((mode & 224) != 32) 
			{
				//throw new InvalidDataException("Unrecognized polygon mode, mode was: " + mode); 
				classification.skip = true;
			}
			mode >>= 2;
			classification.textureMapped = intToBool(mode & 1);
			mode >>= 1;
			classification.quad = intToBool(mode & 1);
			mode >>= 1;
			classification.gouraudShaded = intToBool(mode & 1);

			classification.unlit = intToBool(flags & 1);
			flags >>= 1;
			classification.twoSided = intToBool(flags & 1);
			flags >>= 1;
			classification.gradation = intToBool(flags & 1);
			
			return classification;

		}

		// this method, lol, hope you like nested if statements
		private static PRIMITIVEDATA readPrimitiveData(BinaryReader b, PRIMITIVECLASSIFICATION classification, VERTEX[] vertices)
		{
			PRIMITIVEDATA data = new PRIMITIVEDATA();
			if (classification.quad)
			{
				if (classification.unlit)
				{
					if (classification.gradation || classification.gouraudShaded)
					{
						if (classification.textureMapped)
						{
							// fig. 2-54 d
							Console.WriteLine("fig. 2-54 d");
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
							Console.WriteLine("fig. 2-54 c");
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
							Console.WriteLine("fig. 2-54 b");
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
							Console.WriteLine("fig. 2-54 a");
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
							Console.WriteLine("fig. 2-50 f");
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
								Console.WriteLine("fig. 2-50 e");
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
								Console.WriteLine("fig. 2-50 d");
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
							Console.WriteLine("fig. 2-50 c");
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
								Console.WriteLine("fig. 2-50 b");
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
								Console.WriteLine("fig. 2-50 a");
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



				// now sort them to the correct order
				//sortTriangleIndices(ref data.triangleIndices, vertices);
			}
			else
			{
				if (classification.unlit)
				{
					if (classification.gradation || classification.gouraudShaded)
					{
						if (classification.textureMapped)
						{
							// fig. 2-52 d
							Console.WriteLine("fig. 2-52 d");
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
							Console.WriteLine("fig. 2-52 c");
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
							Console.WriteLine("fig. 2-52 b");
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
							Console.WriteLine("fig. 2-52 a");
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
							Console.WriteLine("fig. 2-48 f");
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
								Console.WriteLine("fig. 2-48 e");
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
								Console.WriteLine("fig. 2-48 d");
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
							Console.WriteLine("fig. 2-48 c");
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
								Console.WriteLine("fig. 2-48 b");
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
								Console.WriteLine("fig. 2-48 a");
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
			for (int i = 0; i < (classification.quad ? 4 : 3); i++)
			{
				if (!classification.unlit)
				{
					if (classification.gouraudShaded) { data.normalIndices[i] += 1; }
					else { data.normalIndices[0] += 1; }
				}
				data.triangleIndices[i] += 1;
			}

			return data;
		}

		private static void sortTriangleIndices(ref short[] indices, VERTEX[] vertices)
		{
			// calculate center of poly
			VERTEX center = new VERTEX(0, 0, 0);
			foreach (short index in indices)
			{
				center += vertices[index];
				Console.WriteLine("{0}, {1}, {2}", vertices[index].X, vertices[index].Y, vertices[index].Z);
			}
			center /= 4;

			for (int i = 0; i < 2; i++)
			{
				VERTEX a = new VERTEX(0, 0, 0);
				float smallestAngle = -1;
				int smallest = -1;

				a = vertices[indices[i]] - center;
				a = VERTEX.Normalize(a);

				for (int j = i+1; j < 4; j++)
				{
					VERTEX b = new VERTEX(0, 0, 0);
					float angle;
					b = vertices[indices[j]] - center;
					b = VERTEX.Normalize(b);

					angle = VERTEX.Dot(a, b);

					if (angle > smallestAngle)
					{
						smallestAngle = angle;
						smallest = j;
					}
				}

				if (smallest == -1) { continue; }

				short t = indices[smallest];
				indices[smallest] = indices[i + 1];
				indices[i + 1] = t;
			}
			
			
			/*// calculate surface normal
			VERTEX normal = new VERTEX(0, 0, 0);
			normal = VERTEX.Cross(vertices[indices[0]], vertices[indices[1]]); // 0 and 1 are arbitrary, any differing index values will do

			for (int i = 0; i < 2; i++)
			{
				VERTEX v1 = vertices[indices[i]];

				float smallestTestVal = 0;
				int smallestIndex = -1;

				for (int j = i + 1; j < 4; j++)
				{
					VERTEX v2 = vertices[indices[j]];
					float testVal = VERTEX.Dot(normal, VERTEX.Cross(v1 - center, v2 - center));

					if (testVal < smallestTestVal)
					{
						smallestTestVal = testVal;
						smallestIndex = j;
					}
				}

				if (smallestIndex == -1) { break; }

				short t = indices[smallestIndex];
				indices[smallestIndex] = indices[i + 1];
				indices[i + 1] = t;
				
			}*/
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

		public static void WriteToObj(string path, TMD tmd)
		{
			using (StreamWriter w = new StreamWriter(Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + ".obj", false))
			{
				w.WriteLine("# generated with LBD2OBJ");
				w.WriteLine("# made by Figglewatts, 2015");
				w.WriteLine("# check out www.lsdrevamped.net");
				w.WriteLine("# </shamelessSelfPromotion>");
			
				List<VERTEX> vertexList = new List<VERTEX>();
				List<NORMAL> normalList = new List<NORMAL>();
				List<UV> uvList = new List<UV>();
				Dictionary<PRIMITIVE, List<int>> uvIndices = new Dictionary<PRIMITIVE, List<int>>();
			
				foreach (OBJECT o in tmd.objTable)
				{
					foreach (VERTEX v in o.vertices)
					{
						vertexList.Add(v);
					}
					foreach (NORMAL n in o.normals)
					{
						normalList.Add(n);
					}

					int uvIndex = 1;
					foreach (PRIMITIVE p in o.primitives)
					{
						if (!p.classification.textureMapped) { continue; }

						uvIndices.Add(p, new List<int>());
						foreach (UV uv in p.data.uvCoords)
						{
							uvList.Add(uv);
							uvIndices[p].Add(uvIndex);
                            uvIndex++;
						}
					}
				}

				foreach (VERTEX v in vertexList)
				{
					writeVertex(v, w);
				}
				foreach (UV uv in uvList)
				{
					writeUV(uv, w);
				}
				foreach (NORMAL n in normalList)
				{
					writeNormal(n, w);
				}

				int objCount = 0;
				foreach (OBJECT o in tmd.objTable)
				{
					writeObject(w, objCount);
					foreach (PRIMITIVE p in o.primitives)
					{
						if (p.classification.textureMapped)
						{
							writeFace(p, w, uvIndices[p]);
						}
						else
						{
							writeFace(p, w);
						}
					}
					objCount++;
				}

			}
		}
		
		private static void writeVertex(VERTEX v, StreamWriter w)
		{
			w.WriteLine("v " + v.X + " " + v.Y + " " + v.Z);
		}

		private static void writeUV(UV uv, StreamWriter w)
		{
			float[] convertedUV = new float[2];
			convertedUV[0] = (float)uv.U / 256F;
			convertedUV[1] = (float)uv.V / 256F;
			w.WriteLine("vt " + convertedUV[0] + " " + convertedUV[1]);
		}

		private static void writeNormal(NORMAL n, StreamWriter w)
		{
			w.WriteLine("vn " + n.nX.ToString() + " " + n.nY.ToString() + " " + n.nZ.ToString());
		}

		private static void writeObject(StreamWriter w, int objNumber)
		{
			w.WriteLine("o Object " + objNumber);
		}

		private static void writeFace(PRIMITIVE p, StreamWriter w, List<int> uvIndices=null)
		{
			int numVerts = p.classification.quad ? 4 : 3;

			if (p.classification.unlit)
			{
				// has no normals
				if (p.classification.textureMapped)
				{
					// has UVs
					actuallyWriteFace(p.data, p.classification, w, numVerts, false, true, false, uvIndices);
				}
				else
				{
					// has no UVs
					actuallyWriteFace(p.data, p.classification, w, numVerts);
				}
			}
			else
			{
				// has at least 1 normal
				if (p.classification.gouraudShaded)
				{
					// has normals for each vert
					if (p.classification.textureMapped)
					{
						// has UVs
						actuallyWriteFace(p.data, p.classification, w, numVerts, true, true, true, uvIndices);
					}
					else
					{
						// has no UVs
						actuallyWriteFace(p.data, p.classification, w, numVerts, true, false, true);
					}
				}
				else
				{
					// only has 1 normal
					if (p.classification.textureMapped)
					{
						// has UVs
						actuallyWriteFace(p.data, p.classification, w, numVerts, true, true, false, uvIndices);
					}
					else
					{
						// has no UVs
						actuallyWriteFace(p.data, p.classification, w, numVerts, true, false, false);
					}
				}
			}
		}

		private static void actuallyWriteFace(PRIMITIVEDATA data, PRIMITIVECLASSIFICATION classification, StreamWriter w, int numVerts, bool normals=false, bool UVs=false, bool gouraud=false, List<int> uvIndex=null)
		{
			if (classification.quad)
			{
				// write as 2 tris
				w.Write("f ");
				writeFacePart(data, 0, normals, UVs, gouraud, uvIndex, w);
				writeFacePart(data, 2, normals, UVs, gouraud, uvIndex, w);
				writeFacePart(data, 3, normals, UVs, gouraud, uvIndex, w);
				writeFacePart(data, 1, normals, UVs, gouraud, uvIndex, w);
				w.Write("\n");
			}
			else
			{
				w.Write("f ");
				for (int i = 0; i < 3; i++)
				{
					writeFacePart(data, i, normals, UVs, gouraud, uvIndex, w);
				}
				w.Write("\n");
			}
		}

		private static void writeFacePart(PRIMITIVEDATA data, int i, bool normals, bool UVs, bool gouraud, List<int> uvIndex, StreamWriter w)
		{
			
			w.Write((data.triangleIndices[i] + data.triangleIndexOffset));
			if (!gouraud)
			{
				if (!normals && UVs)
				{
					w.Write("/" + uvIndex[i]);
				}
				else if (normals && !UVs)
				{
					w.Write("//" + (data.normalIndices[0] + data.normalIndexOffset));
				}
				else if (normals && UVs)
				{
					w.Write("/" + uvIndex[i] + "/" + (data.normalIndices[0] + data.normalIndexOffset));
				}
			}
			else
			{
				if (!normals && UVs)
				{
					w.Write("/" + uvIndex[i]);
				}
				else if (normals && !UVs)
				{
					w.Write("//" + (data.normalIndices[i] + data.normalIndexOffset));
				}
				else if (normals && UVs)
				{
					w.Write("/" + uvIndex[i] + "/" + (data.normalIndices[i] + data.normalIndexOffset));
				}
			}
			w.Write(" ");
		}
	}
}
