/*
img_tga.c - tga format load & save
Copyright (C) 2007 Uncle Mike

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
*/

/*
Reviewed by RD_AAOW
*/

#include "BatchImageConvertor.h"
#include "TGA.h"

#define ITGA_EXIT(c)		free (buffer); return c;

// Image_LoadTGA
sint Image_LoadTGA (schar *FileName, struct rgbdata_t *Img)
	{
	// Переменные
	slong	i,
			columns, rows,
			row_inc,
			row, col;
	uchar	*buf_p,
			*buffer,
			*pixbuf,
			*targa_rgba;
	uchar	palette[256][4],
			red = 0, green = 0, blue = 0, alpha = 0;
	slong	readpixelcount, pixelcount;
	uchar	compressed;
	struct tga_t	targa_header;

	ulong	filesize;
	FILE	*FS;

	// Контроль
	if (!Img || !FileName)
		{
		return EXEC_INVALID_PARAMETERS;
		}

	// Открытие файла и получение его размера
	BIC_INIT_FILE_READING

	fseek (FS, 0, SEEK_END);
	filesize = ftell (FS);
	fseek (FS, 0, SEEK_SET);

	// Контроль размера
	if (filesize < sizeof (struct tga_t))
		{
		fclose (FS);
		return EXEC_INVALID_FILE;
		}

	// Выделение памяти
	if ((buf_p = buffer = (uchar *)malloc (filesize)) == NULL)
		{
		fclose (FS);
		return EXEC_MEMORY_ALLOC_FAIL;
		}

	// Считывание и закрытие файла
	if (fread (buf_p, 1, filesize, FS) != filesize)
		{
		fclose (FS);
		ITGA_EXIT (EXEC_INVALID_FILE)
		}
	fclose (FS);

	// Заполнение структуры
	targa_header.id_length = *buf_p++;
	targa_header.colormap_type = *buf_p++;
	targa_header.image_type = *buf_p++;

	targa_header.colormap_index = buf_p[0] + buf_p[1] * 256;
	buf_p += 2;
	targa_header.colormap_length = buf_p[0] + buf_p[1] * 256;
	buf_p += 2;
	targa_header.colormap_size = *buf_p;
	buf_p += 1;
	targa_header.x_origin = *(uint *)buf_p;
	buf_p += 2;
	targa_header.y_origin = *(uint *)buf_p;
	buf_p += 2;
	targa_header.width = Img->width = *(uint *)buf_p;
	buf_p += 2;
	targa_header.height = Img->height = *(uint *)buf_p;
	buf_p += 2;
	targa_header.pixel_size = *buf_p++;
	targa_header.attributes = *buf_p++;
	if (targa_header.id_length != 0)
		{
		buf_p += targa_header.id_length;	// Skip TARGA image comment
		}
	Img->flags = 0;

	// Контроль
	if ((Img->width > IMAGE_MAXWIDTH) || (Img->height > IMAGE_MAXHEIGHT) ||
		(Img->width * Img->height == 0))
		{
		ITGA_EXIT (EXEC_INVALID_FILE)
		}

	// Разбор
	Img->type = PF_RGBA_32;	// Always exctracted to 32-bit buffer

	if ((targa_header.image_type == 1) || (targa_header.image_type == 9))
		{
		// Uncompressed colormapped image
		if (targa_header.pixel_size != 8)
			{
			// Only 8 bit images supported for type 1 and 9
			ITGA_EXIT (EXEC_INVALID_FILE)
			}
		if (targa_header.colormap_length != 256)
			{
			// Only 8 bit colormaps are supported for type 1 and 9
			ITGA_EXIT (EXEC_INVALID_FILE)
			}
		if (targa_header.colormap_index)
			{
			// colormap_index is not supported for type 1 and 9
			ITGA_EXIT (EXEC_INVALID_FILE)
			}
		if (targa_header.colormap_size == 24)
			{
			for (i = 0; i < targa_header.colormap_length; i++)
				{
				palette[i][2] = *buf_p++;
				palette[i][1] = *buf_p++;
				palette[i][0] = *buf_p++;
				palette[i][3] = 255;
				}
			}
		else if (targa_header.colormap_size == 32)
			{
			for (i = 0; i < targa_header.colormap_length; i++)
				{
				palette[i][2] = *buf_p++;
				palette[i][1] = *buf_p++;
				palette[i][0] = *buf_p++;
				palette[i][3] = *buf_p++;
				}
			}
		else
			{
			// Only 24 and 32 bit colormaps are supported for type 1 and 9
			ITGA_EXIT (EXEC_INVALID_FILE)
			}
		}
	else if ((targa_header.image_type == 2) || (targa_header.image_type == 10))
		{
		// Uncompressed or RLE compressed RGB
		if ((targa_header.pixel_size != 32) && (targa_header.pixel_size != 24))
			{
			// Only 32 or 24 bit images supported for type 2 and 10
			ITGA_EXIT (EXEC_INVALID_FILE)
			}
		}
	else if ((targa_header.image_type == 3) || (targa_header.image_type == 11))
		{
		// Uncompressed greyscale
		if (targa_header.pixel_size != 8)
			{
			// Only 8 bit images supported for type 3 and 11
			ITGA_EXIT (EXEC_INVALID_FILE)
			}
		}

	columns = targa_header.width;
	rows = targa_header.height;

	targa_rgba = Img->buffer = (uchar *)malloc (Img->width * Img->height * 4);
	if (Img->buffer == NULL)
		{
		ITGA_EXIT (EXEC_MEMORY_ALLOC_FAIL)
		}

	// If bit 5 of attributes isn't set, the image has been stored from bottom to top
	if (targa_header.attributes & 0x20)
		{
		pixbuf = targa_rgba;
		row_inc = 0;
		}
	else
		{
		pixbuf = targa_rgba + (rows - 1) * columns * 4;
		row_inc = -columns * 4 * 2;
		}

	compressed = ((targa_header.image_type == 9) || (targa_header.image_type == 10) || (targa_header.image_type == 11));
	for (row = col = 0; row < rows; )
		{
		pixelcount = 0x10000;
		readpixelcount = 0x10000;

		if (compressed)
			{
			pixelcount = *buf_p++;
			if (pixelcount & 0x80)  // Run-length packet
				{
				readpixelcount = 1;
				}
			pixelcount = 1 + (pixelcount & 0x7F);
			}

		while (pixelcount-- && (row < rows))
			{
			if (readpixelcount-- > 0)
				{
				switch (targa_header.image_type)
					{
					// Colormapped image
					case 1:
					case 9:
						blue = *buf_p++;
						red = palette[blue][0];
						green = palette[blue][1];
						alpha = palette[blue][3];
						blue = palette[blue][2];
						if (alpha != 255) 
							{
							Img->flags |= IMAGE_HAS_ALPHA;
							}
						break;

					case 2:
					case 10:
						// 24 or 32 bit image
						blue = *buf_p++;
						green = *buf_p++;
						red = *buf_p++;
						alpha = 255;
						if (targa_header.pixel_size == 32)
							{
							alpha = *buf_p++;
							if (alpha != 255)
								{
								Img->flags |= IMAGE_HAS_ALPHA;
								}
							}
						break;

					case 3:
					case 11:
						// Greyscale image
						blue = green = red = *buf_p++;
						alpha = 255;
						break;
					}
				}

			if ((red != green) || (green != blue))
				{
				Img->flags |= IMAGE_HAS_COLOR;
				}

			*pixbuf++ = red;
			*pixbuf++ = green;
			*pixbuf++ = blue;
			*pixbuf++ = alpha;
			if (++col == columns)
				{
				// Run spans across rows
				row++;
				col = 0;
				pixbuf += row_inc;
				}
			}
		}

	// Успешно
	ITGA_EXIT (EXEC_OK)
	}

