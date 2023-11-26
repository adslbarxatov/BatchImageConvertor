// Интерфейс подключения к библиотеке OpenJPEG2000

#include "BatchImageConvertor.h"
#include "JP2.h"

#define JP2_EXIT(c)				opj_destroy_codec (codec1); opj_destroy_codec (codec2); opj_stream_destroy (stream);\
								opj_image_destroy (image); return c;
#define JP2_W_EXIT(c)			opj_destroy_codec (codec);\
								for (k = 0; k < image->numcomps; k++)\
									free (image->comps[k].data);\
								free (image->comps);\
								free (image);\
								return c;


// Функция загружает изображение из файла
sint JP2_LoadImage (schar *FileName, uint *Width, uint *Height, uchar **Buffer)
	{
	// Переменные
	opj_dparameters_t parameters1, parameters2;
	opj_image_t *image = NULL;
	opj_codec_t *codec1 = opj_create_decompress (OPJ_CODEC_JP2);
	opj_codec_t *codec2 = opj_create_decompress (OPJ_CODEC_J2K);
	opj_stream_t *stream = NULL;

	uchar currentCodec = 1;
	uchar *buf;
	ulong w, h, k;

	// Контроль
	BIC_CHECK_INPUT_PARAMETERS

		// Настройка декодера
		opj_set_default_decoder_parameters (&parameters1);
	opj_set_default_decoder_parameters (&parameters2);

	if (!opj_setup_decoder (codec1, &parameters1) || !opj_setup_decoder (codec2, &parameters2))
		{
		opj_destroy_codec (codec1);
		opj_destroy_codec (codec2);
		return EXEC_INVALID_PARAMETERS;
		}

	// Чтение изображения
	if ((stream = opj_stream_create_default_file_stream (FileName, 1)) == NULL)
		{
		opj_destroy_codec (codec1);
		opj_destroy_codec (codec2);
		return EXEC_FILE_UNAVAILABLE;
		}

	if (!opj_read_header (stream, codec1, &image))
		{
		currentCodec++;
		opj_stream_destroy (stream);

		if ((stream = opj_stream_create_default_file_stream (FileName, 1)) == NULL)
			{
			opj_destroy_codec (codec1);
			opj_destroy_codec (codec2);
			return EXEC_FILE_UNAVAILABLE;
			}

		if (!opj_read_header (stream, codec2, &image))
			{
			opj_destroy_codec (codec1);
			opj_destroy_codec (codec2);
			opj_stream_destroy (stream);
			return EXEC_INVALID_FILE;
			}
		}

	// Контроль размеров и чтение тела
	*Width = image->x1 - image->x0;
	*Height = image->y1 - image->y0;
	if (BIC_IMAGE_IS_INVALID)
		{
		JP2_EXIT (EXEC_INVALID_FILE)
		}

	if ((currentCodec == 1) && (!opj_decode (codec1, stream, image) || !opj_end_decompress (codec1, stream)))
		{
		JP2_EXIT (EXEC_INVALID_FILE)
		}
	if ((currentCodec == 2) && (!opj_decode (codec2, stream, image) || !opj_end_decompress (codec2, stream)))
		{
		JP2_EXIT (EXEC_INVALID_FILE)
		}

	opj_destroy_codec (codec1);
	opj_destroy_codec (codec2);
	opj_stream_destroy (stream);

	// Подгонка параметров и цветовых пространств
	if (image->comps[0].data == NULL)
		{
		opj_image_destroy (image);
		return EXEC_INVALID_FILE;
		}

	if ((image->color_space != OPJ_CLRSPC_SYCC) &&
		(image->numcomps == 3) &&
		(image->comps[0].dx == image->comps[0].dy) &&
		(image->comps[1].dx != 1))
		{
		image->color_space = OPJ_CLRSPC_SYCC;
		}
	else if (image->numcomps <= 2)
		{
		image->color_space = OPJ_CLRSPC_GRAY;
		}

	if (image->color_space == OPJ_CLRSPC_SYCC)
		{
		color_sycc_to_rgb (image);
		}
	else if (image->color_space == OPJ_CLRSPC_CMYK)
		{
		color_cmyk_to_rgb (image);
		}
	else if (image->color_space == OPJ_CLRSPC_EYCC)
		{
		color_esycc_to_rgb (image);
		}

	switch (image->color_space)
		{
		case OPJ_CLRSPC_UNSPECIFIED:
		case OPJ_CLRSPC_SRGB:
			break;

		case OPJ_CLRSPC_GRAY:
			image = convert_gray_to_rgb (image);	// Возвращает NULL в случае ошибки
			break;

		default:
			opj_image_destroy (image);
			return EXEC_INVALID_FILE;
		}

	// Контроль результатов преобразований
	if (!image || (image->numcomps != 3) && (image->numcomps != 4))
		{
		opj_image_destroy (image);
		return EXEC_INVALID_FILE;
		}

	// Выгрузка данных
	if ((buf = (uchar *)malloc (*Width * *Height * 4)) == NULL)
		{
		opj_image_destroy (image);
		return EXEC_MEMORY_ALLOC_FAIL;
		}

	for (h = 0; h < *Height; h++)
		{
		for (w = 0; w < *Width; w++)
			{
			for (k = 0; k < image->numcomps; k++)
				{
				buf[(h * *Width + w) * 4 + k] = image->comps[k].data[h * *Width + w];
				}
			if (image->numcomps == 3)
				{
				buf[(h * *Width + w) * 4 + 3] = 255;
				}
			}
		}

	// Завершено
	opj_image_destroy (image);
	*Buffer = buf;
	return EXEC_OK;
	}

