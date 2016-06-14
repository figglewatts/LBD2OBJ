using System;

namespace LBD2OBJLib.Types
{
	public class FixedPoint
	{
		public int IntegralPart { get; set; }
		public int DecimalPart { get; set; }

		const byte SIGN_MASK = 128;
		const byte INTEGRAL_MASK = 112;
		const byte MANTISSA_MASK = 15;

		public FixedPoint(byte[] data)
		{
			if (data.Length != 2) { throw new ArgumentException("data must be 2 bytes", "data"); }

			byte[] _data = new byte[2];
			data.CopyTo(_data, 0);

			bool isNegative = (_data[0] & SIGN_MASK) == 128;
			int integralPart = (_data[0] & INTEGRAL_MASK) * (isNegative ? -1 : 1);
			int decimalPart = (_data[0] & MANTISSA_MASK);
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
