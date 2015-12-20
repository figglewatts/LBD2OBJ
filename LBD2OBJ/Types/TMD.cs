using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBD2OBJ.Types
{
	struct TMDHEADER
	{
		uint ID; // should be 0x41
		uint flags; // 0 if addresses are relative from top of block, 1 if from start of file
		uint numObjects;
	}

	struct OBJECT
	{
		uint vertTop; // pointer to start of vertices
		uint numVerts;
		uint normTop; // pointer to start of normals
		uint numNorms;
		uint primTop; // pointer to start of primitives
		uint numPrims;
		int scale; // unused
	}

	struct PRIMITIVE
	{
		byte mode; // b001 - poly, b010 - line, b011 - sprite
		byte flag;
		byte ilen;
		byte olen;
	}

	struct VERTEX
	{
		short X;
		short Y;
		short Z;
	}

	struct TMD
	{
		TMDHEADER header;
		OBJECT[] objTable;
	}
}