// Image_SaveTGA
sint Image_SaveTGA (schar *FileName, struct rgbdata_t *Img)
	{
	// Переменные
	slong	y,
			outsize,
			pixel_size;
	uchar	*bufend,
			*in,
			*buffer,
			*out;
	schar	*comment = " Generated by Xash ImageLib under RD_AAOW review \0";
	uchar	commentLen = strlen (comment);

	FILE	*FS;	

	// Контроль
	if (!Img || !FileName)
		{
		return EXEC_INVALID_PARAMETERS;
		}

	// Открытие файла и подготовка к записи
	BIC_INIT_FILE_WRITING

	outsize = Img->width * Img->height * ((Img->flags & IMAGE_HAS_ALPHA) ? 4 : 3) + 18 + commentLen;
	if ((buffer = (uchar *)malloc (outsize)) == NULL)
		{
		fclose (FS);
		return EXEC_MEMORY_ALLOC_FAIL;
		}
	memset (buffer, 0, 18);

	// Prepare header
	buffer[0] = commentLen;		// TGA comment length
	buffer[2] = 2;				// Uncompressed type
	buffer[12] = (Img->width >> 0) & 0xFF;
	buffer[13] = (Img->width >> 8) & 0xFF;
	buffer[14] = (Img->height >> 0) & 0xFF;
	buffer[15] = (Img->height >> 8) & 0xFF;
	buffer[16] = (Img->flags & IMAGE_HAS_ALPHA ) ? 32 : 24;
	buffer[17] = (Img->flags & IMAGE_HAS_ALPHA ) ? 8 : 0;	// 8 bits of alpha
	
	strncpy ((char *)buffer + 18, comment, commentLen); 
	out = buffer + 18 + commentLen;

	// Get image description
	switch (Img->type)
		{
		case PF_RGB_24:
		case PF_BGR_24:
			pixel_size = 3;
			break;

		case PF_RGBA_32:
		case PF_BGRA_32:
			pixel_size = 4;
			break;

		default:
			// Unsupported image type
			ITGA_EXIT (EXEC_INVALID_PARAMETERS)
		}

	switch (Img->type)
		{
		case PF_RGB_24:
		case PF_RGBA_32:
			// Swap rgba to bgra and flip upside down
			for (y = Img->height - 1; y >= 0; y--)
				{
				in = Img->buffer + y * Img->width * pixel_size;
				bufend = in + Img->width * pixel_size;
				for ( ; in < bufend; in += pixel_size)
					{
					*out++ = in[2];
					*out++ = in[1];
					*out++ = in[0];
					if (Img->flags & IMAGE_HAS_ALPHA)
						{
						*out++ = in[3];
						}
					}
				}
			break;

		case PF_BGR_24:
		case PF_BGRA_32:
			// Flip upside down
			for (y = Img->height - 1; y >= 0; y--)
				{
				in = Img->buffer + y * Img->width * pixel_size;
				bufend = in + Img->width * pixel_size;
				for ( ; in < bufend; in += pixel_size)
					{
					*out++ = in[0];
					*out++ = in[1];
					*out++ = in[2];
					if (Img->flags & IMAGE_HAS_ALPHA)
						{
						*out++ = in[3];
						}
					}
				}
			break;
		}	

	// Завершение
	fwrite (buffer, 1, outsize, FS);
	fclose (FS);
	ITGA_EXIT (EXEC_OK)
	}
