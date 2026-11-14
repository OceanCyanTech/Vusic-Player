#pragma once
#include <stdint.h>

#ifdef VUSICNATIVE_EXPORTS
#define VUSIC_API __declspec(dllexport)
#else
#define VUSIC_API __declspec(dllimport)
#endif
//  OCEANCYAN TECH HEADER
//VUSIC API CORE - VERSION v1.0.0.0
//© OCEANCYAN TECH 2026

//HEADER FILE - MEDIA PROPERTIES FROM FFMPEG
//BUILD 0002

typedef struct
{
	int64_t duration_microseconds;
	int width;
	int height;
	int tracknumber;	
	int totaltracks;	
	int64_t bitrate;	
	int64_t channels;	
	int64_t samplerate;	
	int64_t filesize;	//
	char title[512]; //
	char artist[512]; //
	char lyricist[512];//
	char fileformat[512];
	char album[512];//
	char contributingartists[512];//
	char composer[512];//
	char copyright[512];//
	char publisher[512];//
	char date[512];//
	char creation_time[512];
	char performer[512];//
	char genre[512];//
	char comment[512];//
	char video_codec[64];
	char audio_codec[64];
	int64_t totalframes;
	uint8_t* cover_data;//
	int cover_size;//
} MediaProperties;
VUSIC_API int GetMediaProperties(const char* utf8_path, MediaProperties* out_info);
VUSIC_API void FreeMediaProperties(MediaProperties* info);