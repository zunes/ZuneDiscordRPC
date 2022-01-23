#pragma once

#ifndef HINST_THISCOMPONENT
EXTERN_C IMAGE_DOS_HEADER __ImageBase;
#define HINST_THISCOMPONENT ((HINSTANCE)&__ImageBase)
#endif

constexpr int64_t DS_CLIENT_ID = 904704369998561321;
constexpr wchar_t MSN_CLASS_NAME[] = L"MsnMsgrUIManager";
