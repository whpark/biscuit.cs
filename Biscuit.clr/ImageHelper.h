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
		enum class eFREE_IMAGE_FORMAT : int32_t {
			FIF_UNKNOWN = -1,
			FIF_BMP		= 0,  FIF_ICO		= 1,  FIF_JPEG	= 2,		FIF_JNG		= 3,
			FIF_KOALA	= 4,  FIF_LBM		= 5,  FIF_IFF = FIF_LBM,	FIF_MNG		= 6,
			FIF_PBM		= 7,  FIF_PBMRAW	= 8,  FIF_PCD		= 9,	FIF_PCX		= 10,
			FIF_PGM		= 11, FIF_PGMRAW	= 12, FIF_PNG		= 13,	FIF_PPM		= 14,
			FIF_PPMRAW	= 15, FIF_RAS		= 16, FIF_TARGA		= 17,	FIF_TIFF	= 18,
			FIF_WBMP	= 19, FIF_PSD		= 20, FIF_CUT		= 21,	FIF_XBM		= 22,
			FIF_XPM		= 23, FIF_DDS		= 24, FIF_GIF		= 25,	FIF_HDR		= 26,
			FIF_SGI		= 27, FIF_EXR		= 28, FIF_J2K		= 29,	FIF_JP2		= 30,
			FIF_PFM		= 31, FIF_PICT		= 32, FIF_RAW		= 33,	FIF_WEBP	= 34,
			FIF_JXR		= 35
		};

		static eFREE_IMAGE_FORMAT GetImageFileType(String^ filename);
		static bool IsImageFile(String^ filename);

		static BitmapData^ GetBitmapData(CV::Mat^ mat);

		static bool PackValue(CV::Mat^ mat, uint32_t row0, uint32_t row1, int32_t bpp, array<int>^ buffer, int^ pos);

	};

}
