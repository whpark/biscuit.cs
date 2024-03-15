#include "pch.h"
#include "ImageHelper.h"
#include <gsl/gsl>

using namespace System;
using namespace OpenCvSharp;
namespace CV = OpenCvSharp;

namespace Biscuit {

	xImageHelper::~xImageHelper() {
		Close();
	}

	void xImageHelper::Close() {
		if (m_fb) {
			auto* fb = m_fb;
			m_fb = nullptr;	// std::exchange does NOT compile. ???
			FreeImage_Unload(fb);
		}
		m_bpp = 0;
		m_pitch = 0;
		m_width = 0;
		m_height = 0;
		m_dotsPerMeterX = 0;
		m_dotsPerMeterY = 0;
	}

	bool xImageHelper::LoadImage(String^ filename) {
		Close();

		pin_ptr<const wchar_t> wch = PtrToStringChars(filename);

		std::filesystem::path path(wch);
		std::error_code ec;
		if (!std::filesystem::is_regular_file(path, ec))
			return false;
		size_t sizeFile = std::filesystem::file_size(path, ec);
		if (!sizeFile)
			return false;

		filename->LastIndexOf('.');
		auto strExt = path.extension().string();
		auto eFileType = FreeImage_GetFIFFromFilename(strExt.c_str());
		if (eFileType == FIF_UNKNOWN)
			return false;

		auto* fb = FreeImage_LoadU(eFileType, wch, 0);
		if (!fb)
			return false;
		m_bpp = FreeImage_GetBPP(fb);
		m_pitch = FreeImage_GetPitch(fb);
		m_width = FreeImage_GetWidth(fb);
		m_height = FreeImage_GetHeight(fb);
		m_dotsPerMeterX = FreeImage_GetDotsPerMeterX(fb);
		m_dotsPerMeterY = FreeImage_GetDotsPerMeterY(fb);

		m_fb = fb;

		return true;
	}

