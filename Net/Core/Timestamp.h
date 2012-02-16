#ifndef __LINKS_TIMESTAMP_H__
#define __LINKS_TIMESTAMP_H__

#ifdef _MFC_VER
long long GetTimestamp();
#endif

#ifdef __GNUC__
#include <stdint.h>
uint64_t GetTimestamp();
#endif

#endif
