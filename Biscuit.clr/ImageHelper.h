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
		xImageHelper() {}
		xImageHelper(FIBITMAP* fbToBeMoved);
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

		xImageHelper^ GetRotated(double angle_deg /* 0, 90, -90, 180, ... 90n */);

	public:
		static BitmapData^ GetBitmapData(CV::Mat^ mat);

		static bool PackValue(CV::Mat^ mat, uint32_t row0, uint32_t row1, int32_t bpp, array<int>^ buffer, int^ pos) {
			//// make sure, bpp is one of 1, 2, 4
			//if (bpp != 1 && bpp != 2 && bpp != 4)
			//	return;
			if (mat->Type() != CV::MatType::CV_8UC1)
				return false;

			const uint8_t shift0 = 32;
			uint32_t mask = (1u << bpp) - 1;

			for (uint32_t y = row0; y < row1; y++) {
				int shift = shift0 - bpp;
				IntPtr ptr = mat->Ptr(y);
				for (int x = 0; x < mat->Cols; x++, shift -= bpp) {
					uint32_t v = ((uint8_t*)ptr.ToPointer())[x];
					buffer[*pos] |= (int)((v & mask) << (int)shift);
					if (shift == 0) {
						(*pos)++;
						shift = shift0;
					}
				}
				if (shift != shift0-bpp)
					(*pos)++;
			}

			return true;
		}

	};

}
