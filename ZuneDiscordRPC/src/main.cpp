#include <iostream>
#include <Windows.h>
#include <string>
#include <fcntl.h>
#include <io.h>
#include <vector>
#include <sstream>
#include <discord-sdk/discord.h>
#include "Resources.h"
#include <thread>
#include <chrono>
#include <codecvt>

discord::Core* core{};
std::atomic<bool> shouldClose = false;

void SetDiscordPlaying(const std::string& title, const std::string& album, const std::string& artist) {
	discord::Activity activity{};
	std::string details = artist + " - " + title;
	activity.SetDetails(details.c_str());
	activity.SetState(album.c_str());
	activity.GetAssets().SetLargeImage("modern_zune_logo");

	core->ActivityManager().UpdateActivity(activity, [](discord::Result result) {
		if (result != discord::Result::Ok) {
			std::cerr << "Failed to update Discord Activity" << std::endl;
		}
	});
}

void SetDiscordStopped() {
	core->ActivityManager().ClearActivity([](discord::Result result) {
		if (result != discord::Result::Ok) {
			std::cerr << "Failed to update Discord Activity" << std::endl;
		}
	});
}

LRESULT CALLBACK WindowProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {
	switch (uMsg)
	{
	case WM_COPYDATA:  
		{
		COPYDATASTRUCT* data = (COPYDATASTRUCT*)lParam;
		std::wstring wideZuneMsg((wchar_t*)data->lpData, (size_t)data->cbData/2);
		std::string zuneMsg;

		std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> conv;
		zuneMsg = conv.to_bytes(wideZuneMsg);

		//Only look for messages comming from Zune
		if (zuneMsg.substr(0, 4) != "ZUNE") {
			return 0;
		}

		if (zuneMsg.length() == 22) {
			//Not Playing
			std::cout << "Not Playing" << std::endl;
			SetDiscordStopped();
		}
		else {
			//Playing
			std::stringstream tmp(zuneMsg);
			std::string segment;
			int index = 0;
		
			std::string artist;
			std::string title;
			std::string album;

			while (std::getline(tmp, segment, '\\')) {
				if (index == 4) {
					title = segment.erase(0, 1);
				}
				else if (index == 5) {
					artist = segment.erase(0, 1);
				}
				else if (index == 6) {
					album = segment.erase(0, 1);
				}

				index++;
			}	


			std::cout << "Artist: " << artist << std::endl;
			std::cout << "Title: " << title << std::endl;
			std::cout << "Album: " << album << std::endl;
			SetDiscordPlaying(title, album, artist);
		}
		}
		break;
	default:
		return DefWindowProc(hWnd, uMsg, wParam, lParam);
		break;
	}

	return 0;
}

void DiscordTick() {
	using namespace std::chrono_literals;
	while (!shouldClose) {
		core->RunCallbacks();
		std::this_thread::sleep_for(100ms);
	}
}

int main() {
	//Create Hidden Window to receive messges from Zune
	WNDCLASS wc = {};
	wc.lpszClassName = MSN_CLASS_NAME;
	wc.hInstance = HINST_THISCOMPONENT;
	wc.lpfnWndProc = WindowProc;
	RegisterClass(&wc);

	HWND hWnd = CreateWindow(MSN_CLASS_NAME, L"",WS_DISABLED, CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT, NULL, NULL, HINST_THISCOMPONENT, NULL);
	if (!hWnd) {
		std::cerr << "Error: Could not create window!" << std::endl;
		return -1;
	}
	std::cout << "Created MSN Fake Window!" << std::endl;

	//Init Discord
	auto result = discord::Core::Create(DS_CLIENT_ID, DiscordCreateFlags_Default, &core);
	if (result != discord::Result::Ok) {
		std::cerr << "Could not initialize discord rpc" << std::endl;
		return -1;
	}
	std::thread discordTickThread(DiscordTick);
	std::wcout << "Initialized Discord RPC!" << std::endl;
	
	UpdateWindow(hWnd);
	MSG msg = {};
	while (GetMessage(&msg, NULL, 0, 0) > 0) {
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	std::cout << "Shutting down..." << std::endl;
	shouldClose = true;
	discordTickThread.join();

	return 0;
}
