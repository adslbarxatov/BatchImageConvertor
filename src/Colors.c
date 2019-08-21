// Функции перевода цветов в схему RGB с огрублением и округлением
#include "BatchImageConvertor.h"

// RGB
union RGB_Color RGBtoRGB (uint R, uint G, uint B)
	{
	union RGB_Color color;
	color.RGB.R = (R >> 8);
	color.RGB.G = (G >> 8);
	color.RGB.B = (B >> 8);
	return color;
	}

union RGB_Color RGBtoRGB_f (float R, float G, float B)
	{
	union RGB_Color color;
	color.RGB.R = ((R * 255.5f) > 255.9f) ? 255 : (uchar)(R * 255.5f);
	color.RGB.G = ((G * 255.5f) > 255.9f) ? 255 : (uchar)(G * 255.5f);
	color.RGB.B = ((B * 255.5f) > 255.9f) ? 255 : (uchar)(B * 255.5f);
	return color;
	}

// Greyscale
union RGB_Color GreytoRGB (uint V)
	{
	union RGB_Color color;
	double Vf = 255 * (double)((10000 - V) % 10001) / 10000.0;
	color.RGB.R = color.RGB.G = color.RGB.B = (uchar)(Vf + 0.5);
	return color;
	}

union RGB_Color GreytoRGB_f (float V)
	{
	union RGB_Color color;
	float Vf = ((255.5f * V) > 255.9f) ? 255.0f : (255.5f * V);
	color.RGB.R = color.RGB.G = color.RGB.B = (uchar)Vf;
	return color;
	}

// HSB
union RGB_Color HSBtoRGB (uint H, uint S, uint B)
	{
	// wikipedia.org/hsv

	// Общие переменные
	union RGB_Color color;
	double Hf = 360.0 * (double)H / (double)BIC_ICL;	// Перегонка в реальное HSB
	double Sf = 100.0 * (double)S / (double)BIC_ICL;
	double Vf = 100.0 * (double)B / (double)BIC_ICL;

	// Вспомогательные переменные
	uchar Hi = (uchar)(Hf / 60.0) % 6;
	double Vmin = (100.0 - Sf) * Vf / 100.0;
	double a = ((Vf - Vmin) * (double)((uint)Hf % 60)) / 60.0;
	double Vinc = Vmin + a;
	double Vdec = Vf - a;

	// Приведённые переменные
	uchar Bi = (uchar)(255.0 * Vf / 100.0);
	uchar Bmin = (uchar)(255.0 * Vmin / 100.0);
	uchar Binc = (uchar)(255.0 * Vinc / 100.0);
	uchar Bdec = (uchar)(255.0 * Vdec / 100.0);

	// Формирование цвета
	switch (Hi)
		{
		case 0:
			color.RGB.R = Bi;
			color.RGB.G = Binc;
			color.RGB.B = Bmin;
			break;

		case 1:
			color.RGB.R = Bdec;
			color.RGB.G = Bi;
			color.RGB.B = Bmin;
			break;

		case 2:
			color.RGB.R = Bmin;
			color.RGB.G = Bi;
			color.RGB.B = Binc;
			break;

		case 3:
			color.RGB.R = Bmin;
			color.RGB.G = Bdec;
			color.RGB.B = Bi;
			break;

		case 4:
			color.RGB.R = Binc;
			color.RGB.G = Bmin;
			color.RGB.B = Bi;
			break;

		case 5:
			color.RGB.R = Bi;
			color.RGB.G = Bmin;
			color.RGB.B = Bdec;
			break;
		}

	// Завершено
	return color;
	}
