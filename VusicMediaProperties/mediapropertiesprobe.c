#include "mediapropertiesprobe.h"
#include<libavformat/avformat.h>
#include<libavformat/avio.h>
#include <string.h>
#include<libavutil/dict.h>

//  OCEANCYAN TECH HEADER
//VUSIC API CORE - VERSION v1.0.0.0
//© OCEANCYAN TECH 2026

//C FILE - MEDIA PROPERTIES FROM FFMPEG
//BUILD 0002

int GetMediaProperties(const char* filepath, MediaProperties* mediainfo)
{
	
	AVFormatContext* ctx = NULL;
	AVDictionaryEntry* metadata = NULL;
	memset(mediainfo, 0, sizeof(MediaProperties));
	if (mediainfo == NULL)return -1;
	int openresult = avformat_open_input(&ctx, filepath, NULL, NULL);
	if (openresult == 0)
	{
		int streaminfo = avformat_find_stream_info(ctx, NULL);
		if (streaminfo >= 0)
		{
			if (ctx->duration != AV_NOPTS_VALUE)
			{
				mediainfo->duration_microseconds = ctx->duration;
			}
			while ((metadata = av_dict_get(ctx->metadata, "", metadata, AV_DICT_IGNORE_SUFFIX)))
			{
				if (_stricmp(metadata->key, "album") == 0)
				{
					strncpy_s(mediainfo->album, sizeof(mediainfo->album), metadata->value, _TRUNCATE);
				}

				else 		if (_stricmp(metadata->key, "title") == 0)
				{
					strncpy_s(mediainfo->title, sizeof(mediainfo->title), metadata->value, _TRUNCATE);
				}
				else 		if (_stricmp(metadata->key, "album_artist") == 0)
				{
					strncpy_s(mediainfo->artist, sizeof(mediainfo->artist), metadata->value, _TRUNCATE);
				}
				else 		if (_stricmp(metadata->key, "artist") == 0)
				{
					strncpy_s(mediainfo->contributingartists, sizeof(mediainfo->contributingartists), metadata->value, _TRUNCATE);
				}
				else 		if (_stricmp(metadata->key, "genre") == 0)
				{
					strncpy_s(mediainfo->genre, sizeof(mediainfo->genre), metadata->value, _TRUNCATE);
				}
				else 		if (_stricmp(metadata->key, "composer") == 0)
				{
					strncpy_s(mediainfo->composer, sizeof(mediainfo->composer), metadata->value, _TRUNCATE);
				}
				else 		if (_stricmp(metadata->key, "copyright") == 0)
				{
					strncpy_s(mediainfo->copyright, sizeof(mediainfo->copyright), metadata->value, _TRUNCATE);
				}
				else 		if (_stricmp(metadata->key, "publisher") == 0)
				{
					strncpy_s(mediainfo->publisher, sizeof(mediainfo->publisher), metadata->value, _TRUNCATE);
				}
				else 		if (_stricmp(metadata->key, "comment") == 0)
				{
					strncpy_s(mediainfo->comment, sizeof(mediainfo->comment), metadata->value, _TRUNCATE);
				}
				else 		if (_stricmp(metadata->key, "performer") == 0)
				{
					strncpy_s(mediainfo->performer, sizeof(mediainfo->performer), metadata->value, _TRUNCATE);
				}
				else 		if (_stricmp(metadata->key, "date") == 0)
				{
					strncpy_s(mediainfo->date, sizeof(mediainfo->date), metadata->value, _TRUNCATE);
				}
				else 		if (_stricmp(metadata->key, "track") == 0)
				{
					strncpy_s(mediainfo->tracknumber, sizeof(mediainfo->tracknumber), metadata->value, _TRUNCATE);
				}
				else if (_stricmp(metadata->key, "lyricist") == 0 ||
					(_stricmp(metadata->key, "writer") == 0 && mediainfo->lyricist[0] == '\0'))
				{
					strncpy_s(mediainfo->lyricist, sizeof(mediainfo->lyricist), metadata->value, _TRUNCATE);
				}
			}
			for (unsigned int i = 0; i < ctx->nb_streams; i++) 
			{
				AVStream* avstream = ctx->streams[i];
				while ((metadata = av_dict_get(avstream->metadata, "", metadata, AV_DICT_IGNORE_SUFFIX)))
				{
					if (_stricmp(metadata->key, "title") == 0 && mediainfo->title[0] == '\0') 
					{
						strncpy_s(mediainfo->title, sizeof(mediainfo->title), metadata->value, _TRUNCATE);
					}
					else 	if (_stricmp(metadata->key, "artist") == 0 && mediainfo->artist[0] == '\0')
					{
						strncpy_s(mediainfo->artist, sizeof(mediainfo->artist), metadata->value, _TRUNCATE);
					}
					else 	if (_stricmp(metadata->key, "album") == 0 && mediainfo->album[0] == '\0')
					{
						strncpy_s(mediainfo->album, sizeof(mediainfo->album), metadata->value, _TRUNCATE);
					}
				}

				if (avstream->disposition & AV_DISPOSITION_ATTACHED_PIC) 
				{
				mediainfo->cover_data=	malloc(avstream->attached_pic.size);
				if (mediainfo->cover_data != NULL)
				{
					if (avstream->attached_pic.size > 0)
					{
						memcpy(mediainfo->cover_data, avstream->attached_pic.data, avstream->attached_pic.size);
						mediainfo->cover_size = avstream->attached_pic.size;
					}
				}
				}
				//BRANCH INTO VIDEO AND AUDIO
				if (avstream->codecpar->bit_rate > 0) 
				{

				}

			}
			int64_t filesize = 0;
			if (ctx->pb) 
			{
				if (avio_size(ctx)>0)
				{
					filesize = avio_size(ctx);
					mediainfo->filesize = filesize;
				}
			}
		}
		avformat_close_input(&ctx);
	}
	return openresult;
}
void FreeMediaProperties(MediaProperties *mediainfo)
{
	if (mediainfo != NULL && mediainfo->cover_data != NULL)
	{
		free(mediainfo->cover_data);
		mediainfo->cover_data = NULL;
		mediainfo->cover_size = 0;
	}
}
