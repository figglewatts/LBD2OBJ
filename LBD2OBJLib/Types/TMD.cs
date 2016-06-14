using System;

namespace LBD2OBJLib.Types
{
	public struct TMDHEADER
	{
		public int ID; // should be 0x41
		public uint flags; // 0 if addresses are relative from top of block, 1 if from start of file
		public uint numObjects;
	}

	public struct OBJECT
	{
		public uint vertTop; // pointer to start of vertices
		public uint numVerts;
		public uint normTop; // pointer to start of normals
		public uint numNorms;
		public uint primTop; // pointer to start of primitives
		public uint numPrims;
		public int scale; // unused
		public VERTEX[] vertices;
		public NORMAL[] normals;
		public PRIMITIVE[] primitives;
	}

	public struct PRIMITIVE
	{
		public byte mode; // b001 - poly, b010 - line, b011 - sprite
		public byte flag; // rendering info
		public byte ilen; // length (in words) of packet data
		public byte olen; // length (in words) of 2D drawing primitives
		public PRIMITIVECLASSIFICATION classification;
		public PRIMITIVEDATA data;
	}

	public struct PRIMITIVECLASSIFICATION
	{
		public bool skip;
		public bool gouraudShaded;
		public bool quad;
		public bool textureMapped;
		public bool unlit; // if unlit, there are no normals
		public bool gradation;
		public bool twoSided;
	}

	public struct PRIMITIVEDATA
	{
		public short triangleIndexOffset;
		public short[] triangleIndices;
		public short normalIndexOffset;
		public short[] normalIndices;
		public short uvIndexOffset;
		public UV[] uvCoords;
	}

	public struct VERTEX
	{
		public float X;
		public float Y;
		public float Z;

		public VERTEX(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static VERTEX operator + (VERTEX a, VERTEX b)
		{
			VERTEX returnVal = new VERTEX();
			returnVal.X = (a.X + b.X);
			returnVal.Y = (a.Y + b.Y);
			returnVal.Z = (a.Z + b.Z);
			return returnVal;
		}
		public static VERTEX operator - (VERTEX a)
		{
			VERTEX returnVal = new VERTEX();
			returnVal.X = -a.X;
			returnVal.Y = -a.Y;
			returnVal.Z = -a.Z;
			return returnVal;
		}
		public static VERTEX operator - (VERTEX a, VERTEX b)
		{
			return a + -b;
		}
		public static VERTEX operator * (VERTEX a, float s)
		{
			VERTEX returnVal = new VERTEX();
			returnVal.X = (a.X * s);
			returnVal.Y = (a.Y * s);
			returnVal.Z = (a.Z * s);
			return returnVal;
		}
		public static VERTEX operator / (VERTEX a, float s)
		{
			VERTEX returnVal = new VERTEX();
			returnVal.X = (a.X / s);
			returnVal.Y = (a.Y / s);
			returnVal.Z = (a.Z / s);
			return returnVal;
		}

		public static float Dot (VERTEX a, VERTEX b)
		{
			Console.WriteLine("Getting dot product of:");
			Console.WriteLine("{0}, {1}, {2}", a.X, a.Y, a.Z);
			Console.WriteLine("{0}, {1}, {2}", b.X, b.Y, b.Z);
			return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
		}
		public static VERTEX Cross (VERTEX a, VERTEX b)
		{
			VERTEX returnVal = new VERTEX();
			returnVal.X = a.Y * b.Z - a.Z * b.Y;
			returnVal.Y = a.Z * b.X - a.X * b.Z;
			returnVal.Z = a.X * b.Y - a.Y * b.X;
			return returnVal;
		}
		public static float Magnitude(VERTEX a)
		{
			return (float)Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
		}
		public static VERTEX Normalize(VERTEX a)
		{
			return a / Magnitude(a);
		}
	}

	public struct NORMAL
	{
		public FixedPoint nX;
		public FixedPoint nY;
		public FixedPoint nZ;
	}

	public struct UV
	{
		public byte U;
		public byte V;
	}

	public struct TMD
	{
		public TMDHEADER header;
		public bool fixP;
		public long objTop;
		public OBJECT[] objTable;
	}
}
