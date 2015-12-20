using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBD2OBJ.Types
{
	struct TMDHEADER
	{
		public int ID; // should be 0x41
		public uint flags; // 0 if addresses are relative from top of block, 1 if from start of file
		public uint numObjects;
	}

	struct OBJECT
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

	struct PRIMITIVE
	{
		public byte mode; // b001 - poly, b010 - line, b011 - sprite
		public byte flag; // rendering info
		public byte ilen; // length (in words) of packet data
		public byte olen; // length (in words) of 2D drawing primitives
		public PRIMITIVECLASSIFICATION classification;
		public PRIMITIVEDATA data;
	}

	struct PRIMITIVECLASSIFICATION
	{
		public bool gouraudShaded;
		public bool quad;
		public bool textureMapped;
		public bool unlit; // if unlit, there are no normals
		public bool gradation;
		public bool twoSided;
	}

	struct PRIMITIVEDATA
	{
		public short[] triangleIndices;
		public short[] normalIndices;
		public UV[] uvCoords;
	}

	struct VERTEX
	{
		public short X;
		public short Y;
		public short Z;
	}

	struct NORMAL
	{
		public FixedPoint nX;
		public FixedPoint nY;
		public FixedPoint nZ;
	}

	struct UV
	{
		public byte U;
		public byte V;
	}

	struct TMD
	{
		public TMDHEADER header;
		public bool fixP;
		public long objTop;
		public OBJECT[] objTable;
	}
}
