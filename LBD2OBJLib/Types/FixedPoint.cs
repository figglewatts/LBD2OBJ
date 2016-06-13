using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBD2OBJLib.Types
{
	class FixedPoint
	{
		public int IntegralPart { get; set; }
		public int DecimalPart { get; set; }

		public FixedPoint(byte[] data)
		{
			if (data.Length != 2) { throw new ArgumentException("data must be 2 bytes", "data"); }

			byte[] _data = new byte[2];
			data.CopyTo(_data, 0);

			var signMask = (byte)128;
			var integralMask = (byte)112;
			var firstPartOfDecimalMask = (byte)15;

			bool isNegative = (_data[0] & signMask) == 128;
			int integralPart = (_data[0] & integralMask) * (isNegative ? -1 : 1);
			int decimalPart = (_data[0] & firstPartOfDecimalMask);
			decimalPart <<= 8;
			decimalPart += data[1];

			IntegralPart = integralPart;
			DecimalPart = decimalPart;
		}

		public override string ToString()
		{
			return IntegralPart + "." + DecimalPart;
		}
	}
}
