#pragma once

using namespace System;
using namespace System::Drawing::Imaging;
using namespace OpenCvSharp;
namespace CV = OpenCvSharp;

namespace Biscuit {

	public ref class xImageHelper {
	protected:
		FIBITMAP* m_fb = nullptr;
		uint32_t m_bpp{};
		uint32_t m_pitch{};
		uint32_t m_width{}, m_height{};
		uint32_t m_dotsPerMeterX{}, m_dotsPerMeterY{};

	public:
		~xImageHelper();

	public:
		bool LoadImage(String^ filename);
		void Close();

		UInt32 BPP() { return m_bpp; }
		UInt32 Width() { return m_width; }
		UInt32 Height() { return m_height; }
		UInt32 DotsPerMeterX() { return m_dotsPerMeterX; }
		UInt32 DotsPerMeterY() { return m_dotsPerMeterY; }

		array<Byte>^ GetColorIndexRow(int y);
		array<Byte>^ GetBitPlaneRow(int y);
		array<Byte>^ GetBitPlaneRowInverted(int y);
		CV::Mat^ GetImage(bool bBGRtoRGB);

		bool FlipXY(bool bHorz, bool bVert);
		bool Rotate(double angle_deg /* 0, 90, -90, 180, ... 90n */);

	public:
		static BitmapData^ GetBitmapData(CV::Mat^ mat);
	};

}
