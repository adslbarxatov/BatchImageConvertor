// ������������ ��� Portable bitmap

// ��������� ���� �����������
enum PBMImageTypes
	{
	// ׸���-����� � ��������� �������������
	PBMBitmapAsText = 1,

	// ������� ������ � ��������� �������������
	PBMGreyscaleAsText = 2,

	// ������� � ��������� �������������
	PBMColorAsText = 3,

	// ׸���-����� � �������� �������������
	PBMBitmapAsBinary = 4,

	// ������� ������ � �������� �������������
	PBMGreyscaleAsBinary = 5,

	// ������� � �������� �������������
	PBMColorAsBinary = 6
	};

// ����������
sint PBM_LoadImage (schar *FileName, uint *Width, uint *Height, uchar **Buffer);
sint PBM_SaveImage (schar *FileName, uint Width, uint Height, uchar *Buffer, uchar ImageType);