// Функция сохраняет изображение в файл
sint JP2_SaveImage (schar *FileName, uint Width, uint Height, uchar *Buffer, uchar CodecType)
	{
	// Переменные
	opj_cparameters_t parameters;
	opj_image_t *image = NULL;
	opj_codec_t *codec = NULL;
	opj_stream_t *stream = NULL;

	ulong w, h, k;

	// Контроль
	BIC_CHECK_OUTPUT_PARAMETERS;
	if ((CodecType < 1) || (CodecType > 2))
		{
		return EXEC_INVALID_PARAMETERS;
		}

	// Настройка энкодера
	opj_set_default_encoder_parameters (&parameters);

	if (CodecType == 1)
		codec = opj_create_compress (OPJ_CODEC_JP2);
	else
		codec = opj_create_compress (OPJ_CODEC_J2K);

	// Аллокация структуры изображения
	if ((image = (opj_image_t *)malloc (sizeof (opj_image_t))) == NULL)
		{
		return EXEC_MEMORY_ALLOC_FAIL;
		}

	// Сборка структуры изображения
	image->color_space = OPJ_CLRSPC_SRGB;
	image->icc_profile_buf = NULL;
	image->icc_profile_len = 0;
	image->numcomps = 3;		// Процедура записи альфа-канала в библиотеке OpenJPEG неисправна
	// Перенос выполняется полностью, но альфа-канал будет игнорироваться
	image->x0 = image->y0 = 0;
	image->x1 = Width;
	image->y1 = Height;

	if ((image->comps = (opj_image_comp_t *)malloc (sizeof (opj_image_comp_t) * image->numcomps)) == NULL)
		{
		free (image);
		return EXEC_MEMORY_ALLOC_FAIL;
		}

	for (k = 0; k < image->numcomps; k++)
		{
		image->comps[k].dx = image->comps[k].dy = 1;
		image->comps[k].x0 = image->comps[k].y0 = 0;
		image->comps[k].w = Width;
		image->comps[k].h = Height;
		image->comps[k].alpha = image->comps[k].bpp = image->comps[k].factor =
			image->comps[k].resno_decoded = image->comps[k].sgnd = 0;
		image->comps[k].prec = 8;

		if ((image->comps[k].data = (OPJ_INT32 *)malloc (sizeof (OPJ_INT32) * image->x1 * image->y1)) == NULL)
			{
			free (image->comps);
			free (image);
			return EXEC_MEMORY_ALLOC_FAIL;
			}
		}
	//image.comps[3].alpha = 1;

	// Перегонка
	for (h = 0; h < Height; h++)
		{
		for (w = 0; w < Width; w++)
			{
			for (k = 0; k < image->numcomps; k++)
				{
				image->comps[k].data[h * image->x1 + w] = Buffer[(h * image->x1 + w) * 4 + k];
				}
			}
		}

	// Запись
	if (!opj_setup_encoder (codec, &parameters, image))
		{
		JP2_W_EXIT (EXEC_INVALID_PARAMETERS);
		}

	if ((stream = opj_stream_create_default_file_stream (FileName, 0)) == NULL)
		{
		JP2_W_EXIT (EXEC_FILE_UNAVAILABLE);
		}

	if (!opj_start_compress (codec, image, stream) || !opj_encode (codec, stream) || !opj_end_compress (codec, stream))
		{
		opj_stream_destroy (stream);
		JP2_W_EXIT (EXEC_INVALID_PARAMETERS);
		}

	// Завершено
	opj_stream_destroy (stream);
	JP2_W_EXIT (EXEC_OK)
	}/**/