	array<Byte>^ xImageHelper::GetColorIndexRow(int y) {
		if (!m_fb)
			return nullptr;

		y = m_height - 1 - y;	// flip y-axis
		auto* bits = FreeImage_GetBits(m_fb);
		RGBQUAD* palette = FreeImage_GetPalette(m_fb);

		array<Byte>^ row = nullptr;

		switch (m_bpp) {
		case 1:
			{
				row = gcnew array<Byte>(m_pitch * 8);
				uint64_t offset = y * m_pitch;
				for (auto i{0u}; i < m_pitch; ++i) {
					int x0 = i << 3;	// == i*8
					// loop unrolling
					auto v = bits[offset + i];
					row[x0 + 0] = (v & 0b1000'0000) ? 1 : 0;
					row[x0 + 1] = (v & 0b0100'0000) ? 1 : 0;
					row[x0 + 2] = (v & 0b0010'0000) ? 1 : 0;
					row[x0 + 3] = (v & 0b0001'0000) ? 1 : 0;
					row[x0 + 4] = (v & 0b0000'1000) ? 1 : 0;
					row[x0 + 5] = (v & 0b0000'0100) ? 1 : 0;
					row[x0 + 6] = (v & 0b0000'0010) ? 1 : 0;
					row[x0 + 7] = (v & 0b0000'0001) ? 1 : 0;
				}
			}
			break;

		case 2:
			{
				row = gcnew array<Byte>(m_pitch * 4);
				uint64_t offset = y * m_pitch;
				for (auto i{0u}; i < m_pitch; ++i) {
					int x0 = i << 2;	// == i*4
					// loop unrolling
					uint8_t v = bits[offset + i];
					row[x0 + 0] = (v >> 6) & 0b0011;
					row[x0 + 1] = (v >> 4) & 0b0011;
					row[x0 + 2] = (v >> 2) & 0b0011;
					row[x0 + 3] = (v >> 0) & 0b0011;
				}
			}
			break;

		case 4:
			{
				row = gcnew array<Byte>(m_pitch * 2);
				uint64_t offset = y * m_pitch;
				for (auto i{0u}; i < m_pitch; ++i) {
					int x0 = i << 1;
					uint8_t v = bits[offset + i];
					row[x0 + 0] = (v >> 4) & 0b1111;
					row[x0 + 1] = (v >> 0) & 0b1111;
				}
			}
			break;

		case 8:
			{
				row = gcnew array<Byte>(m_pitch);
				uint64_t offset = y * m_pitch;
				for (auto i{0u}; i < m_pitch; ++i) {
					row[i] = bits[offset + i];
				}
			}
			break;

		default:
			break;	// error case
		}

		return row;
	}

	array<Byte>^ xImageHelper::GetBitPlaneRow(int y) {
		if (!m_fb)
			return nullptr;

		y = m_height - 1 - y;	// flip y-axis
		auto* bits = FreeImage_GetBits(m_fb);

		array<Byte>^ row = gcnew array<Byte>(m_pitch);
		uint64_t offset = y * m_pitch;
		auto pos = bits + offset;
		for (auto i{0u}; i < m_pitch; ++i) {
			row[i] = *pos++;
		}

		return row;
	}
	array<Byte>^ xImageHelper::GetBitPlaneRowInverted(int y) {
		auto row = GetBitPlaneRow(y);
		for (int i{}; i < row->Length; ++i) {
			row[i] = ~row[i];
		}
		return row;
	}

	CV::Mat^ xImageHelper::GetImage(bool bRGBtoBGR) {
		if (!m_fb)
			return nullptr;
		auto* src = m_fb;

		CV::MatType type;
		int bpp{-1};
		auto eImageType = FreeImage_GetImageType(src);
		switch (eImageType) {
		case FIT_UINT16:	type = CV::MatType::CV_16UC1; break;
		case FIT_INT16:		type = CV::MatType::CV_16SC1; break;
		//case FIT_UINT32:	type = CV::MatType::CV_32SC1; break;
		case FIT_INT32:		type = CV::MatType::CV_32SC1; break;
		case FIT_FLOAT:		type = CV::MatType::CV_32FC1; break;
		case FIT_DOUBLE:	type = CV::MatType::CV_64FC1; break;
		case FIT_COMPLEX:	type = CV::MatType::CV_64FC2; break;
		case FIT_RGB16:		type = CV::MatType::CV_16UC3; break;
		case FIT_RGBA16:	type = CV::MatType::CV_16UC4; break;
		case FIT_RGBF:		type = CV::MatType::CV_32FC3; break;
		case FIT_RGBAF:		type = CV::MatType::CV_32FC4; break;
		case FIT_BITMAP:
			bpp = FreeImage_GetBPP(src);
			switch (bpp) {
			case 1:
			case 2:	// probably there might not be 2bpp image
			case 4:
			case 8:  type = CV::MatType::CV_8UC1; break;	// To Be Determined
			case 16: type = CV::MatType::CV_16UC1; break;	// To Be Determined
			case 24: type = CV::MatType::CV_8UC3; break;
			case 32: type = CV::MatType::CV_8UC4; break;
			}
			break;
		}
		if (type < 0)
			return {};

		//auto* info = FreeImage_GetInfo(src);
		//info->bmiHeader;

		FIBITMAP* converted{};
		bool flip{true};
		FIBITMAP* fib = src;
		if (eImageType == FIT_BITMAP) {	// Standard image type
			// first check if grayscale image
			if (bpp <= 16) {
				bool bColor{};
				bool bAlpha{};
				// get palette
				if (auto* palette = FreeImage_GetPalette(src)) {
					for (auto nPalette = FreeImage_GetColorsUsed(src), i{ 0u }; i < nPalette; i++) {
						if (auto c = palette[i]; c.rgbBlue != c.rgbGreen or c.rgbGreen != c.rgbRed) {	// todo: alpha channel?
							bColor = true;
							break;
						}
					}
					for (auto nPalette = FreeImage_GetColorsUsed(src), i{ 0u }; i < nPalette; i++) {
						if (auto c = palette[i]; c.rgbReserved) {	// todo: alpha channel?
							bAlpha = true;
							break;
						}
					}
				}
				if (bAlpha) {
					converted = FreeImage_ConvertTo32Bits(src);
					type = CV::MatType::CV_8UC4;
				} else if (bColor or (bpp == 16)) {
					converted = FreeImage_ConvertTo24Bits(src);
					type = CV::MatType::CV_8UC3;
				} else {
					converted = FreeImage_ConvertToGreyscale(src);
					type = CV::MatType::CV_8UC1;
				}
				fib = converted;
			}
		}
		else {
			flip = true;	// don't know if image needs be flipped. so just flip it. :)
		}

		CV::Mat^ img{};
		try {
			if (!fib)
				return {};

			BYTE* data = FreeImage_GetBits(fib);
			auto step = FreeImage_GetPitch(fib);
			CV::Size^ size = gcnew CV::Size((int)FreeImage_GetWidth(fib), (int)FreeImage_GetHeight(fib));
			if (!data or !step or size->Width <= 0 or size->Height <= 0)
				return {};

			img = gcnew CV::Mat(size->Height, size->Width, type, (IntPtr)data, (long long)step);
			
			if (flip)
				img = img->Flip(CV::FlipMode::X);
			else
				img = img;

			if (bRGBtoBGR) {
				CV::ColorConversionCodes cvt{};
				switch (img->Channels()) {
				case 3: cvt = CV::ColorConversionCodes::RGB2BGR; break;
				case 4: cvt = CV::ColorConversionCodes::RGBA2BGRA; break;
				}
				if (cvt != CV::ColorConversionCodes{})
					img = img->CvtColor(cvt, 0);
			}

		}
		finally {
			if (converted)
				FreeImage_Unload(converted);
		}

		return img;

	}



}
