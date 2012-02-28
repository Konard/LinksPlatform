#ifndef __LINKS_TIMESTAMP_H__
#define __LINKS_TIMESTAMP_H__

#include "Common.h"

// Получить число 100-наносекундных интервалов от 1 января 1601 года. (+возможна небольшая коррекция +1 тик)
int64_t GetTimestamp();

#endif
