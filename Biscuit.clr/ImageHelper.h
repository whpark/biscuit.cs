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
		bool IsValid() { return m_fb != nullptr; }
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
		CV::Mat^ GetIndexImage();	// Get Pre-Palette Image
		CV::Mat^ GetPalette();		// Get Palette Image (1x256x3)
		bool IsPaletteImage();

		bool FlipXY(bool bHorz, bool bVert);
		bool Rotate(double angle_deg /* 0, 90, -90, 180, ... 90n */);

	public:
		static BitmapData^ GetBitmapData(CV::Mat^ mat);
	};

}
