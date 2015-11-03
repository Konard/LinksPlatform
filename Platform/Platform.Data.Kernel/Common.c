#include "Common.h"

signed_integer Error(char* message)
{
#ifdef DEBUG
    printf("%s\n\n", message);
#endif
    return ERROR_RESULT;
}

signed_integer ErrorWithCode(char* message, signed_integer errorCode)
{
#ifdef DEBUG
    printf("%s Error code: %" PRId64 ".\n\n", message, errorCode);
#endif
    return SUCCESS_RESULT == errorCode ? ERROR_RESULT : errorCode;
}

void DebugInfo(char* message)
{
#ifdef DEBUG
    printf("%s\n", message);
#endif
}
